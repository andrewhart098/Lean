﻿using System;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Notifications;
using QuantConnect.Packets;
using NetMQ;
using NetMQ.Sockets;

namespace QuantConnect.Messaging
{
    /// <summary>
    /// Message handler that sends messages over tcp using NetMQ.
    /// </summary>
    public class StreamingMessageHandler : IMessagingHandler
    {
        private string _port;
        private PushSocket _server;
        private AlgorithmNodePacket _job;
        
        /// <summary>
        /// Gets or sets whether this messaging handler has any current subscribers.
        /// This is not used in this message handler.  Messages are sent via tcp as they arrive
        /// </summary>
        public bool HasSubscribers { get; set; }

        /// <summary>
        /// Initialize the messaging system
        /// </summary>
        public void Initialize()
        {
            _port   = Config.Get("desktop-http-port");
            CheckPort();
            _server = new PushSocket("@tcp://*:" + _port);
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
                Transmit(_job, typeof(LiveNodePacket).Name);
            }
            Transmit(_job, typeof(BacktestNodePacket).Name);
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
        /// Send certain types of messages with a base type of Packet.
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
            Transmit(packet, typeof(BacktestResultPacket).Name);
        }

        private void SendHandledErrorEvent(HandledErrorPacket packet)
        {
            Transmit(packet, typeof(HandledErrorPacket).Name);
        }

        private void SendRuntimeErrorEvent(RuntimeErrorPacket packet)
        {
            Transmit(packet, typeof(RuntimeErrorPacket).Name);
        }

        private void SendLogEvent(LogPacket packet)
        {
            Transmit(packet, typeof(LogPacket).Name);
        }

        private void SendDebugEvent(DebugPacket packet)
        {
            Transmit(packet, typeof(DebugPacket).Name);
        }



        /// <summary>
        /// Send a message to the _server using ZeroMQ
        /// </summary>
        /// <param name="packet">Packet to transmit</param>
        /// <param name="resource">The resource where the packet will be sent</param>
        public void Transmit(Packet packet, string resource)
        {
            var payload = JsonConvert.SerializeObject(packet);

            var message = new NetMQMessage();

            message.Append(resource);
            message.Append(payload);

            _server.SendMultipartMessage(message);
        }

        /// <summary>
        /// Check if port to be used by the desktop application is available.
        /// </summary>
        private void CheckPort()
        {
            try
            {
                TcpListener tcpListener = new TcpListener(IPAddress.Any, _port.ToInt32());
                tcpListener.Start();
                tcpListener.Stop();
            }
            catch
            {
                throw new Exception("The port configured in config.json is either being used or blocked by a firewall." +
                    "Please choose a new port or open the port in the firewall.");
            }
        }
    }
}
