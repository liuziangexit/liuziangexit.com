using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebApi.Core;
using WebApi.Http.Struct;
using WebApi.Util;
using WebSocketSharp;
using WebSocketSharp.Server;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    5/3/2019
 * 
 * WebSocketService
 * 
 */

namespace WebApi.Logic
{
    class WebSocketService : WebSocketBehavior
    {
        public LogManager logger = null;

        protected override void OnOpen() { }

        protected override void OnClose(CloseEventArgs e) { }

        protected override void OnError(ErrorEventArgs e)
        {
            if (logger != null && e.Exception != null)
                logger.LogAsync(e.Exception);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            String response = null;
            if (e.IsText)
                response = "this is liuziang's websocket demo server, we are using UTF8 encoding, your message is: " + e.Data;
            else
                response = "this is liuziang's websocket demo server, we are using UTF8 encoding, currently we only support text websocket message";
            try
            {
                //encode as UTF-8
                this.Send(response);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogAsync(ex);
            }
        }
    }
}
