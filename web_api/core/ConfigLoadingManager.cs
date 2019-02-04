using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using JsonErrorHandler = System.EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    8/28/2018
 * 
 * ConfigManager
 *  
 * 功能：
 * -向所有模块提供线程安全的配置文件接口
 * -监视磁盘上配置文件的修改，自动读取最新的配置文件
 * 
 */

namespace GameDbCache
{
    class ConfigLoadingManager
    {
        private ConfigLoadingManager()
        {
            Config = ReadConfig(delegate (Object o, Newtonsoft.Json.Serialization.ErrorEventArgs a)
            {
                Console.WriteLine("致命错误-首次读取配置失败");
                Environment.Exit(1);
            });
            RwLock = new ReaderWriterLockSlim();
            FileWatcher = new FileSystemWatcher();
            FileWatcher.Path = ".";
            FileWatcher.Filter = ConfigFileName;
            FileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            FileWatcher.Changed += new FileSystemEventHandler(OnConfigFileChanged);
            FileWatcher.EnableRaisingEvents = true;
        }

        static public ConfigLoadingManager GetInstance() => Lazy.Value;

        public WebApi.Config GetConfig()
        {
            RwLock.EnterReadLock();
            var returnMe = Config;
            RwLock.ExitReadLock();
            return returnMe;
        }

        private WebApi.Config ReadConfig(JsonErrorHandler onError)
        {
            try
            {
                return JsonConvert.DeserializeObject<WebApi.Config>(File.ReadAllText(ConfigFileName), new JsonSerializerSettings
                {
                    Error = onError
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void OnConfigFileChanged(object source, FileSystemEventArgs e)
        {
            bool isOk = true;
            RwLock.EnterWriteLock();
            while (true)
            {
                try
                {
                    var tmp = ReadConfig(delegate (Object o, Newtonsoft.Json.Serialization.ErrorEventArgs a)
                    {
                        isOk = false;
                    });
                    if (isOk)
                        Config = tmp;
                    break;
                }
                catch (IOException)
                {
                    Thread.Yield();
                }
                catch (Exception)
                {
                    isOk = false;
                    break;
                }
            }
            RwLock.ExitWriteLock();
            if (isOk)
                LogManager.GetInstance().LogAsync("信息-已重新读取配置");
            else
                LogManager.GetInstance().LogAsync("致命错误-无法重新读取配置");
        }

        private WebApi.Config Config;
        private ReaderWriterLockSlim RwLock;
        private FileSystemWatcher FileWatcher;

        static readonly private string ConfigFileName = "config.json";
        static private DateTime LastRead = DateTime.Now;

        private static readonly Lazy<ConfigLoadingManager> Lazy =
        new Lazy<ConfigLoadingManager>(() => new ConfigLoadingManager());
    }
}
