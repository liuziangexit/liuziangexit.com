using HttpMachine;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WebApi.Http.Struct;
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
        public void OnMessageBegin(HttpParser parser)
        {
            CurrentRequest = new HttpRequest();
        }

        public void OnBody(HttpParser parser, ArraySegment<byte> data)
        {
            string body = null;
            bool isUrlEncoded = false;
            if (CurrentRequest.Headers.ContainsKey("Content-Type"))
            {
                if (CurrentRequest.Headers["Content-Type"].IndexOf("application/x-www-form-urlencoded") != -1)
                    isUrlEncoded = true;
                var charsetPos = CurrentRequest.Headers["Content-Type"].IndexOf("charset=");
                if (charsetPos != -1)
                {
                    var charsetName = CurrentRequest.Headers["Content-Type"].Substring(
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

            CurrentRequest.Body = body;
        }

        public void OnHeaderName(HttpParser parser, string name)
        {
            HeaderName = name;
        }

        public void OnHeaderValue(HttpParser parser, string value)
        {
            if (CurrentRequest.Headers == null)
                CurrentRequest.Headers = new SortedList<string, string>();
            CurrentRequest.Headers[this.HeaderName] = value;
        }

        public void OnMethod(HttpParser parser, string method)
        {
            CurrentRequest.Method = new HttpMethod(method);
        }

        public void OnPath(HttpParser parser, string path)
        {
            CurrentRequest.Path = path;
        }

        public void OnQueryString(HttpParser parser, string queryString)
        {
            if (CurrentRequest.QueryString == null)
                CurrentRequest.QueryString = new SortedList<string, string>();

            var firstRound = queryString.Split('&');
            foreach (var pair in firstRound)
            {
                var secondRound = pair.Split('=');
                if (secondRound.Length == 2)
                    CurrentRequest.QueryString[WebUtility.UrlDecode(secondRound[0])] = WebUtility.UrlDecode(secondRound[1]);
            }
        }

        public void OnMessageEnd(HttpParser parser)
        {
            Requests.Enqueue(CurrentRequest);
        }

        public void OnRequestUri(HttpParser parser, string requestUri) { }

        public void OnFragment(HttpParser parser, string fragment) { }

        public void OnHeadersEnd(HttpParser parser) { }

        public Queue<HttpRequest> Requests;

        private HttpRequest CurrentRequest;
        private string HeaderName;
    }
}
