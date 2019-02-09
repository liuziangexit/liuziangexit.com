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
    class ArticleHandler : RouteHandler
    {
        public HttpResponse OnGet(HttpRequest r)
        {
            return new HttpResponse { StatusCode = 200, Body = "GET Article" };
        }

        public HttpResponse OnPost(HttpRequest r)
        {
            return new HttpResponse { StatusCode = 200, Body = "POST Article" };
        }
    }
}