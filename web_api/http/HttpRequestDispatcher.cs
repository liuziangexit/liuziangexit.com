using GameDbCache;
using HttpMachine;
using System;
using System.Collections.Generic;
using System.Net;
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

            Session session = new Session { Client = client, SslStream = new SslStream(client.GetStream()), HttpState = new HttpParser(new HttpRequestHandler()), ReadBuffer = new byte[4096] };

            //ssl handshake
            try
            {
                session.SslStream.AuthenticateAsServer(new X509Certificate(@"C:\Users\liuzi\Documents\GitHub\GameDbCache\certificate\server\server.pfx", ""), false, false);
            }
            catch (Exception ex)
            {
                try
                {
                    LogManager.GetInstance().LogAsync(ex.Message + "\r\n" + ex.StackTrace);
                    return;
                }
                finally
                {
                    client.Client.Disconnect(false);
                    client.Close();
                    session.SslStream.Close();
                }
            }

            session.SslStream.BeginRead(session.ReadBuffer, 0, session.ReadBuffer.Length, OnRead, session);
        }

        private void OnRead(IAsyncResult ar)
        {
            Session session = (Session)ar.AsyncState;
            int bytesTransferred = session.SslStream.EndRead(ar);
            if (bytesTransferred == 0)
                return;
            string look = Encoding.ASCII.GetString(session.ReadBuffer, 0, bytesTransferred);
            var writeMe = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n\r\nhihihi\r\n\r\n");
            session.SslStream.Write(writeMe, 0, writeMe.Length);
            //session.Client.Client.Disconnect(false);
            //session.Client.Close();
            // session.SslStream.Close();
            Console.Write("");
        }

        private class HttpRequestHandler : IHttpRequestParserDelegate
        {
            public void OnBody(HttpParser parser, ArraySegment<byte> data)
            {
            }

            public void OnFragment(HttpParser parser, string fragment)
            {
            }

            public void OnHeaderName(HttpParser parser, string name)
            {
            }

            public void OnHeadersEnd(HttpParser parser)
            {
            }

            public void OnHeaderValue(HttpParser parser, string value)
            {
            }

            public void OnMessageBegin(HttpParser parser)
            {
            }

            public void OnMessageEnd(HttpParser parser)
            {
            }

            public void OnMethod(HttpParser parser, string method)
            {
            }

            public void OnPath(HttpParser parser, string path)
            {
            }

            public void OnQueryString(HttpParser parser, string queryString)
            {
            }

            public void OnRequestUri(HttpParser parser, string requestUri)
            {
            }
        }

        private TcpListener ConnectionAcceptor;
        public IDictionary<string, RouteHandler> RouteHandlers
        {
            get
            {
                if (mRouteHandlers == null)
                    mRouteHandlers = new SortedDictionary<string, RouteHandler>();
                return mRouteHandlers;
            }
        }

        private SortedDictionary<string, RouteHandler> mRouteHandlers;

    }
}
