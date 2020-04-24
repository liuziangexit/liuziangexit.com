using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.MySql;
using Newtonsoft.Json;
using WebApi.Core;
using WebApi.Http.Struct;
using WebApi.Logic.Article.Struct;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using static WebApi.Logic.VerificationHelper;
using System.Data;
using System.Net.Sockets;
using System.Net;

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
            return HttpResponse.BadRequest;
        }

        public void Stop()
        {
            this.toCompilerApi.Disconnect(true);
            this.toCompilerApi.Close();
        }

        //↓

        private InterpreterHandler()
        {
            bool v = connectToBackend();
            this.sendMessage("var a;return a;");
            string v1 = this.recvMessage();
            this.GetType();
        }

        private bool connectToBackend()
        {
            try
            {
                this.toCompilerApi.Connect(IPAddress.Parse("127.0.0.1"), ConfigLoadingManager.GetInstance().GetConfig().CompilerPort);
            }
            catch (SocketException)
            {
                return false;
            }
            return true;
        }

        private string sendToBackend(string source)
        {
            if (!this.toCompilerApi.Connected)
            {
                if (!this.connectToBackend())
                    return "liuziangexit.com无法连接到编译器";
            }
            //send request
            this.sendMessage(source);
            //read response
            return this.recvMessage();
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
        private Socket toCompilerApi = new Socket(SocketType.Stream, ProtocolType.Tcp);

        private static readonly Lazy<InterpreterHandler> Lazy =
               new Lazy<InterpreterHandler>(() => new InterpreterHandler());
    }
}