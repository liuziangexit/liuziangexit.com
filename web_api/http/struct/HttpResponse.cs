using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/5/2019
 * 
 * HttpResponse
 * 
 */

namespace WebApi.Http.Struct
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

        public UInt16 StatusCode;
        public SortedList<string, string> Headers;
        public string Body;

        static readonly public HttpResponse Ok = new HttpResponse
        {
            StatusCode = 200,
            Body = "<html><title>OK</title><body><center><h1>200 Ok</h1></center></body></html>",
            Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
        };

        static readonly public HttpResponse BadRequest = new HttpResponse
        {
            StatusCode = 400,
            Body = "<html><title>BAD REQUEST</title><body><center><h1>400 Bad Request</h1></center></body></html>",
            Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
        };
        static readonly public HttpResponse Unauthorized = new HttpResponse
        {
            StatusCode = 401,
            Body = "<html><title>UNAUTHORIZED</title><body><center><h1>401 Unauthorized</h1></center></body></html>",
            Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
        };
        static readonly public HttpResponse Forbidden = new HttpResponse
        {
            StatusCode = 403,
            Body = "<html><title>FORBIDDEN</title><body><center><h1>403 Forbidden</h1></center></body></html>",
            Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
        };
        static readonly public HttpResponse NotFound = new HttpResponse
        {
            StatusCode = 404,
            Body = "<html><title>NOT FOUND</title><body><center><h1>404 Not Found</h1></center></body></html>",
            Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
        };
        static readonly public HttpResponse RequestTimeout = new HttpResponse
        {
            StatusCode = 408,
            Body = "<html><title>REQUEST TIMEOUT</title><body><center><h1>408 Request Timeout</h1></center></body></html>",
            Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
        };
        static readonly public HttpResponse InternalServerError = new HttpResponse
        {
            StatusCode = 500,
            Body = "<html><title>INTERNAL SERVER ERROR</title><body><center><h1>500 Internal Server Error</h1></center></body></html>",
            Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
        };
        static readonly public HttpResponse NotImplemented = new HttpResponse
        {
            StatusCode = 501,
            Body = "<html><title>NOT IMPLEMENTED</title><body><center><h1>501 Not Implemented</h1></center></body></html>",
            Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
        };
    }
}

