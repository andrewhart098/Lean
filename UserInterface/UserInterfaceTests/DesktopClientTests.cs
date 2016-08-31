﻿using System.Collections.Generic;
using System.Threading;
using Moq;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Messaging;
using QuantConnect.Packets;

namespace QuantConnect.Views.UserInterfaceTests
{
    [TestFixture]
    class DesktopClientTests
    {
        private string _port = "1235";
        private DesktopClient _desktopMessageHandler;

        [TestFixtureSetUp]
        public void Setup()
        {
            _desktopMessageHandler = new DesktopClient();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _desktopMessageHandler.StopServer();
        }

        private void StartClientThread(IDesktopMessageHandler messageHandler)
        {
            var thread = new Thread(() => _desktopMessageHandler.Run(_port, messageHandler));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        [Test]
        public void DesktopClient_WillNotAccept_SinglePartMessages()
        {
            Queue<Packet> packets = new Queue<Packet>();
            // Mock the the message handler processor
            var messageHandler = new Mock<IDesktopMessageHandler>();
            messageHandler.Setup(mh => mh.DisplayBacktestResultsPacket(It.IsAny<BacktestResultPacket>())).Callback(
                (BacktestResultPacket packet) =>
                {
                    packets.Enqueue(packet);
                }).Verifiable();

            // Setup Client
            StartClientThread(messageHandler.Object);

            using (PushSocket server = new PushSocket("@tcp://*:" + _port))
            {
                var message = new NetMQMessage();

                message.Append(typeof(BacktestResultPacket).Name);

                server.SendMultipartMessage(message);
            }

            // Give NetMQ time to send the message
            Thread.Sleep(500);

            Assert.IsTrue(packets.Count == 0);
        }

        [Test]
        public void DesktopClient_WillAccept_TwoPartMessages()
        {
            Queue<Packet> packets = new Queue<Packet>();
            // Mock the the message handler processor
            var messageHandler = new Mock<IDesktopMessageHandler>();
            messageHandler.Setup(mh => mh.DisplayLogPacket(It.IsAny<LogPacket>()))
                .Callback((LogPacket packet) =>
                {
                    packets.Enqueue(packet);
                })
                .Verifiable();

            // Setup Client
            StartClientThread(messageHandler.Object);

            using (PushSocket server = new PushSocket("@tcp://*:" + _port))
            {
                var message = new NetMQMessage();

                message.Append(typeof(LiveNodePacket).Name);
                message.Append(JsonConvert.SerializeObject(new LogPacket()));

                server.SendMultipartMessage(message);
            }

            // Give NetMQ time to send the message
            Thread.Sleep(500);

            Assert.IsTrue(packets.Count == 1);
        }

        [Test]
        public void DesktopClient_WillNotAccept_MoreThanTwoPartMessages()
        {
            Queue<Packet> packets = new Queue<Packet>();
            // Mock the the message handler processor
            var messageHandler = new Mock<IDesktopMessageHandler>();
            messageHandler.Setup(mh => mh.DisplayLogPacket(It.IsAny<LogPacket>()))
                .Callback((LogPacket packet) =>
                {
                    packets.Enqueue(packet);
                })
                .Verifiable();

            // Setup Client
            StartClientThread(messageHandler.Object);

            using (PushSocket server = new PushSocket("@tcp://*:" + _port))
            {
                var message = new NetMQMessage();

                message.Append(typeof(LogPacket).Name);
                message.Append(JsonConvert.SerializeObject(new LogPacket()));
                message.Append("hello!");

                server.SendMultipartMessage(message);
            }

            // Give NetMQ time to send the message
            Thread.Sleep(500);

            Assert.IsTrue(packets.Count == 0);
        }
    }
}
