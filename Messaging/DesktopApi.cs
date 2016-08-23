using System;
using Grapevine;
using Grapevine.Client;
using Newtonsoft.Json;
using QuantConnect.Interfaces;
using QuantConnect.Packets;


namespace QuantConnect.Messaging
{
    /// <summary>
    /// 
    /// </summary>
    public class DesktopApi
    {
        // Client for sending asynchronous requests.
        private static readonly RESTClient _client = new RESTClient("http://localhost:1238");
        private readonly EventMessagingHandler _messaging;


        //public DesktopApi()
        //{
        //    //_messaging = (EventMessagingHandler)notificationHandler;

        //    //// Setup event handlers
        //    //_messaging.DebugEvent += MessagingOnDebugEvent;
        //    //_messaging.LogEvent += MessagingOnLogEvent;
        //    //_messaging.RuntimeErrorEvent += MessagingOnRuntimeErrorEvent;
        //    //_messaging.HandledErrorEvent += MessagingOnHandledErrorEvent;
        //    //_messaging.BacktestResultEvent += MessagingOnBacktestResultEvent;
        //}

        private void MessagingOnBacktestResultEvent(BacktestResultPacket packet)
        {
            Send(packet, "BacktestResult");
        }

        private void MessagingOnHandledErrorEvent(HandledErrorPacket packet)
        {
            Send(packet, "HandledError");
        }

        private void MessagingOnRuntimeErrorEvent(RuntimeErrorPacket packet)
        {
            Send(packet, "RuntimeError");
        }

        private void MessagingOnLogEvent(LogPacket packet)
        {
            Send(packet, "Log");
        }

        private void MessagingOnDebugEvent(DebugPacket packet)
        {
            Send(packet, "Debug");
        }

        private void Send(Packet packet, string path)
        {
            var tx = JsonConvert.SerializeObject(packet);
            var request = new RESTRequest("/resources");

            request.Payload = tx;


            _client.Execute(request);
        }

        public static void Send(Packet packet)
        {
            var tx = JsonConvert.SerializeObject(packet);
            var request = new RESTRequest("/resources", HttpMethod.POST);

            request.Payload = tx;

            var r = _client.Execute(request);
        }
    }
}
