using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grapevine;
using Grapevine.Client;
using Grapevine.Server;
using Newtonsoft.Json;
using QuantConnect.Interfaces;
using QuantConnect.Packets;

namespace QuantConnect.Messaging
{
    public class DesktopApi
    {
        private readonly EventMessagingHandler _messaging;
        private readonly DesktopApiServer _server;
        private readonly DesktopApiClient _client;

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

        public DesktopApi(IMessagingHandler messaging, string serverPort, string clientPort)
        {
            _messaging = (EventMessagingHandler)messaging;

            // Setup Event Handlers of message handler 
            // These will call GrapeVine client
            _messaging.DebugEvent += MessagingOnDebugEvent;
            _messaging.LogEvent += MessagingOnLogEvent;
            _messaging.RuntimeErrorEvent += MessagingOnRuntimeErrorEvent;
            _messaging.HandledErrorEvent += MessagingOnHandledErrorEvent;
            _messaging.BacktestResultEvent += MessagingOnBacktestResultEvent;

            // Start server
            _server = new DesktopApiServer(this);
            _server.StartServer(serverPort);

            _client = new DesktopApiClient(clientPort);
        }

        public void StopServer()
        {
            _server.StopServer();
        }
        public void ConsumerReady()
        {
            _messaging.OnConsumerReadyEvent();
        }

        public void SendEnqueuedPackets()
        {
            _messaging.SendEnqueuedPackets();
        }

        #region Event Handler
        // Event Handlers 
        private void MessagingOnHandledErrorEvent(HandledErrorPacket packet)
        {
            _client.SendHandledErrorEvent(packet);
        }

        private void MessagingOnRuntimeErrorEvent(RuntimeErrorPacket packet)
        {
            _client.SendRuntimeErrorEvent(packet);
        }

        private void MessagingOnLogEvent(LogPacket packet)
        {
            _client.SendLogEvent(packet);
        }

        private void MessagingOnDebugEvent(DebugPacket packet)
        {
            _client.SendDebugEvent(packet);
        }

        private void MessagingOnBacktestResultEvent(BacktestResultPacket packet)
        {
            _client.SendBacktestResultEvent(packet);
        }
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
    }

    #region Client
    public class DesktopApiClient
    {
        private static RESTClient _client;

        public DesktopApiClient(string port)
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

        // POST requests
        public void SendDebugEvent(DebugPacket packet)
        {
            var request = CreatePostRequest(packet, "/DebugEvent");

            _client.Execute(request);
        }
        public void SendBacktestResultEvent(BacktestResultPacket packet)
        {
            var request = CreatePostRequest(packet, "/BacktestResultEvent");

            _client.Execute(request);
        }

        public void SendHandledErrorEvent(HandledErrorPacket packet)
        {
            var request = CreatePostRequest(packet, "/HandledErrorEvent");

            _client.Execute(request);
        }

        public void SendRuntimeErrorEvent(RuntimeErrorPacket packet)
        {
            var request = CreatePostRequest(packet, "/RuntimeErrorEvent");

            _client.Execute(request);
        }

        public void SendLogEvent(LogPacket packet)
        {
            var request = CreatePostRequest(packet, "/LogEvent");

            _client.Execute(request);
        }

        // Helper methods
        private static RESTRequest CreatePostRequest(Packet packet, string resource)
        {
            return new RESTRequest
            {
                Method = HttpMethod.POST,
                Payload = JsonConvert.SerializeObject(packet),
                Resource = resource
            };
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
    public class DesktopApiServer
    {
        private static RESTServer _server;
        private static DesktopApi _handler;

        public DesktopApiServer(DesktopApi handler)
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
            [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/ConsumerReady")]
            public void HandleConsumerReadyEvent(HttpListenerContext context)
            {
                _handler.ConsumerReady();
                this.SendTextResponse(context, "The consumer is ready!");
            }

            [RESTRoute(Method = HttpMethod.GET, PathInfo = @"^/SendEnqueuedPackets")]
            public void HandleSendEnqueuedPackets(HttpListenerContext context)
            {
                _handler.SendEnqueuedPackets();
                this.SendTextResponse(context, "Sending enqueued packets!");
            }

            [RESTRoute]
            public void Heartbeat(HttpListenerContext context)
            {
                this.SendTextResponse(context, "Server is alive!");
            }
        }
    }
    #endregion
}
