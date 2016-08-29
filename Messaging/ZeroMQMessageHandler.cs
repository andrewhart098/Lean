using System;
using System.Net;
using Newtonsoft.Json;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Notifications;
using QuantConnect.Packets;
using NetMQ;
using NetMQ.Sockets;
using RestSharp;

namespace QuantConnect.Messaging
{
    // <summary>
    /// Message handler that sends messages over http as soon as the messages arrive.
    /// </summary>
    public class ZeroMQMessageHandler : IMessagingHandler
    {
        private AlgorithmNodePacket _job;

        // Port to send data to
        public static readonly string Port = "1234";//Config.Get("http-port");

        // Client for sending asynchronous requests.
        private static readonly RequestSocket Client = new RequestSocket("http://localhost:" + Port);


        /// <summary>
        /// Gets or sets whether this messaging handler has any current subscribers.
        /// This is not used in this message handler.  Messages are sent via http as they arrive
        /// </summary>
        public bool HasSubscribers { get; set; }

        /// <summary>
        /// Initialize the messaging system
        /// </summary>
        public void Initialize()
        {
            // put the port to start on here
        }

        /// <summary>
        /// Set the user communication channel
        /// </summary>
        /// <param name="job"></param>
        public void SetAuthentication(AlgorithmNodePacket job)
        {
            _job = job;

            if (_job is LiveNodePacket)
            {
                Transmit(_job, "/NewLiveJob");
            }
            Transmit(_job, "/NewBacktestingJob");
        }

        /// <summary>
        /// Send any notification with a base type of Notification.
        /// </summary>
        /// <param name="notification">The notification to be sent.</param>
        public void SendNotification(Notification notification)
        {
            var type = notification.GetType();
            if (type == typeof(NotificationEmail) || type == typeof(NotificationWeb) || type == typeof(NotificationSms))
            {
                Log.Error("Messaging.SendNotification(): Send not implemented for notification of type: " + type.Name);
                return;
            }
            notification.Send();
        }

        /// <summary>
        /// Send any message with a base type of Packet over http.
        /// </summary>
        public void Send(Packet packet)
        {
            //Packets we handled in the UX.
            switch (packet.Type)
            {
                case PacketType.Debug:
                    var debug = (DebugPacket)packet;
                    SendDebugEvent(debug);
                    break;

                case PacketType.Log:
                    var log = (LogPacket)packet;
                    SendLogEvent(log);
                    break;

                case PacketType.RuntimeError:
                    var runtime = (RuntimeErrorPacket)packet;
                    SendRuntimeErrorEvent(runtime);
                    break;

                case PacketType.HandledError:
                    var handled = (HandledErrorPacket)packet;
                    SendHandledErrorEvent(handled);
                    break;

                case PacketType.BacktestResult:
                    var result = (BacktestResultPacket)packet;
                    SendBacktestResultEvent(result);
                    break;
            }

            if (StreamingApi.IsEnabled)
            {
                StreamingApi.Transmit(_job.UserId, _job.Channel, packet);
            }
        }

        private void SendBacktestResultEvent(BacktestResultPacket packet)
        {
            Transmit(packet, "/BacktestResultEvent");
        }

        private void SendHandledErrorEvent(HandledErrorPacket packet)
        {
            Transmit(packet, "/HandledErrorEvent");
        }

        private void SendRuntimeErrorEvent(RuntimeErrorPacket packet)
        {
            Transmit(packet, "/RuntimeErrorEvent");
        }

        private void SendLogEvent(LogPacket packet)
        {
            Transmit(packet, "/LogEvent");
        }

        private void SendDebugEvent(DebugPacket packet)
        {
            Transmit(packet, "/DebugEvent");
        }



        /// <summary>
        /// Send a message to the Client using GrapeVine
        /// </summary>
        /// <param name="packet">Packet to transmit</param>
        /// <param name="resource">The resource where the packet will be sent</param>
        public static void Transmit(Packet packet, string resource)
        {
            var tx = JsonConvert.SerializeObject(packet);
            var message = new NetMQMessage();

            message.Append(resource);
            message.Append(tx);

            Client.SendMultipartMessage(message);
        }

        //public bool CheckHeartBeat()
        //{
        //    var request = new RestRequest
        //    {
        //        Method = Method.GET,
        //        Resource = "/",
        //        Timeout = 1000
        //    };

        //    var response = Client.Execute(request);

        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        return true;
        //    }

        //    return false;
        //}
    }
}
