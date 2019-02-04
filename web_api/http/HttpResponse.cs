using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.Http
{
    class HttpResponse
    {
        public string SerializationToString()
        {
            return null;
        }

        public byte[] SerializationToBytes()
        {
            return null;
        }

        public UInt16 StatusCode;
        public IDictionary<string, string> Headers
        {
            get
            {
                if (mHeaders == null)
                    mHeaders = new SortedList<string, string>();
                return mHeaders;
            }
        }
        public string Body;

        private SortedList<string, string> mHeaders;
    }
}

