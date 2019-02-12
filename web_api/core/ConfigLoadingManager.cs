using System;
using System.IO;
using Newtonsoft.Json;
using JsonErrorHandler = System.EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    8/28/2018
 * 
 * ConfigLoadingManager
 *  
 * 功能：
 * -向所有模块提供线程安全的配置文件接口
 * -监视磁盘上配置文件的修改，自动读取最新的配置文件
 * 
 */

namespace WebApi.Core
{
    class ConfigLoadingManager
    {
        private ConfigLoadingManager()
        {
            Config = ReadConfig(delegate (Object o, Newtonsoft.Json.Serialization.ErrorEventArgs a)
            {
                //TODO
                Console.WriteLine("致命错误-首次读取配置失败");
                Environment.Exit(1);
            });
        }

        static public ConfigLoadingManager GetInstance() => Lazy.Value;

        public WebApi.Struct.Config GetConfig()
        {
            return Config;
        }

        private WebApi.Struct.Config ReadConfig(JsonErrorHandler onError)
        {
            try
            {
                return JsonConvert.DeserializeObject<WebApi.Struct.Config>(File.ReadAllText(ConfigFileName), new JsonSerializerSettings
                {
                    Error = onError
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private WebApi.Struct.Config Config;

        static readonly private string ConfigFileName = "config.json";

        private static readonly Lazy<ConfigLoadingManager> Lazy =
        new Lazy<ConfigLoadingManager>(() => new ConfigLoadingManager());
    }
}
