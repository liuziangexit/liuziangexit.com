using System;
using System.IO;
using System.Text;
using WebApi.Core;

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

namespace WebApi.Core
{
    class LogManager
    {
        public LogManager(string log)
        {
            tp = new FixedThreadPool(1);
            logfile = log;
        }

        public void LogAsync(string content)
        {
            var now = DateTime.Now;
            tp.Async(() =>
            {
                File.AppendAllText(this.logfile,
                now.ToString() + Environment.NewLine + content + Environment.NewLine + Environment.NewLine,
                Encoding.UTF8);
            });
        }

        public void LogAsync(Exception ex)
        {
            var now = DateTime.Now;
            tp.Async(() =>
            {
                File.AppendAllText(this.logfile,
                now.ToString() + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + Environment.NewLine,
                Encoding.UTF8);
            });
        }

        public void Stop()
        {
            this.tp.Stop();
        }

        private string logfile;
        private FixedThreadPool tp;
    }
}
