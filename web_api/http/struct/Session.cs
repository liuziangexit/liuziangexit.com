using HttpMachine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using WebApi.Http.Handler;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/6/2019
 * 
 * Session
 * 
 */

namespace WebApi.Http.Struct
{
    class Session
    {
        public UInt64 SessionId;
        public TcpClient Client;
        public Stream Stream;
        public HttpParser HttpState;
        public HttpRequestHandler HttpHandler;
        public byte[] ReadBuffer;
    }
}
