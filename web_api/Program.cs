using GameDbCache;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using WebApi.Core;
using WebApi.Http;
using WebApi.Http.Struct;
using WebApi.Logic;
using WebApi.Logic.Article;

namespace WebApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigLoadingManager.GetInstance().GetConfig();

            SortedDictionary<string, RouteHandler> routeHandlers = new SortedDictionary<string, RouteHandler>();
            routeHandlers.Add("/article/latest", new ArticleHandler());

            ExecuteRouteHandler executeRouteHandler = new ExecuteRouteHandler { RouteHandlers = routeHandlers };

            HttpRequestDispatcher httpDispatcher = null;
            HttpRequestDispatcher httpsDispatcher = null;

            if (config.HttpListenAddress.isAvailable())
            {
                httpDispatcher = new HttpRequestDispatcher();
                httpDispatcher.Start(config.HttpListenAddress.IP, config.HttpListenAddress.Port,
                 config.SessionReadBufferSize, config.SessionNoActionTimeout,
                 executeRouteHandler.HttpRequestHandler);
                Console.WriteLine("Http Server - " + Environment.NewLine + config.HttpListenAddress.IP + ":" + config.HttpListenAddress.Port);
            }
            if (config.HttpsListenAddress.isAvailable())
            {
                httpsDispatcher = new HttpRequestDispatcher();
                httpsDispatcher.Start(config.HttpsListenAddress.IP, config.HttpsListenAddress.Port,
                 config.SessionReadBufferSize, config.SessionNoActionTimeout,
                  new X509Certificate2(config.HttpsPfxCertificate, config.HttpsPfxCertificatePassword),
                 executeRouteHandler.HttpRequestHandler);
                Console.WriteLine("Https Server - " + Environment.NewLine + config.HttpsListenAddress.IP + ":" + config.HttpsListenAddress.Port);
            }
            if (httpDispatcher == null && httpsDispatcher == null)
                return;
            LogManager.GetInstance().LogAsync("startup successfully");

            Console.WriteLine("press any key to shut down...");
            Console.ReadKey();

            if (httpDispatcher != null)
                httpDispatcher.Stop();
            if (httpsDispatcher != null)
                httpsDispatcher.Stop();

            LogManager.GetInstance().LogAsync("stopped");
            LogManager.GetInstance().Stop();
        }
    }
}
