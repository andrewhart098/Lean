using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Configuration;
using QuantConnect.Messaging;
using QuantConnect.Packets;

namespace QuantConnect.Tests.Messaging
{
    [TestFixture, Ignore("This tests requires an open TCP port.")]
    public class StreamingMessageHandlerTests
    {
        private readonly string _port = "1234";
        private StreamingMessageHandler _messageHandler;

        [TestFixtureSetUp]
        public void SetUp()
        {
            Config.Set("http-port", _port);

            _messageHandler = new StreamingMessageHandler();
            _messageHandler.Initialize();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            //
        }

        [Test]
        public void MessageHandler_WillSend_MultipartMessage()
        {
            var resource = "/LogEvent";

            using (var pullSocket = new PullSocket(">tcp://localhost:" + _port))
            {
                var logPacket = new LogPacket
                {
                    Message = "1"
                };

                var tx = JsonConvert.SerializeObject(logPacket);

                _messageHandler.Transmit(logPacket, resource);

                var message = pullSocket.ReceiveMultipartMessage();

                Assert.IsTrue(message.FrameCount == 2);
                Assert.IsTrue(message[0].ConvertToString() == resource);
                Assert.IsTrue(message[1].ConvertToString() == tx);
            }
        }

        [Test]
        public void MessageHandler_SendsCorrectPackets_ToCorrectRoutes()
        {
            var debug = new DebugPacket();
            var log = new LogPacket();
            var backtest = new BacktestResultPacket();
            var handled = new HandledErrorPacket();
            var error = new RuntimeErrorPacket();
            var packetList = new List<Packet>
                {
                    log,
                    debug,
                    backtest,
                    handled,
                    error
                };

            using (var pullSocket = new PullSocket(">tcp://localhost:" + _port))
            {
                var count = 0;
                while (count < packetList.Count)
                {
                    _messageHandler.Send(packetList[count]);

                    var message = pullSocket.ReceiveMultipartMessage();

                    var resource = message[0].ConvertToString();
                    var packet   = message[1].ConvertToString();

                    Assert.IsTrue(message.FrameCount == 2);

                    switch (resource)
                    {
                        case Resources.Debug:
                            Assert.IsTrue(packet == JsonConvert.SerializeObject(debug));
                            break;
                        case Resources.HandledError:
                            Assert.IsTrue(packet == JsonConvert.SerializeObject(handled));
                            break;
                        case Resources.BacktestResult:
                            Assert.IsTrue(packet == JsonConvert.SerializeObject(backtest));
                            break;
                        case Resources.RuntimeError:
                            Assert.IsTrue(packet == JsonConvert.SerializeObject(error));
                            break;
                        case Resources.Log:
                            Assert.IsTrue(packet == JsonConvert.SerializeObject(log));
                            break;
                    }

                    count++;
                }
            }
        }

        [Test]
        public void MessageHandler_WillSend_NewBackTestJob_ToCorrectRoute()
        {
            var backtest = new BacktestNodePacket();
            using (var pullSocket = new PullSocket(">tcp://localhost:" + _port))
            {
                _messageHandler.SetAuthentication(backtest);

                var message = pullSocket.ReceiveMultipartMessage();

                var resource = message[0].ConvertToString();
                var packet   = message[1].ConvertToString();

                Assert.IsTrue(message.FrameCount == 2);
                Assert.IsTrue(resource == Resources.BacktestJob);
                Assert.IsTrue(packet == JsonConvert.SerializeObject(backtest));
            }
        }

        [Test]
        public void MessageHandler_WillSend_NewLiveJob_ToCorrectRoute()
        {
            using (var pullSocket = new PullSocket(">tcp://localhost:" + _port))
            {
                _messageHandler.SetAuthentication(new LiveNodePacket());

                var message = pullSocket.ReceiveMultipartMessage();

                var resource = message[0].ConvertToString();
                var packet = message[1].ConvertToString();

                Assert.IsTrue(message.FrameCount == 2);
                Assert.IsTrue(resource == Resources.LiveJob);
                Assert.IsTrue(packet == JsonConvert.SerializeObject(new LiveNodePacket()));
            }
        }
    }
}
