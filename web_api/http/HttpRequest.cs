using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace WebApi.Http
{
    class HttpRequest
    {
        public HttpMethod Method;
        public string Path;
        public SortedList<string, string> Headers;
        public SortedList<string, string> QueryString;
        public string Body;
    }
}
