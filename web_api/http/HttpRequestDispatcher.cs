using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace WebApi.Http
{
    class HttpRequestDispatcher
    {
        public void Start()
        { }

        public void Stop()
        { }

        private TcpListener ConnectionAcceptor;
        public IDictionary<string, RouteHandler> RouteHandlers
        {
            get
            {
                if (mRouteHandlers == null)
                    mRouteHandlers = new SortedDictionary<string, RouteHandler>();
                return mRouteHandlers;
            }
        }

        private SortedDictionary<string, RouteHandler> mRouteHandlers;

    }
}
