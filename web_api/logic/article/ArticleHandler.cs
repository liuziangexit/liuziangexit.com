using System;
using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.MySql;
using Newtonsoft.Json;
using WebApi.Core;
using WebApi.Http.Struct;
using WebApi.Logic.Article.Struct;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using GameDbCache;
using static WebApi.Logic.VerificationHelper;
using System.Data;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/9/2019
 * 
 * ArticleHandler
 *
 * ----------------------------------
 * HTTP GET /article/latest
 * 获取最新的文章信息
 *
 * QueryString: 
 * fetchCount(最多获取多少条结果，整形，非必需)
 *
 * Return:
 * 200 JsonArray<WebApi.Logic.Article.Struct.ArticleInfo>
 *
 * ----------------------------------
 * HTTP POST /article
 * 发布新文章(需验证)
 *
 * QueryString: 
 * id(整形，必须)
 * title(字符串，必须)
 * image(字符串，必须)
 * author(字符串，必须)
 * platform(字符串，必须)
 * intro(字符串，必须)
 * link(字符串，必须)
 *
 * Return:
 * 200 OK
 * 403 id已存在
 * 401 无法验证
 * 408 时间戳超出范围
 *
 * ----------------------------------
 * HTTP PUT /article
 * 修改文章(需验证)
 *
 * QueryString: 
 * id(整形，必须)
 * title(字符串，必须)
 * image(字符串，必须)
 * author(字符串，必须)
 * platform(字符串，必须)
 * intro(字符串，必须)
 * link(字符串，必须)
 *
 * Return:
 * 200 OK
 * 403 id不存在
 * 401 无法验证
 * 408 时间戳超出范围
 *
 * ----------------------------------
 * HTTP DELETE /article
 * 删除文章(需验证)
 *
 * QueryString: 
 * id(整形，必须)
 *
 * Return:
 * 200 OK
 * 403 id不存在
 * 401 无法验证
 * 408 时间戳超出范围
 * 
 */

namespace WebApi.Logic.Article
{
    class ArticleHandler : RouteHandler
    {
        public HttpResponse OnRequest(HttpRequest r)
        {
            switch (r.Path)
            {
                case "/article/latest":
                    {
                        if (r.Method != HttpMethod.Get)
                            return HttpResponse.NotImplemented;
                        return GetLatest(r);
                    }
                    break;
                case "/article":
                    {
                        //验证用证书
                        var verificationCertificate = new X509Certificate2(ConfigLoadingManager.GetInstance().GetConfig().VerificationCertificate,
                                    ConfigLoadingManager.GetInstance().GetConfig().VerificationCertificatePassword);
                        if (r.Method == HttpMethod.Post)
                            return NewArticleAsync(r, verificationCertificate);
                        if (r.Method == HttpMethod.Put)
                            return ReplaceArticle(r, verificationCertificate);
                        if (r.Method == HttpMethod.Delete)
                            return DeleteArticle(r, verificationCertificate);
                        return HttpResponse.NotImplemented;
                    }
                    break;
            }
            //never happen
            throw new Exception();
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
                {
                    var tmp = JsonConvert.DeserializeObject<ArticleInfo>(p.Info);
                    tmp.Id = p.Id;
                    tmp.Time = p.Time;
                    latestArticles.Add(tmp);
                }
            }

            return new HttpResponse
            {
                StatusCode = 200,
                Body = JsonConvert.SerializeObject(latestArticles, Formatting.Indented),
                Headers = new SortedList<string, string> { { "Content-Type", "application/json" } }
            };
        }

        private HttpResponse NewArticleAsync(HttpRequest r, X509Certificate2 c)
        {
            switch (VerifyRequest(r, c))
            {
                case Reasons.BadSignature: return HttpResponse.Unauthorized;
                case Reasons.BadTime: return HttpResponse.RequestTimeout;
                case Reasons.HeaderMissing: return HttpResponse.Unauthorized;
            }

            if (!r.QueryString.ContainsKey("id")
                || !r.QueryString.ContainsKey("title")
                || !r.QueryString.ContainsKey("image")
                || !r.QueryString.ContainsKey("time")
                || !r.QueryString.ContainsKey("author")
                || !r.QueryString.ContainsKey("platform")
                || !r.QueryString.ContainsKey("intro")
                || !r.QueryString.ContainsKey("link"))
                return HttpResponse.BadRequest;

            var newArticle = new ArticleInfo
            {
                Id = uint.Parse(r.QueryString["id"]),
                Title = r.QueryString["title"],
                Image = r.QueryString["image"],
                Time = long.Parse(r.QueryString["time"]),
                Author = r.QueryString["author"],
                Platform = r.QueryString["platform"],
                Intro = r.QueryString["intro"],
                Link = r.QueryString["link"]
            };
            var newDbElement = new ArticleTable
            {
                Id = newArticle.Id,
                Time = newArticle.Time,
                Info = JsonConvert.SerializeObject(newArticle)
            };

            using (DataConnection db = MySqlTools.CreateDataConnection(
            ConfigLoadingManager.GetInstance()
            .GetConfig().Database.GetConnectionString()))
            {
                try
                {
                    db.BeginTransaction(IsolationLevel.Serializable);

                    var query = (from p in db.GetTable<ArticleTable>()
                                 where p.Id == newDbElement.Id
                                 select p);
                    if (query.Count() != 0)
                        return HttpResponse.Forbidden;

                    db.Insert(newDbElement);
                }
                finally
                {
                    db.CommitTransaction();
                }
            }

            return HttpResponse.Ok;
        }

        private HttpResponse ReplaceArticle(HttpRequest r, X509Certificate2 c)
        {
            return null;
        }

        private HttpResponse DeleteArticle(HttpRequest r, X509Certificate2 c)
        {
            return null;
        }

        private Reasons VerifyRequest(HttpRequest r, X509Certificate2 c)
        {
            string signature = null;
            long timeStamp;
            if (!VerificationHelper.GetHeaders(r, out signature, out timeStamp))
                return Reasons.HeaderMissing;

            return VerificationHelper.VerifyRequest(ReadyForVerify(r.QueryString, timeStamp), signature, timeStamp,
                        ConfigLoadingManager.GetInstance().GetConfig().MaxAllowedStampDifference, c);
        }

        private string ReadyForVerify(SortedList<string, string> map, long timeStamp)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in map)
            {
                sb.Append(entry.Key);
                sb.Append(entry.Value);
            }
            sb.Append(timeStamp.ToString());
            return sb.ToString();
        }
    }
}