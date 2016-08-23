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
        private readonly AlgorithmNodePacket _job;
        private readonly InterprocessApiServer _server;
        private readonly InterprocessApiClient _client;

        public InterprocessApi(IMessagingHandler messaging, AlgorithmNodePacket job, string serverPort, string clientPort)
        {
            _messaging = (EventMessagingHandler)messaging;
            _job = job;

            // Setup Event Handlers
            _messaging.DebugEvent += MessagingOnDebugEvent;
            _messaging.LogEvent += MessagingOnLogEvent;
            _messaging.RuntimeErrorEvent += MessagingOnRuntimeErrorEvent;
            _messaging.HandledErrorEvent += MessagingOnHandledErrorEvent;
            _messaging.BacktestResultEvent += MessagingOnBacktestResultEvent;

            // Start client
            _client = new InterprocessApiClient(clientPort);

            // Start server on different thread
            _server = new InterprocessApiServer(this);
            var thread = new Thread(() => _server.StartServer(serverPort));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Get the URL for the embedded charting
        /// </summary>
        /// <param name="job">Job packet for the URL</param>
        /// <param name="liveMode">Is this a live mode chart?</param>
        /// <param name="holdReady">Hold the ready signal to inject data</param>
        public static string GetUrl(AlgorithmNodePacket job, bool liveMode = false, bool holdReady = false)
        {
            var url = "";
            var hold = holdReady == false ? "0" : "1";
            var embedPage = liveMode ? "embeddedLive" : "embedded";

            url = string.Format(
                "https://www.quantconnect.com/terminal/{0}?user={1}&token={2}&pid={3}&version={4}&holdReady={5}&bid={6}",
                embedPage, job.UserId, job.Channel, job.ProjectId, Globals.Version, hold, job.AlgorithmId);

            //Show warnings if the API token and UID aren't set.
            if (job.UserId == 0)
            {
                throw new System.Exception("Your user id is not set. Please check your config.json file 'job-user-id' property.");
            }
            if (job.Channel == "")
            {
                throw new System.Exception("Your API token is not set. Please check your config.json file 'api-access-token' property.");
            }

            return url;
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
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;
            return new RESTRequest
            {
                Method = HttpMethod.POST,
                Payload = JsonConvert.SerializeObject(packet, Formatting.Indented, settings),
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
