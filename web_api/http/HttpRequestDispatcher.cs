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
using System.Collections.Generic;
using WebApi.Core;

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

        public delegate HttpResponse ResponseMaker();

        public void Start(string ip, UInt16 port, uint readBufferSize, uint timeout, HttpRequestProcessor httpRequestHandler, ResponseMaker internalServerError)
        {
            this.Start(ip, port, readBufferSize, timeout, null, httpRequestHandler, internalServerError);
        }

        public void Start(string ip, UInt16 port, uint readBufferSize, uint timeout, X509Certificate2 certificate, HttpRequestProcessor httpRequestHandler, ResponseMaker internalServerError)
        {
            if (ConnectionAcceptor == null)
                ConnectionAcceptor = new TcpListener(IPAddress.Parse(ip), port);

            if (Sessions == null)
                Sessions = new ConcurrentDictionary<ulong, Session>();

            this.ReadBufferSize = readBufferSize;
            this.Timeout = timeout;
            this.HttpRequestHandler = httpRequestHandler;
            this.GetInternalServerError = internalServerError;
            this.Certificate = certificate;

            ConnectionAcceptor.Start();
            ConnectionAcceptor.BeginAcceptTcpClient(new AsyncCallback(OnAccept), this.ConnectionAcceptor);
        }

        public void Stop()
        {
            ConnectionAcceptor.Stop();
            foreach (var session in this.Sessions.Values)
                CloseSession(session);
        }

        //implementation

        private async void OnAccept(IAsyncResult ar)
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
                    await sslStream.AuthenticateAsServerAsync(this.Certificate, false, false);
                }
                catch (Exception ex)
                {
                    //ssl handshake failed, close tcp connection
                    sslStream.Close();
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

            Queue<HttpRequest> requestQueue = new Queue<HttpRequest>();
            Session session = new Session
            {
                SessionId = randomUInt64(),
                ReadBuffer = new byte[this.ReadBufferSize],
                Client = client,
                Stream = stream,
                Timeout = null,
                HttpState = new HttpParser(new HttpRequestHandler { Requests = requestQueue }),
                Requests = requestQueue
            };

            //keep session object reference
            while (!this.Sessions.TryAdd(session.SessionId, session))
            {
                session.SessionId = randomUInt64();
                Thread.Yield();
            }

            //start timer
            session.Timeout = new Timer((object state) => CloseSession(session), null,
                                        this.Timeout * 1000, System.Threading.Timeout.Infinite);

            //post an async READ operation            
            try
            {
                session.Stream.BeginRead(session.ReadBuffer, 0, session.ReadBuffer.Length, OnRead, session);
            }
            catch (Exception ex)
            {
                CloseSession(session);
                LogManager.GetInstance().LogAsync(ex);
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
            catch (ObjectDisposedException)
            {
                //session has been closed by timer
                return;
            }
            catch (Exception ex)
            {
                //io exception, session will be closed by the if section blow
                LogManager.GetInstance().LogAsync(ex);
            }

            if (bytesTransferred == 0)
            {
                //connection has been shuted down gracefully
                //or...
                //READ operation throws an exception
                CloseSession(session);
                return;
            }

            try
            {
                //pause timer
                session.Timeout.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }
            catch (ObjectDisposedException)
            {
                //session has been closed by timer
                return;
            }

            //parse http request(s)
            if (session.HttpState.Execute(new ArraySegment<byte>(session.ReadBuffer, 0, bytesTransferred)) != bytesTransferred)
            {
                //parsing error
                CloseSession(session);
                return;
            }

            //run logic
            Queue<HttpResponse> responses = new Queue<HttpResponse>(session.Requests.Count);
            while (session.Requests.Count != 0)
            {
                var request = session.Requests.Dequeue();
                try
                {
                    responses.Enqueue(HttpRequestHandler(request));
                }
                catch (Exception ex)
                {
                    //logic error
                    responses.Enqueue(GetInternalServerError());
                    LogManager.GetInstance().LogAsync(ex);
                }
            }

            try
            {
                //write response(s)
                while (responses.Count != 0)
                {
                    var response = responses.Dequeue();
                    await session.Stream.WriteAsync(response.SerializationToBytes());
                }
                //resume timer
                session.Timeout.Change(this.Timeout * 1000, System.Threading.Timeout.Infinite);
                //post an async READ operation
                session.Stream.BeginRead(session.ReadBuffer, 0, session.ReadBuffer.Length, OnRead, session);
            }
            catch (ObjectDisposedException)
            {
                //session has been closed by timer
                return;
            }
            catch (Exception ex)
            {
                CloseSession(session);
                LogManager.GetInstance().LogAsync(ex);
                return;
            }
        }

        private void CloseSession(Session session)
        {
            //stop timer
            //the example code provided by msdn shows that calling Timer.Dispose in Timer's callback is legal
            //https://docs.microsoft.com/en-us/dotnet/api/system.threading.timer.-ctor?#System_Threading_Timer__ctor_System_Threading_TimerCallback_
            session.Timeout.Dispose();

            //close Streams
            session.Stream.Close();

            //remove current session from session pool
            Session uselessOutHolder = null;
            this.Sessions.TryRemove(session.SessionId, out uselessOutHolder);
        }

        private HttpRequestProcessor HttpRequestHandler;
        private ResponseMaker GetInternalServerError;
        private uint ReadBufferSize, Timeout;
        private X509Certificate2 Certificate;
        private TcpListener ConnectionAcceptor;
        private ConcurrentDictionary<UInt64, Session> Sessions;

    }
}
