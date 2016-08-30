using System;
using System.IO;
using Newtonsoft.Json;
using QuantConnect.Orders;
using QuantConnect.Packets;
using NetMQ;
using NetMQ.Sockets;
using QuantConnect.Messaging;
using QuantConnect.Views.WinForms;

namespace QuantConnect.Views
{
    public class DesktopMessageHandler
    {
        /// <summary>
        /// This 0MQ Pull socket accepts certain messages from a 0MQ Push socket
        /// </summary>
        /// <param name="port">The port on which to listen</param>
        /// <param name="form">The form on which to push responses</param>
        public void StartMessageHandler(string port, LeanWinForm form)
        {
            using (var pullSocket = new PullSocket(">tcp://localhost:" + port))
            {
                while (true)
                {
                    var message  = pullSocket.ReceiveMultipartMessage();

                    // There should only be 2 part messages
                    if (message.FrameCount != 2) continue;

                    var resource = message[0].ConvertToString();
                    var packet   = message[1].ConvertToString();

                    switch (resource)
                    {
                        case Resources.LiveJob:
                            var liveJobModel = Bind<LiveNodePacket>(packet);
                            if (!liveJobModel.Errors)
                                form.Initialize(liveJobModel.Packet);
                            break;
                        case Resources.BacktestJob:
                            var backtestJobModel = Bind<BacktestNodePacket>(packet);
                            if (!backtestJobModel.Errors)
                                form.Initialize(backtestJobModel.Packet);
                            break;
                        case Resources.Debug:
                            var debugEventModel = Bind<DebugPacket>(packet);
                            if (!debugEventModel.Errors)
                                form.MessagingOnDebugEvent(debugEventModel.Packet);
                            break;
                        case Resources.HandledError:
                            var handleErrorEventModel = Bind<HandledErrorPacket>(packet);
                            form.MessagingOnHandledErrorEvent(handleErrorEventModel.Packet);
                            break;
                        case Resources.BacktestResult:
                            var backtestResultEventModel = Bind<BacktestResultPacket>(packet);
                            if (!backtestResultEventModel.Errors)
                                form.MessagingOnBacktestResultEvent(backtestResultEventModel.Packet);
                            break;
                        case Resources.RuntimeError:
                            var runtimeErrorEventModel = Bind<RuntimeErrorPacket>(packet);
                            if (!runtimeErrorEventModel.Errors)
                                form.MessagingOnRuntimeErrorEvent(runtimeErrorEventModel.Packet);
                            break;
                        case Resources.Log:
                            var logEventModel = Bind<LogPacket>(packet);
                            if (!logEventModel.Errors)
                                form.MessagingOnLogEvent(logEventModel.Packet);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// This method deserializes the incoming stream to a specific type
        /// </summary>
        /// <typeparam name="T">The type of Packet that we are deserialiing into</typeparam>
        /// <param name="st">The payload of the message from the 0MQ Push Socket</param>
        /// <returns>The </returns>
        public static Model<T> Bind<T>(string st)
        {
            try
            {
                using (Stream s = GenerateStreamFromString(st))
                {
                    using (StreamReader reader = new StreamReader(s))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(reader))
                        {
                            JsonSerializer ser = new JsonSerializer();

                            // If backtest result, add custom converter
                            if (typeof(T) == typeof(BacktestResultPacket))
                            {
                                var converter = new OrderJsonConverter();
                                ser.Converters.Add(converter);
                            }

                            var packet = ser.Deserialize<T>(jsonReader);

                            return new Model<T>
                            {
                                Packet = packet,
                                Errors = false
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error deserializing the request. " + ex.ToString());
            }

            // If we made it this far something went wrong
            return new Model<T>
            {
                Packet = default(T),
                Errors = true
            };
        }

        /// <summary>
        /// Generates a Stream from a string.
        /// Taken from http://stackoverflow.com/questions/1879395/how-to-generate-a-stream-from-a-string
        /// </summary>
        /// <param name="s">The string to turn into a stream</param>
        /// <returns>A sequence of bytes representing the string passed into the method</returns>
        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// A class representing the multipart message in 0MQ
        /// </summary>
        /// <typeparam name="T">The type of packet that this model represents</typeparam>
        public class Model<T>
        {
            public T Packet { get; set; }
            public bool Errors { get; set; }
        }
    }
}