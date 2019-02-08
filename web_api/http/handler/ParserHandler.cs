using HttpMachine;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WebApi.Http.Struct;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/9/2019
 * 
 * HttpRequestHandler, ProcessHttpRequest
 * 
 */

namespace WebApi.Http.Handler
{

    delegate HttpResponse ProcessHttpRequest(HttpRequest r);

    class HttpRequestHandler : IHttpRequestParserDelegate
    {

        public HttpRequestHandler(ProcessHttpRequest processor)
        {
            this.Processor = processor;
        }

        public void OnMessageBegin(HttpParser parser)
        {
            Request = new HttpRequest();
        }

        public void OnBody(HttpParser parser, ArraySegment<byte> data)
        {
            if (Request.Headers.ContainsKey("Content-Encoding"))
                Request.Body = Encoding.GetEncoding(Request.Headers["Content-Encoding"]).GetString(data.Array);
            else
                Request.Body = Encoding.UTF8.GetString(data.Array);
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
                    Request.QueryString.Add(secondRound[0], secondRound[1]);
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
        private ProcessHttpRequest Processor;
        private string HeaderName;

    }
}
