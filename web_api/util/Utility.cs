using System;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/9/2019
 * 
 * Utility
 * 
 */

namespace WebApi.Util
{
    static class Utility
    {
        static public long CurrentMilliseconds()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc))).TotalMilliseconds;
        }
    }
}
