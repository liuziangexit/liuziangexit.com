using GameDbCache;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Http;

namespace WebApi
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigLoadingManager.GetInstance();
            HttpRequestDispatcher dispatcher = new HttpRequestDispatcher();
            dispatcher.Start();
            Console.Read();

            dispatcher.Stop();
            Console.Read();
        }
    }
}
