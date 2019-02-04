﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace src
{
    class HttpServerHelper
    {
        public void start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:80/");            
            listener.Start();
            var context = listener.GetContext();
            HttpListenerResponse response = context.Response;
            // Construct a response.
            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            // Get a response stream and write the response to it.
            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            // You must close the output stream.
            output.Close();
            listener.Stop();
        }

        public void stop()
        { }

        private HttpListener listener;
    }
}
