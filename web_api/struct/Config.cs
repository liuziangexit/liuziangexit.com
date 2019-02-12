using System;
using Newtonsoft.Json;
using WebApi.Util;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    8/29/2018
 * 
 * Config, DbConfig, Address, SqlSslModeEnum
 * 
 */

namespace WebApi.Struct
{
    struct Config
    {
        //http监听地址
        [JsonProperty("http_listen_address"), JsonConverter(typeof(AddressConverter))]
        public Address HttpListenAddress { get; set; }

        //https监听地址
        [JsonProperty("https_listen_address"), JsonConverter(typeof(AddressConverter))]
        public Address HttpsListenAddress { get; set; }

        //pfx证书路径，要求包含私钥
        [JsonProperty("https_pfx_certificate")]
        public string HttpsPfxCertificate { get; set; }

        //pfx证书密码
        [JsonProperty("https_pfx_certificate_password")]
        public string HttpsPfxCertificatePassword { get; set; }

        //为每个连接分配的读取缓冲区大小(字节)
        [JsonProperty("session_read_buffer_size")]
        public uint SessionReadBufferSize { get; set; }

        //服务端断开TCP连接前允许的最长无消息时间(秒)
        [JsonProperty("session_no_action_timeout")]
        public uint SessionNoActionTimeout { get; set; }

        //日志路径
        [JsonProperty("log_file")]
        public string LogFile { get; set; }

        //用于身份验证
        //pfx证书路径，不要求包含私钥
        [JsonProperty("verification_certificate")]
        public string VerificationCertificate { get; set; }

        //用于身份验证
        //pfx证书密码
        [JsonProperty("verification_certificate_password")]
        public string VerificationCertificatePassword { get; set; }

        //用于身份验证
        //最大允许的请求时间戳与服务端时间戳差距(秒)
        [JsonProperty("max_allowed_stamp_difference")]
        public long MaxAllowedStampDifference { get; set; }

        [JsonProperty("database")]
        public DbConfig Database { get; set; }
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

    public struct Address
    {

        public bool IsAvailable()
        {
            return IP != null && Port != 0;
        }

        [JsonProperty("ip")]
        public string IP { get; set; }

        [JsonProperty("port")]
        public UInt16 Port { get; set; }
    }

    enum SqlSslModeEnum
    {
        None,
        VerifyCA,//要求使用SSL。如果服务器不支持SSL，则连接将会失败。如果签发服务器证书的CA不受信任或与提供的CA证书不匹配，则连接将会失败。允许服务器证书“Host不匹配”
        VerifyFull//要求使用SSL。如果服务器不支持SSL，则连接将会失败。如果签发服务器证书的CA不受信任或与提供的CA证书不匹配，则连接将会失败。如果服务器证书“Host不匹配”，则连接将会失败
    }
}
