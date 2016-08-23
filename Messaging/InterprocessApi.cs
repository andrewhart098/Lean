using System.IO;
using System.Net;
using System.Threading;
using Grapevine;
using Grapevine.Client;
using Grapevine.Server;
using Newtonsoft.Json;
using QuantConnect.Interfaces;
using QuantConnect.Packets;

namespace QuantConnect.Messaging
{
    public class InterprocessApi
    {
        private readonly EventMessagingHandler _messaging;
        private readonly InterprocessApiServer _server;
        private readonly InterprocessApiClient _client;

        public InterprocessApi(IMessagingHandler messaging, string serverPort, string clientPort)
        {
            _messaging = (EventMessagingHandler)messaging;

            // Setup Event Handlers
            _messaging.DebugEvent += MessagingOnDebugEvent;
            _messaging.LogEvent += MessagingOnLogEvent;
            _messaging.RuntimeErrorEvent += MessagingOnRuntimeErrorEvent;
            _messaging.HandledErrorEvent += MessagingOnHandledErrorEvent;
            _messaging.BacktestResultEvent += MessagingOnBacktestResultEvent;

            _client = new InterprocessApiClient(clientPort);

            // Start server on different thread
            _server = new InterprocessApiServer(this);
            var thread = new Thread(() => _server.StartServer(serverPort));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
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


    }

    #region Client
    public class InterprocessApiClient
    {
        private static RESTClient _client;

        public InterprocessApiClient(string port)
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
    public class InterprocessApiServer
    {
        private static RESTServer _server;
        private static InterprocessApi _handler;

        public InterprocessApiServer(InterprocessApi handler)
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
