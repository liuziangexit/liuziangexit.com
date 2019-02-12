using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/6/2019
 * 
 * HttpRequest
 * 
 */

namespace WebApi.Http.Struct
{
    class HttpRequest
    {
        public string SerializationToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Method.Method);
            sb.Append(' ');
            sb.Append(Path);
            if (QueryString != null && QueryString.Count != 0)
            {
                sb.Append('?');
                sb.Append(QueryStringBuilder.CreateQueryStringFromMap(QueryString).ToString());
            }
            sb.Append(' ');
            sb.Append("HTTP/1.1");
            sb.Append("\r\n");
            if (Headers != null)
            {
                foreach (var p in Headers)
                {
                    sb.Append(p.Key);
                    sb.Append(": ");
                    sb.Append(p.Value);
                    sb.Append("\r\n");
                }
            }
            sb.Append("\r\n");
            sb.EnsureCapacity(sb.Length + Body.Length);
            sb.Append(Body);
            return sb.ToString();
        }

        public byte[] SerializationToBytes()
        {
            return Encoding.UTF8.GetBytes(SerializationToString());
        }

        public HttpMethod Method;
        public string Path;
        public SortedList<string, string> Headers;
        public SortedList<string, string> QueryString;
        public string Body;
    }
}
