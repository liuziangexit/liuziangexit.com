using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/6/2019
 * 
 * HttpRequest
 * 
 */

namespace WebApi.Http.Struct
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
