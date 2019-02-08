using GameDbCache;
using HttpMachine;
using System;
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

        public void Start()
        {
            if (ConnectionAcceptor == null)
            {
                var config = ConfigLoadingManager.GetInstance().GetConfig();
                ConnectionAcceptor = new TcpListener(IPAddress.Parse(config.Address.IP), config.Address.Port);
            }

            if (Sessions == null)
                Sessions = new ConcurrentDictionary<ulong, Session>();

            ConnectionAcceptor.Start();
            ConnectionAcceptor.BeginAcceptTcpClient(new AsyncCallback(HandleNewConnection), this.ConnectionAcceptor);
        }

        public void Stop()
        {
            ConnectionAcceptor.Stop();
        }

        //implementation

        private void HandleNewConnection(IAsyncResult ar)
        {
            //accept tcp connection
            TcpListener acceptor = (TcpListener)ar.AsyncState;
            TcpClient client;
            try
            {
                client = acceptor.EndAcceptTcpClient(ar);
                acceptor.BeginAcceptTcpClient(new AsyncCallback(HandleNewConnection), acceptor);
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
            SslStream sslStream = null;
            try
            {
                sslStream = new SslStream(client.GetStream());
                sslStream.AuthenticateAsServer(new X509Certificate(@"C:\Users\liuzi\Documents\GitHub\GameDbCache\certificate\server\server.pfx", ""), false, false);
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

            //keep objects reference
            Random random = new Random();
            Func<UInt64> randomUInt64 = () => {
                var buffer = new byte[sizeof(UInt64)];
                random.NextBytes(buffer);
                return BitConverter.ToUInt64(buffer, 0);
            };

            var httpHandler = new HttpRequestHandler(new ProcessHttpRequest(ExecuteRouteHandler));
            Session session = new Session
            {
                SessionId = randomUInt64(),
                Client = client,
                SslStream = sslStream,
                HttpHandler = httpHandler,
                HttpState = new HttpParser(httpHandler),
                ReadBuffer = new byte[4096]
            };
            while (!this.Sessions.TryAdd(session.SessionId, session))
            {
                session.SessionId = randomUInt64();
                Thread.Yield();
            }

            //post an async READ operation
            session.SslStream.BeginRead(session.ReadBuffer, 0, session.ReadBuffer.Length, OnRead, session);
        }

        private async void OnRead(IAsyncResult ar)
        {
            Session session = (Session)ar.AsyncState;
            int bytesTransferred = 0;
            try
            {
                bytesTransferred = session.SslStream.EndRead(ar);
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
                string look = response.SerializationToString();
                try
                {
                    await session.SslStream.WriteAsync(response.SerializationToBytes());
                }
                catch(Exception ex)
                {
                    LogManager.GetInstance().LogAsync(ex);
                }
            }

            session.SslStream.BeginRead(session.ReadBuffer, 0, session.ReadBuffer.Length, OnRead, session);
        }

        void CloseSession(Session session)
        {
            Session uselessOutHolder = null;
            this.Sessions.TryRemove(session.SessionId, out uselessOutHolder);

            session.Client.Client.BeginDisconnect(false, (IAsyncResult disconnectAr) =>
            {
                session.Client.Close();
                session.SslStream.Close();
            }, null);
        }

        HttpResponse ExecuteRouteHandler(HttpRequest r)
        {
            Console.Write("");
            return new HttpResponse
            {
                StatusCode = 200,
                Headers = new SortedList<string, string> { { "Server", "hihihi" }, { "Content-Length", "6" } },
                Body = "naive!"
            };
        }

        public SortedDictionary<string, RouteHandler> RouteHandlers;
        private TcpListener ConnectionAcceptor;
        private ConcurrentDictionary<UInt64, Session> Sessions;

    }
}
