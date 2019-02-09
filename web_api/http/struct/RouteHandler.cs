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
        HttpResponse OnGet(HttpRequest r);
        HttpResponse OnPost(HttpRequest r);
    }
}
