using HttpMachine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

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
        public byte[] ReadBuffer;

        //connection
        public TcpClient Client;
        public Stream Stream;

        //timeout handler
        public Timer Timeout;

        //http logic
        public HttpParser HttpState;
        public Queue<HttpRequest> Requests;
    }
}
