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
        public TcpClient Client;
        public SslStream SslStream;
        public HttpParser HttpState;
        public byte[] ReadBuffer;
    }
}
