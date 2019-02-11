using HttpMachine;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WebApi.Http.Struct;
using static WebApi.Http.HttpRequestDispatcher;
using System.Net;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/9/2019
 * 
 * HttpRequestHandler
 * 
 */

namespace WebApi.Http.Handler
{
    class HttpRequestHandler : IHttpRequestParserDelegate
    {
        public HttpRequestHandler(HttpRequestProcessor processor)
        {
            this.Processor = processor;
        }

        public void OnMessageBegin(HttpParser parser)
        {
            Request = new HttpRequest();
        }

        public void OnBody(HttpParser parser, ArraySegment<byte> data)
        {
            string body = null;
            bool isUrlEncoded = false;
            if (Request.Headers.ContainsKey("Content-Type"))
            {
                if (Request.Headers["Content-Type"].IndexOf("application/x-www-form-urlencoded") != -1)
                    isUrlEncoded = true;
                var charsetPos = Request.Headers["Content-Type"].IndexOf("charset=");
                if (charsetPos != -1)
                {
                    var charsetName = Request.Headers["Content-Type"].Substring(
                        charsetPos + "charset=".Length);
                    body = Encoding.GetEncoding(charsetName).GetString(data.Array, data.Offset, data.Count);
                }
            }
            if (body == null)
                body = Encoding.UTF8.GetString(data.Array, data.Offset, data.Count);

            if (isUrlEncoded)
            {
                OnQueryString(parser, body);
                return;
            }

            Request.Body = body;
        }

        public void OnHeaderName(HttpParser parser, string name)
        {
            HeaderName = name;
        }

        public void OnHeaderValue(HttpParser parser, string value)
        {
            if (Request.Headers == null)
                Request.Headers = new SortedList<string, string>();
            Request.Headers.Add(this.HeaderName, value);
        }

        public void OnMethod(HttpParser parser, string method)
        {
            Request.Method = new HttpMethod(method);
        }

        public void OnPath(HttpParser parser, string path)
        {
            Request.Path = path;
        }

        public void OnQueryString(HttpParser parser, string queryString)
        {
            if (Request.QueryString == null)
                Request.QueryString = new SortedList<string, string>();

            var firstRound = queryString.Split('&');
            foreach (var pair in firstRound)
            {
                var secondRound = pair.Split('=');
                if (secondRound.Length == 2)
                    Request.QueryString.Add(WebUtility.UrlDecode(secondRound[0]), WebUtility.UrlDecode(secondRound[1]));
            }
        }

        public void OnMessageEnd(HttpParser parser)
        {
            Responses.Enqueue(this.Processor(this.Request));
        }

        public void OnRequestUri(HttpParser parser, string requestUri) { }

        public void OnFragment(HttpParser parser, string fragment) { }

        public void OnHeadersEnd(HttpParser parser) { }

        public Queue<HttpResponse> Responses = new Queue<HttpResponse>();

        private HttpRequest Request;
        private HttpRequestProcessor Processor;
        private string HeaderName;
    }
}
