using GameDbCache;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebApi.Http.Struct;
using WebApi.Util;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/11/2019
 * 
 * VerificationHelper
 * 
 * 自定义HTTP头：
 * Signature(对HTTP Body的ASCII表示的签名), TimeStamp(以秒为单位的客户端UTC时间戳)
 * 
 */

namespace WebApi.Logic
{
    static class VerificationHelper
    {

        public enum Reasons
        {
            Ok,//好
            BadSignature,//无法验证签名
            BadTime,//时间戳相差过大
            HeaderMissing//未提供必须的头部
        }

        static public bool GetHeaders(HttpRequest request, out string signature, out long timeStamp)
        {
            string clientSignature = null, clientTime = null;
            if (request.Headers == null
             || !request.Headers.TryGetValue("Signature", out clientSignature)
             || !request.Headers.TryGetValue("TimeStamp", out clientTime))
            {
                signature = null;
                timeStamp = 0;
                return false;
            }
            signature = clientSignature;
            timeStamp = long.Parse(clientTime);
            return true;
        }

        static public Reasons VerifyRequest(string content, string clientSignature, long clientTimeStamp, long maxAllowedStampDiff, X509Certificate2 certificate)
        {
            if (Math.Abs(Utility.CurrentMilliseconds() / 1000 - clientTimeStamp) > maxAllowedStampDiff)
                return Reasons.BadTime;

            if (clientSignature.Length % 2 != 0
                        || !SignUtil.Verify(content, clientSignature, certificate))
                return Reasons.BadSignature;

            return Reasons.Ok;
        }
    }
}
