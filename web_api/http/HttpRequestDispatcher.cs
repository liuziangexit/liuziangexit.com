using GameDbCache;
using HttpMachine;
using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public void Start(string ip, UInt16 port, uint readBufferSize, SortedDictionary<string, RouteHandler> routeHandlers)
        {
            this.Start(ip, port, readBufferSize, null, routeHandlers);
        }

        public void Start(string ip, UInt16 port, uint readBufferSize, X509Certificate certificate, SortedDictionary<string, RouteHandler> routeHandlers)
        {
            if (ConnectionAcceptor == null)
                ConnectionAcceptor = new TcpListener(IPAddress.Parse(ip), port);

            if (Sessions == null)
                Sessions = new ConcurrentDictionary<ulong, Session>();

            this.ReadBufferSize = readBufferSize;
            this.RouteHandlers = routeHandlers;
            this.Certificate = certificate;

            ConnectionAcceptor.Start();
            ConnectionAcceptor.BeginAcceptTcpClient(new AsyncCallback(OnAccept), this.ConnectionAcceptor);
        }

        public void Stop()
        {
            ConnectionAcceptor.Stop();
            this.Sessions.Clear();
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
                LogManager.GetInstance().LogAsync(ex.Message + "\r\n" + ex.StackTrace);
                return;
            }
            catch (ObjectDisposedException)
            {
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
                        client.Close();
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

            //keep objects reference
            Random random = new Random();
            Func<UInt64> randomUInt64 = () =>
            {
                var buffer = new byte[sizeof(UInt64)];
                random.NextBytes(buffer);
                return BitConverter.ToUInt64(buffer, 0);
            };

            var httpHandler = new HttpRequestHandler(new ProcessHttpRequest(ExecuteRouteHandler));
            Session session = new Session
            {
                SessionId = randomUInt64(),
                Client = client,
                Stream = stream,
                HttpHandler = httpHandler,
                HttpState = new HttpParser(httpHandler),
                ReadBuffer = new byte[this.ReadBufferSize]
            };
            while (!this.Sessions.TryAdd(session.SessionId, session))
            {
                session.SessionId = randomUInt64();
                Thread.Yield();
            }

            //post an async READ operation
            session.Stream.BeginRead(session.ReadBuffer, 0, session.ReadBuffer.Length, OnRead, session);
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
                //remote host shuts down the connection gracefully
                //or...
                //READ operation throws an Exception
                CloseSession(session);
                return;
            }

            if (bytesTransferred != session.HttpState.Execute(new ArraySegment<byte>(session.ReadBuffer, 0, bytesTransferred)))
            {
                //HTTP Parser error
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

            session.Stream.BeginRead(session.ReadBuffer, 0, session.ReadBuffer.Length, OnRead, session);
        }

        void CloseSession(Session session)
        {
            Session uselessOutHolder = null;
            this.Sessions.TryRemove(session.SessionId, out uselessOutHolder);

            session.Client.Client.BeginDisconnect(false, (IAsyncResult disconnectAr) =>
            {
                session.Client.Close();
                session.Stream.Close();
            }, null);
        }

        HttpResponse ExecuteRouteHandler(HttpRequest r)
        {
            //TODO: call route handler, implement timeout
            Console.Write("");
            return new HttpResponse
            {
                StatusCode = 200,
                Headers = new SortedList<string, string> { { "Server", "hihihi" }, { "Content-Length", "6" } },
                Body = "naive!"
            };
        }

        private uint ReadBufferSize;
        X509Certificate Certificate;
        private SortedDictionary<string, RouteHandler> RouteHandlers;
        private TcpListener ConnectionAcceptor;
        private ConcurrentDictionary<UInt64, Session> Sessions;

    }
}
