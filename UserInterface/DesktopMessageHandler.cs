using System.IO;
using System.Net;
using System.Threading;
using Grapevine;
using Grapevine.Client;
using Grapevine.Server;
using Newtonsoft.Json;
using QuantConnect.Packets;

namespace QuantConnect.Views
{
    public class DesktopMessageHandler
    {
        private readonly DesktopServer _server;
        private readonly DesktopClient _client;

        public DesktopMessageHandler(string serverPort, string clientPort)
        {
            // Start server
            _server = new DesktopServer(this);
            _server.StartServer(serverPort);

            _client = new DesktopClient(clientPort);
        }

        #region Delegates and Events
        // Create Events and delegates that mirror EventMessagingHandler
        public delegate void DebugEventRaised(DebugPacket packet);
        public event DebugEventRaised DebugEvent;

        public delegate void LogEventRaised(LogPacket packet);
        public event LogEventRaised LogEvent;

        public delegate void RuntimeErrorEventRaised(RuntimeErrorPacket packet);
        public event RuntimeErrorEventRaised RuntimeErrorEvent;

        public delegate void HandledErrorEventRaised(HandledErrorPacket packet);
        public event HandledErrorEventRaised HandledErrorEvent;

        public delegate void BacktestResultEventRaised(BacktestResultPacket packet);
        public event BacktestResultEventRaised BacktestResultEvent;

        public delegate void ConsumerReadyEventRaised();
        public event ConsumerReadyEventRaised ConsumerReadyEvent;
        #endregion

        #region Delegates
        /// <summary>
        /// Raise a debug event safely
        /// </summary>
        public virtual void OnDebugEvent(DebugPacket packet)
        {
            var handler = DebugEvent;

            if (handler != null)
            {
                handler(packet);
            }
        }

        /// <summary>
        /// Handler for consumer ready code.
        /// </summary>
        public virtual void OnConsumerReadyEvent()
        {
            var handler = ConsumerReadyEvent;
            if (handler != null)
            {
                handler();
            }
        }

        /// <summary>
        /// Raise a log event safely
        /// </summary>
        public virtual void OnLogEvent(LogPacket packet)
        {
            var handler = LogEvent;
            if (handler != null)
            {
                handler(packet);
            }
        }

        /// <summary>
        /// Raise a handled error event safely
        /// </summary>
        public virtual void OnHandledErrorEvent(HandledErrorPacket packet)
        {
            var handler = HandledErrorEvent;
            if (handler != null)
            {
                handler(packet);
            }
        }

        /// <summary>
        /// Raise runtime error safely
        /// </summary>
        public virtual void OnRuntimeErrorEvent(RuntimeErrorPacket packet)
        {
            var handler = RuntimeErrorEvent;
            if (handler != null)
            {
                handler(packet);
            }
        }

        /// <summary>
        /// Raise a backtest result event safely.
        /// </summary>
        public virtual void OnBacktestResultEvent(BacktestResultPacket packet)
        {
            var handler = BacktestResultEvent;
            if (handler != null)
            {
                handler(packet);
            }
        }
        #endregion

        public void ConsumerReady()
        {
            _client.ConsumerReady();
        }

        public void SendEnqueuedPackets()
        {
            _client.SendEnqueuedPackets();
        }
    }

    #region Client
    public class DesktopClient
    {
        private static RESTClient _client;

        public DesktopClient(string port)
        {
            _client = new RESTClient("http://localhost:" + port);
        }

        // GET requests

        // Send a message to the server to tell the message handler to begin dequeuing packets
        public void SendEnqueuedPackets()
        {
            var request = CreateGetRequest("/SendEnqueuedPackets");

            _client.Execute(request);
        }

        // Send a message to the server to tell the message handler that the consumer is ready
        public void ConsumerReady()
        {
            var request = CreateGetRequest("/ConsumerReady");

            _client.Execute(request);
        }

        private static RESTRequest CreateGetRequest(string resource)
        {
            return new RESTRequest
            {
                Method = HttpMethod.GET,
                Resource = resource
            };
        }
    }

    #endregion

    #region Server
    public class DesktopServer
    {
        private static RESTServer _server;
        private static DesktopMessageHandler _handler;

        public DesktopServer(DesktopMessageHandler handler)
        {
            _handler = handler;
        }

        public void StartServer(string port)
        {
            _server = new RESTServer(port: port);
            _server.Start();

            while (_server.IsListening)
            {
                Thread.Sleep(300);
            }
        }

        public void StopServer()
        {
            _server.Stop();
        }

        public static T Deserialize<T>(Stream s)
        {
            using (StreamReader reader = new StreamReader(s))
            using (JsonTextReader jsonReader = new JsonTextReader(reader))
            {
                JsonSerializer ser = new JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }

        // Routes
        public sealed class Resources : RESTResource
        {

            // POST routes
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/DebugEvent")]
            public void ReceiveDebugEvent(HttpListenerContext context)
            {
                var packet = Deserialize<DebugPacket>(context.Request.InputStream);
                SendTextResponse(context, "Successfully captured Debug event.");

                _handler.OnDebugEvent(packet);
            }

            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/HandledErrorEvent")]
            public void ReceiveHandledErrorEvent(HttpListenerContext context)
            {
                var packet = Deserialize<HandledErrorPacket>(context.Request.InputStream);
                SendTextResponse(context, "Successfully captured Handled Error Event.");

                _handler.OnHandledErrorEvent(packet);
            }

            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/BacktestResultEvent")]
            public void ReceiveBacktestResultEvent(HttpListenerContext context)
            {
                var packet = Deserialize<BacktestResultPacket>(context.Request.InputStream);
                SendTextResponse(context, "Successfully captured Backtest Result Event.");

                _handler.OnBacktestResultEvent(packet);
            }

            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/RuntimeErrorEvent")]
            public void ReceiveRuntimeErrorEvent(HttpListenerContext context)
            {
                var packet = Deserialize<RuntimeErrorPacket>(context.Request.InputStream);
                SendTextResponse(context, "Successfully captured Runtime Error Event.");

                _handler.OnRuntimeErrorEvent(packet);
            }

            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/LogEvent")]
            public void ReceiveLogEvent(HttpListenerContext context)
            {
                var packet = Deserialize<LogPacket>(context.Request.InputStream);
                SendTextResponse(context, "Successfully captured Log Event.");

                _handler.OnLogEvent(packet);
            }

            // GET routes
            [RESTRoute]
            public void Heartbeat(HttpListenerContext context)
            {
                this.SendTextResponse(context, "Server is alive!");
            }
        }
    }
    #endregion
}
