using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

/** 
* @author  liuziang
* @contact liuziang@liuziangexit.com
* @date    11/06/2018
* 
* QueryString构造器
* 
*/

namespace WebApi.Http
{
    class QueryStringBuilder
    {

        // 对外接口↓

        /**
         * 从Map<String,Object>创建一个QueryString，以指定的字符集编码
         * @exception IllegalArgumentException 当map不合法时
         * @exception UnsupportedCharsetException 当字符编码不受支持时
         */
        static public QueryStringBuilder CreateQueryStringFromMap(IDictionary<string, string> map)
        {
            if (map == null || map.Count == 0)
                throw new ArgumentException();

            bool isFirst = true;
            QueryStringBuilder builder = new QueryStringBuilder();
            foreach (var kv in map)
            {
                builder.AddParam(kv.Key, kv.Value, isFirst);
                isFirst = false;
            }
            return builder;
        }

        /**
         * 创建一个QueryString，并设定首个名值对，以指定的字符集编码
         * @exception IllegalArgumentException 当name为null或空时
         * @exception UnsupportedCharsetException 当字符编码不受支持时
         */
        static public QueryStringBuilder CreateQueryString(String name, String value)
        {
            QueryStringBuilder builder = new QueryStringBuilder();
            return builder.AddParam(name, value, true);
        }

        /**
         * 添加名值对，以指定的字符集编码
         * @exception IllegalArgumentException 当name为null或空时
         * @exception UnsupportedCharsetException 当字符编码不受支持时
         */
        public QueryStringBuilder AddParam(String name, String value)
        {
            return AddParam(name, value, false);
        }

        /**
         * 返回经过URL编码的QueryString
         */
        public override String ToString()
        {
            return sb.ToString();
        }

        // 实现↓

        private QueryStringBuilder() { }

        private QueryStringBuilder AddParam(String name, String value, bool isFirst)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name can not be null or empty");
            String encodedName = WebUtility.UrlEncode(name),
             encodedValue = WebUtility.UrlEncode(value);
            if (!isFirst)
                sb.Append('&');
            sb.Append(encodedName);
            sb.Append('=');
            sb.Append(encodedValue);
            return this;
        }

        private StringBuilder sb = new StringBuilder();

    }
}