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

namespace WebApi.Http.Struct
{
    interface RouteHandler
    {
        HttpResponse OnGet(IEnumerable<string> queryStringParams);
        HttpResponse OnPost(IEnumerable<string> queryStringParams, string body);
    }
}
