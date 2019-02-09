using System;
using System.Threading;

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
        static public int UTF8EncodedLength(string sequence)
        {
            int length = 0;
            for (int i = 0, len = sequence.Length; i < len; i++)
            {
                char ch = sequence[i];
                if (ch <= 0x7F)
                {
                    length++;
                }
                else if (ch <= 0x7FF)
                {
                    length += 2;
                }
                else if (Char.IsHighSurrogate(ch))
                {
                    length += 4;
                    ++i;
                }
                else
                {
                    length += 3;
                }
            }
            return length;
        }

    }
}
