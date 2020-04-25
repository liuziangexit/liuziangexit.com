using System;
using WebApi.Core;
using WebApi.Http.Struct;
using System.Net.Http;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    4/24/2020
 * 
 * InterpreterHandler
 *
 * 在网页上使用我的编程语言
 * 
 */

namespace WebApi.Logic.Interpreter
{
    class InterpreterHandler : RouteHandler
    {
        static public InterpreterHandler GetInstance()
        {
            return InterpreterHandler.Lazy.Value;
        }

        public HttpResponse OnRequest(HttpRequest r)
        {
            if (r.Method != HttpMethod.Post)
            {
                return HttpResponse.BadRequest;
            }
            HttpResponse httpResponse = new HttpResponse();
            try
            {
                httpResponse.Body = this.sendSrcToBackend(r.Body);
            }
            catch (Exception)
            {
                this.closeConn();
                return HttpResponse.InternalServerError;
            }
            httpResponse.StatusCode = 200;
            httpResponse.Headers = new SortedList<string, string> { { "Content-Type", "text/plain" } };
            return httpResponse;
        }

        public void Stop()
        {
            this.closeConn();
        }

        //↓

        private InterpreterHandler()
        {
        }

        private void closeConn()
        {
            if (this.toCompilerApi != null)
            {
                if (this.toCompilerApi.Connected)
                    this.toCompilerApi.Disconnect(true);
                this.toCompilerApi.Close();
                this.toCompilerApi = null;
            }
        }

        private bool connectToBackend()
        {
            try
            {
                this.closeConn();
                this.toCompilerApi = new Socket(SocketType.Stream, ProtocolType.Tcp);
                this.toCompilerApi.Connect(IPAddress.Parse("127.0.0.1"), ConfigLoadingManager.GetInstance().GetConfig().CompilerPort);
            }
            catch (SocketException)
            {
                return false;
            }
            return true;
        }

        private string sendSrcToBackend(string source)
        {
            if (this.toCompilerApi == null || !this.toCompilerApi.Connected || !this.testConn())
            {
                if (!this.connectToBackend())
                    return "无法连接到编译器";
            }
            //send request
            this.sendMessage(source);
            //read response
            return this.recvMessage();
        }

        private bool testConn()
        {
            try
            {
                this.sendMessage("test");
                return this.recvMessage() == "test";
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void sendMessage(String str)
        {
            //sometimes naive
            int bodyLength = Encoding.UTF8.GetByteCount(str.AsSpan());
            byte[] message = new byte[4 + bodyLength];
            Encoding.UTF8.GetBytes(str.AsSpan(), new Span<byte>(message, 4, message.Length - 4));
            byte[] length = i32s(bodyLength);
            Array.Copy(length, message, 4);
            this.toCompilerApi.Send(message);
        }

        private string recvMessage()
        {
            byte[] recv = new byte[4];
            int v = this.toCompilerApi.Receive(recv);
            if (v != 4)
            {
                throw new Exception();
            }
            int length = i32d(recv);
            int toRecv = length;
            byte[] body = new byte[length];
            while (toRecv != 0)
            {
                toRecv -= this.toCompilerApi.Receive(body, length - toRecv, toRecv, SocketFlags.None);
            }
            if (toRecv < 0)
                throw new Exception("wtf");
            return Encoding.UTF8.GetString(body, 0, length);
        }

        // serialization 32 bit integer number to bytes
        static private byte[] i32s(int i)
        {
            byte[] result = new byte[4];
            for (int f = 0; f < 4; f++)
                result[f] = (byte)(i >> ((3 - f) * 8));
            return result;
        }

        // deserialization 32 bit integer number from bytes
        static private int i32d(byte[] i)
        {
            int result = 0;
            for (int f = 0; f < 4; f++)
                result |= ((0xFF & (int)i[f]) << ((3 - f) * 8));
            return result;
        }

        //fields
        private Socket toCompilerApi = null;

        private static readonly Lazy<InterpreterHandler> Lazy =
               new Lazy<InterpreterHandler>(() => new InterpreterHandler());
    }
}