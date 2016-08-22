using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grapevine.Client;
using Grapevine.Server;


namespace QuantConnect.Messaging
{
    /// <summary>
    /// 
    /// </summary>
    public class DesktopClient
    {
        // Client for sending asynchronous requests.
        private static readonly RestClient _client = new RestClient();


    }

    /// <summary>
    /// 
    /// </summary>
    public class DesktopServer
    {
        // Client for sending asynchronous requests.
        private readonly RestServer _server;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        public DesktopServer(string port)
        {
            var settings = new ServerSettings
            {
                Port = port
            };

            _server = new RestServer(settings);
            _server.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsListening()
        {
            return _server.IsListening;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            _server.ThreadSafeStop();
        }

    }
}
