using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace WebApi.Http
{
    class HttpResponse
    {
        public string SerializationToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("HTTP/1.1 ");
            sb.Append(StatusCode);
            sb.Append(' ');
            sb.Append(((HttpStatusCode)StatusCode).ToString());
            sb.Append("\r\n");
            if (mHeaders != null)
            {
                foreach(var p in mHeaders)
                {
                    sb.Append(p.Key);
                    sb.Append(": ");
                    sb.Append(p.Value);
                    sb.Append("\r\n");
                }
            }
            sb.Append("\r\n");
            sb.Append(Body);
            return null;
        }

        public byte[] SerializationToBytes()
        {
            return SerializationToBytes(Encoding.UTF8);
        }

        public byte[] SerializationToBytes(Encoding encoding)
        {
            return encoding.GetBytes(SerializationToString());
        }

        public UInt16 StatusCode;
        public IDictionary<string, string> Headers
        {
            get
            {
                if (mHeaders == null)
                    mHeaders = new SortedList<string, string>();
                return mHeaders;
            }
        }
        public string Body;

        private SortedList<string, string> mHeaders;
    }
}

