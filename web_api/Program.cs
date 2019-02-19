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
            try
            {
                RunServer();
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("crash_log.txt",
                ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + Environment.NewLine);
            }
        }

        static void RunServer()
        {
            var config = ConfigLoadingManager.GetInstance().GetConfig();

            SortedDictionary<string, RouteHandler> routeHandlers = new SortedDictionary<string, RouteHandler>();
            routeHandlers.Add("/article/latest", ArticleHandler.GetInstance());
            routeHandlers.Add("/article", ArticleHandler.GetInstance());

            SortedList<string, RouteHandler> regexRouteHandlers = new SortedList<string, RouteHandler>();
            regexRouteHandlers.Add("/article/\\d$", ArticleHandler.GetInstance());

            LogManager exceptionLogger = new LogManager(ConfigLoadingManager.GetInstance().GetConfig().ExceptionLogFile);
            LogManager accessLogger = new LogManager(ConfigLoadingManager.GetInstance().GetConfig().AccessLogFile);

            ExecuteRouteHandler executeRouteHandler = new ExecuteRouteHandler { RouteHandlers = routeHandlers, RegexRouteHandlers = regexRouteHandlers, ExceptionLogger = exceptionLogger, AccessLogger = accessLogger };

            HttpRequestDispatcher httpDispatcher = null;
            HttpRequestDispatcher httpsDispatcher = null;

            if (config.HttpListenAddress.IsAvailable())
            {
                httpDispatcher = new HttpRequestDispatcher();
                httpDispatcher.Start(config.HttpListenAddress.IP, config.HttpListenAddress.Port,
                 config.SessionReadBufferSize, config.SessionNoActionTimeout,
                 executeRouteHandler.HttpRequestHandler, executeRouteHandler.InternalServerError,
                 exceptionLogger);
                Console.WriteLine("Http Server - " + Environment.NewLine + config.HttpListenAddress.IP + ":" + config.HttpListenAddress.Port);
            }
            if (config.HttpsListenAddress.IsAvailable())
            {
                httpsDispatcher = new HttpRequestDispatcher();
                httpsDispatcher.Start(config.HttpsListenAddress.IP, config.HttpsListenAddress.Port,
                 config.SessionReadBufferSize, config.SessionNoActionTimeout,
                  new X509Certificate2(config.HttpsPfxCertificate, config.HttpsPfxCertificatePassword),
                 executeRouteHandler.HttpRequestHandler, executeRouteHandler.InternalServerError,
                 exceptionLogger);
                Console.WriteLine("Https Server - " + Environment.NewLine + config.HttpsListenAddress.IP + ":" + config.HttpsListenAddress.Port);
            }
            if (httpDispatcher == null && httpsDispatcher == null)
                return;

            exceptionLogger.LogAsync("startup successfully");
            Console.WriteLine("press any key to shut down...");
            Console.ReadKey();

            //stop dispatcher
            if (httpDispatcher != null)
                httpDispatcher.Stop();
            if (httpsDispatcher != null)
                httpsDispatcher.Stop();

            //stop logic
            ArticleHandler.GetInstance().Stop();

            exceptionLogger.LogAsync("stopped");

            //stop logger
            exceptionLogger.Stop();
            accessLogger.Stop();
        }
    }
}
