using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.Http
{
    class RouteHandler
    {
        public Func<IEnumerable<string>, HttpResponse> OnGet;
        public Func<IEnumerable<string>, string, HttpResponse> OnPost;
    }
}
