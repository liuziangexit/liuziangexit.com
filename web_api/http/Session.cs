using HttpMachine;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace WebApi.Http
{
    class Session
    {
        public UInt64 SessionId;
        public TcpClient Client;
        public SslStream SslStream;
        public HttpParser HttpState;
        public HttpRequestHandler HttpHandler;
        public byte[] ReadBuffer;
    }
}
