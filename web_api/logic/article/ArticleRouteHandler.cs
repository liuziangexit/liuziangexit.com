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
 * ArticleRouteHandler
 * 
 */

namespace WebApi.Logic.Article
{
    class ArticleRouteHandler : RouteHandler
    {
        public HttpResponse OnGet(IEnumerable<string> queryStringParams)
        {
            throw new NotImplementedException();
        }

        public HttpResponse OnPost(IEnumerable<string> queryStringParams, string body)
        {
            throw new NotImplementedException();
        }
    }
}