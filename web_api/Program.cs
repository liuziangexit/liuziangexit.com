using GameDbCache;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Core;
using WebApi.Http;

namespace WebApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigLoadingManager.GetInstance().GetConfig();

            HttpRequestDispatcher httpDispatcher = null;
            HttpRequestDispatcher httpsDispatcher = null;

            if (config.HttpListenAddress.isAvailable())
            {
                httpDispatcher = new HttpRequestDispatcher();
                httpDispatcher.Start(config.HttpListenAddress.IP, config.HttpListenAddress.Port,
                 config.SessionReadBufferSize, config.SessionNoActionTimeout,
                 null);
                Console.WriteLine("Http Server - " + Environment.NewLine + config.HttpListenAddress.IP + ":" + config.HttpListenAddress.Port);
            }
            if (config.HttpsListenAddress.isAvailable())
            {
                httpsDispatcher = new HttpRequestDispatcher();
                httpsDispatcher.Start(config.HttpsListenAddress.IP, config.HttpsListenAddress.Port,
                 config.SessionReadBufferSize, config.SessionNoActionTimeout,
                  new X509Certificate(config.HttpsPfxCertificate, config.HttpsPfxCertificatePassword),
                 null);
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

            LogManager.GetInstance().Stop();
        }
    }
}
