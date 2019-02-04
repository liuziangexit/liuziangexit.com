using System;
using Newtonsoft.Json;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    8/29/2018
 * 
 * Config
 * 
 */

namespace GameDbCache
{
    struct Config
    {
        [JsonProperty("database")]
        public DbConfig OutputServer { get; set; }

        [JsonProperty("worker_thread_count")]
        public int WorkerThreadCount { get; set; }

        [JsonProperty("log_file")]
        public string LogFile { get; set; }
    }

    struct DbConfig
    {
        public string GetConnectionString()
        {
            if (SqlSslMode == SqlSslModeEnum.None)
                return "server=" + IP + ";port=" + Port.ToString() + ";uid=" + Username + ";pwd=" + Password + ";database=" + Database + ";";

            string sslModeString = null;
            if (SqlSslMode == SqlSslModeEnum.VerifyCA)
                sslModeString = "VerifyCA";
            if (SqlSslMode == SqlSslModeEnum.VerifyFull)
                sslModeString = "VerifyFull";
            return "server=" + IP + ";port=" + Port.ToString() + ";uid=" + Username + ";pwd=" + Password + ";database=" + Database + ";" + "sslmode=" + sslModeString + ";CACertificateFile=" + SqlCaCertificate + ";";
        }

        [JsonProperty("ip")]
        public string IP { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("database")]
        public string Database { get; set; }

        [JsonProperty("sql_ca_certificate")]
        public string SqlCaCertificate { get; set; }

        [JsonProperty("sql_ssl_mode")]
        public SqlSslModeEnum SqlSslMode { get; set; }
    }

    enum SqlSslModeEnum
    {
        None,
        VerifyCA,//要求使用SSL。如果服务器不支持SSL，则连接将会失败。如果签发服务器证书的CA不受信任或与提供的CA证书不匹配，则连接将会失败。允许服务器证书“Host不匹配”
        VerifyFull//要求使用SSL。如果服务器不支持SSL，则连接将会失败。如果签发服务器证书的CA不受信任或与提供的CA证书不匹配，则连接将会失败。如果服务器证书“Host不匹配”，则连接将会失败
    }

}
