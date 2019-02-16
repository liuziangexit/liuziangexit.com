using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WebApi.Core;
using WebApi.Http;
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
            if (r.Path.EndsWith('/'))
                r.Path = r.Path.Remove(r.Path.Length - 1);

            RouteHandler handler = null;
            if (RouteHandlers == null || !RouteHandlers.TryGetValue(r.Path, out handler))
                return AddHeader(HttpResponse.NotFound);

            HttpResponse responseProducedByLogicHandler = null;
            try
            {
                responseProducedByLogicHandler = handler.OnRequest(r);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogAsync(ex);
                return AddHeader(HttpResponse.InternalServerError);
            }

            if (r.QueryString != null && r.QueryString.Count != 0)
                AccessLogger.LogAsync(r.Method + " " + r.Path + "?" + QueryStringBuilder.CreateQueryStringFromMap(r.QueryString).ToString()
                + Environment.NewLine + responseProducedByLogicHandler.StatusCode);
            else
                AccessLogger.LogAsync(r.Method + " " + r.Path
               + Environment.NewLine + responseProducedByLogicHandler.StatusCode);

            return AddHeader(responseProducedByLogicHandler);
        }

        public HttpResponse InternalServerError()
        {
            return AddHeader(HttpResponse.InternalServerError);
        }

        //------------------------------------------------------------------

        static private HttpResponse AddHeader(HttpResponse r)
        {
            if (r.Headers == null)
                r.Headers = new SortedList<string, string>();
            r.Headers["Content-Length"] = Encoding.UTF8.GetByteCount(r.Body).ToString();
            r.Headers["Connection"] = "keep-alive";
            r.Headers["Server"] = "liuziangWebServer/CSharp";
            r.Headers["Date"] = DateTime.Now.ToUniversalTime().ToString("r");
            if (r.Headers.ContainsKey("Content-Type"))
                r.Headers["Content-Type"] += "; charset=utf-8";
            else
                r.Headers["Content-Type"] = "text/plain; charset=utf-8";
            r.Headers["Access-Control-Allow-Origin"] = "*";
            return r;
        }

        public SortedDictionary<string, RouteHandler> RouteHandlers;
        public LogManager ExceptionLogger;
        public LogManager AccessLogger;
    }
}
