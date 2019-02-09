using System.Collections.Generic;
using System.Linq;
using LinqToDB.Data;
using LinqToDB.DataProvider.MySql;
using Newtonsoft.Json;
using WebApi.Core;
using WebApi.Http.Struct;
using WebApi.Logic.Article.Struct;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/9/2019
 * 
 * ArticleHandler
 *
 * ----------------------------------
 * HTTP GET /article/latest
 *
 * QueryString: 
 * fetchCount(NotNecessary)=number
 *
 * Return JsonArray<WebApi.Logic.Article.Struct.ArticleInfo>
 * 
 */

namespace WebApi.Logic.Article
{
    class ArticleHandler : RouteHandler
    {
        public HttpResponse OnGet(HttpRequest r)
        {
            switch (r.Path)
            {
                case "/article/latest": return GetLatest(r);
            }
            //never happen
            return null;
        }

        public HttpResponse OnPost(HttpRequest r)
        {
            return new HttpResponse { StatusCode = 501, Body = "please use GET" };
        }

        private HttpResponse GetLatest(HttpRequest r)
        {
            uint fetchCount = 12;
            if (r.QueryString != null && r.QueryString.ContainsKey("fetchCount"))
                uint.TryParse(r.QueryString["fetchCount"], out fetchCount);

            List<ArticleInfo> latestArticles = null;
            using (DataConnection db = MySqlTools.CreateDataConnection(
                ConfigLoadingManager.GetInstance()
                .GetConfig().Database.GetConnectionString()))
            {
                var query = from p in db.GetTable<ArticleTable>()
                            orderby p.Time descending
                            select p;
                var queryResult = query.Take((int)fetchCount);

                latestArticles = new List<ArticleInfo>(queryResult.Count());
                foreach (var p in queryResult)
                    latestArticles.Add(JsonConvert.DeserializeObject<ArticleInfo>(p.Info));
            }

            return new HttpResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(latestArticles, Formatting.Indented),
                Headers = new SortedList<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}