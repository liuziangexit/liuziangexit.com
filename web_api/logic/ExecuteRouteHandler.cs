using GameDbCache;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WebApi.Http.Struct;
using WebApi.Util;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/10/2019
 * 
 * ExecuteRouteHandler
 * 
 */

namespace WebApi.Logic
{
    class ExecuteRouteHandler
    {
        public HttpResponse HttpRequestHandler(HttpRequest r)
        {
            if (r.Method != HttpMethod.Get && r.Method != HttpMethod.Post)
                return AddHeader(new HttpResponse
                {
                    StatusCode = 501,
                    Body = "<html><title>NOT IMPLEMENTED</title><body><center><h1>501 Not Implemented</h1></center></body></html>",
                    Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
                });

            if (r.Path.EndsWith('/'))
                r.Path = r.Path.Remove(r.Path.Length - 1);

            RouteHandler handler = null;
            if (RouteHandlers == null || !RouteHandlers.TryGetValue(r.Path, out handler))
            {
                return AddHeader(new HttpResponse
                {
                    StatusCode = 404,
                    Body = "<html><title>NOT FOUND</title><body><center><h1>404 Not Found</h1></center></body></html>",
                    Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
                });
            }

            HttpResponse responseProducedByLogicHandler = null;
            try
            {
                if (r.Method == HttpMethod.Get)
                    responseProducedByLogicHandler = handler.OnGet(r);
                else if (r.Method == HttpMethod.Post)
                    responseProducedByLogicHandler = handler.OnPost(r);
            }
            catch (Exception ex)
            {
                LogManager.GetInstance().LogAsync(ex);
                return AddHeader(new HttpResponse
                {
                    StatusCode = 500,
                    Body = "<html><title>INTERNAL SERVER ERROR</title><body><center><h1>500 Internal Server Error</h1></center></body></html>",
                    Headers = new SortedList<string, string> { { "Content-Type", "text/html" } }
                });
            }

            return AddHeader(responseProducedByLogicHandler);
        }

        static private HttpResponse AddHeader(HttpResponse r)
        {
            if (r.Headers == null)
                r.Headers = new SortedList<string, string>();
            r.Headers["Content-Length"] = Encoding.UTF8.GetByteCount(r.Body).ToString();
            r.Headers["Connection"] = "keep-alive";
            r.Headers["Server"] = "liuziangWebServer/CSharp";
            r.Headers["Date"] = DateTime.Now.ToUniversalTime().ToString("r");
            r.Headers["Connection"] = "keep-alive";
            if (r.Headers.ContainsKey("Content-Type"))
                r.Headers["Content-Type"] += "; charset=utf-8";
            else
                r.Headers["Content-Type"] = "text/plain; charset=utf-8";
            r.Headers["Access-Control-Allow-Origin"] = "*";
            return r;
        }

        public SortedDictionary<string, RouteHandler> RouteHandlers;
    }
}
