using System;
using System.Net;
using System.Threading;

namespace QuantConnect.Views
{
    public class SimpleHTTPServer
    {
        private Thread _serverThread;
        private HttpListener _listener;
        private int _port;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="port">Port of the server.</param>
        public SimpleHTTPServer(int port)
        {
            Initialize(port);
        }

        private void Initialize(int port)
        {
            _port = port;
            _serverThread = new Thread(Listen);
            _serverThread.Start();
        }



        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            try
            {
                //context.Request.InputStream

                //context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                //context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                //context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));


                //byte[] buffer = new byte[1024 * 16];
                //int nbytes;
                //while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                //    context.Response.OutputStream.Write(buffer, 0, nbytes);
                //input.Close();

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            context.Response.OutputStream.Close();
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

    }
}
