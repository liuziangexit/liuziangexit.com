using System;
using System.IO;
using System.Text;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    9/7/2018
 * 
 * LogManager
 *  
 * 功能：
 * -提供异步的写日志文件接口
 * -当日志文件名配置被修改后，将会向新的文件写入日志
 * -日志文件编码为UTF-8
 * 
 */

namespace GameDbCache
{
    class LogManager
    {

        static public LogManager GetInstance() => Lazy.Value;

        public void LogAsync(string content)
        {
            File.AppendAllTextAsync(ConfigLoadingManager.GetInstance().GetConfig().LogFile,
               DateTime.Now.ToString() + Environment.NewLine + content + Environment.NewLine + Environment.NewLine,
               Encoding.UTF8);
        }

        private static readonly Lazy<LogManager> Lazy =
                new Lazy<LogManager>(() => new LogManager());
    }
}
