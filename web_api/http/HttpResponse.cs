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
            if (Headers != null)
            {
                foreach(var p in Headers)
                {
                    sb.Append(p.Key);
                    sb.Append(": ");
                    sb.Append(p.Value);
                    sb.Append("\r\n");
                }
            }
            sb.Append("\r\n");
            // sb.EnsureCapacity(sb.Capacity+)
            sb.Append(Body);
            return sb.ToString();
        }

        public byte[] SerializationToBytes()
        {
            return Encoding.UTF8.GetBytes(SerializationToString());
        }

        public UInt16 StatusCode;
        public SortedList<string, string> Headers;
        public string Body;
    }
}

