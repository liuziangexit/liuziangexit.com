using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.Http
{
    class RouteHandler
    {
        public delegate HttpResponse OnGet(IEnumerable<string> queryStringParams);
        public delegate HttpResponse OnPost(IEnumerable<string> queryStringParams, string body);
    }
}
