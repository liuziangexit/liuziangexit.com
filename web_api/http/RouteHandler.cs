using System;
using System.Collections.Generic;
using System.Text;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/5/2019
 * 
 * RouteHandler
 * 
 */

namespace WebApi.Http
{
    class RouteHandler
    {
        public delegate HttpResponse OnGet(IEnumerable<string> queryStringParams);
        public delegate HttpResponse OnPost(IEnumerable<string> queryStringParams, string body);
    }
}
