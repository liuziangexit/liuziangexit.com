using GameDbCache;
using HttpMachine;
using System;
using System.IO;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using WebApi.Http.Struct;
using WebApi.Http.Handler;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/5/2019
 * 
 * HttpRequestDispatcher
 * 
 */

namespace WebApi.Http
{
    class HttpRequestDispatcher
    {

        //interface

        public delegate HttpResponse HttpRequestProcessor(HttpRequest r);

        public void Start(string ip, UInt16 port, uint readBufferSize, uint timeout, HttpRequestProcessor httpRequestHandler)
        {
            this.Start(ip, port, readBufferSize, timeout, null, httpRequestHandler);
        }

        public void Start(string ip, UInt16 port, uint readBufferSize, uint timeout, X509Certificate2 certificate, HttpRequestProcessor httpRequestHandler)
        {
            if (ConnectionAcceptor == null)
                ConnectionAcceptor = new TcpListener(IPAddress.Parse(ip), port);

            if (Sessions == null)
                Sessions = new ConcurrentDictionary<ulong, Session>();

            this.ReadBufferSize = readBufferSize;
            this.Timeout = timeout;
            this.HttpRequestHandler = httpRequestHandler;
            this.Certificate = certificate;

            ConnectionAcceptor.Start();
            ConnectionAcceptor.BeginAcceptTcpClient(new AsyncCallback(OnAccept), this.ConnectionAcceptor);
        }

        public void Stop()
        {
            ConnectionAcceptor.Stop();
            foreach (var session in this.Sessions.Values)
                RaiseCloseSession(session);
        }

        //implementation

        private void OnAccept(IAsyncResult ar)
        {
            //accept tcp connection
            TcpListener acceptor = (TcpListener)ar.AsyncState;
            TcpClient client;
            try
            {
                client = acceptor.EndAcceptTcpClient(ar);
                acceptor.BeginAcceptTcpClient(new AsyncCallback(OnAccept), acceptor);
            }
            catch (SocketException ex)
            {
                LogManager.GetInstance().LogAsync(ex);
                return;
            }
            catch (ObjectDisposedException)
            {
                //acceptor closed
                return;
            }

            //establish ssl connection
            Stream stream;
            if (this.Certificate != null)
            {
                SslStream sslStream = null;
                try
                {
                    sslStream = new SslStream(client.GetStream());
                    sslStream.AuthenticateAsServer(this.Certificate, false, false);
                }
                catch (Exception ex)
                {
                    //ssl handshake failed, close tcp connection
                    client.Client.BeginDisconnect(false, (IAsyncResult disconnectAr) =>
                    {
                        sslStream.Close();
                    }, null);
                    LogManager.GetInstance().LogAsync(ex);
                    return;
                }
                //handshake successful
                stream = sslStream;
            }
            else
            {
                stream = client.GetStream();
            }

            //construct session object
            Random random = new Random();
            Func<UInt64> randomUInt64 = () =>
            {
                var buffer = new byte[sizeof(UInt64)];
                random.NextBytes(buffer);
                return BitConverter.ToUInt64(buffer, 0);
            };

            var httpHandler = new HttpRequestHandler(new HttpRequestProcessor(this.HttpRequestHandler));
            Session session = new Session
            {
                SessionId = randomUInt64(),
                ReadBuffer = new byte[this.ReadBufferSize],
                Client = client,
                Stream = stream,
                Timeout = null,
                HttpState = new HttpParser(httpHandler),
                HttpHandler = httpHandler
            };

            session.Timeout = new Timer((object state) => RaiseCloseSession(session), null,
                                         System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            //keep session object reference
            while (!this.Sessions.TryAdd(session.SessionId, session))
            {
                session.SessionId = randomUInt64();
                Thread.Yield();
            }

            //start timer
            session.Timeout.Change(this.Timeout * 1000, System.Threading.Timeout.Infinite);

            //post an async READ operation
            try
            {
                session.Stream.BeginRead(session.ReadBuffer, 0, session.ReadBuffer.Length, OnRead, session);
            }
            catch (Exception ex)
            {
                CloseSession(session);
                LogManager.GetInstance().LogAsync(ex);
                return;
            }
        }

        private async void OnRead(IAsyncResult ar)
        {
            Session session = (Session)ar.AsyncState;
            int bytesTransferred = 0;
            try
            {
                bytesTransferred = session.Stream.EndRead(ar);
            }
            catch (Exception ex)
            {
                LogManager.GetInstance().LogAsync(ex);
            }

            if (bytesTransferred == 0)
            {
                //connection has been shuted down gracefully
                //or...
                //READ operation throws an Exception
                CloseSession(session);
                return;
            }

            //pause timer
            session.Timeout.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);

            try
            {
                if (bytesTransferred != session.HttpState.Execute(new ArraySegment<byte>(session.ReadBuffer, 0, bytesTransferred)))
                {
                    //HTTP Parser error
                    CloseSession(session);
                    return;
                }
            }
            catch (Exception ex)
            {
                //logic error
                LogManager.GetInstance().LogAsync(ex);
                CloseSession(session);
                return;
            }

            while (session.HttpHandler.Responses.Count != 0)
            {
                var response = session.HttpHandler.Responses.Dequeue();
                try
                {
                    await session.Stream.WriteAsync(response.SerializationToBytes());
                }
                catch (Exception ex)
                {
                    LogManager.GetInstance().LogAsync(ex);
                }
            }

            //post an async READ operation
            try
            {
                session.Stream.BeginRead(session.ReadBuffer, 0, session.ReadBuffer.Length, OnRead, session);
            }
            catch (Exception)
            {
                CloseSession(session);
                return;
            }

            //restart timer
            session.Timeout.Change(this.Timeout * 1000, System.Threading.Timeout.Infinite);
        }

        private void CloseSession(Session session)
        {
            //stop timer
            session.Timeout.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            //remove from session pool
            Session uselessOutHolder = null;
            this.Sessions.TryRemove(session.SessionId, out uselessOutHolder);
            //close connection and release un-managed resources
            session.Client.Client.BeginDisconnect(false, (IAsyncResult disconnectAr) =>
            {
                session.Timeout.Dispose();
                session.Stream.Close();
            }, null);
        }

        private void RaiseCloseSession(Session session)
        {
            //stop timer
            session.Timeout.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            //close the underlying TCP connection
            session.Client.Client.BeginDisconnect(false, (IAsyncResult ar) => { }, null);
        }

        private HttpRequestProcessor HttpRequestHandler;
        private uint ReadBufferSize, Timeout;
        private X509Certificate2 Certificate;
        private TcpListener ConnectionAcceptor;
        private ConcurrentDictionary<UInt64, Session> Sessions;

    }
}
