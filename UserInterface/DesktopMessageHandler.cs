using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using QuantConnect.Orders;
using QuantConnect.Packets;
using NetMQ;
using NetMQ.Sockets;
using QuantConnect.Views.WinForms;

namespace QuantConnect.Views
{
    public class DesktopMessageHandler
    {
        private readonly DesktopServer _server;

        public DesktopMessageHandler(string port, LeanWinForm form)
        {
            // Start server on another thread
            _server = new DesktopServer();
            var thread = new Thread(() => _server.StartServer(port, form));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public class DesktopServer
        {
            public void StartServer(string port, LeanWinForm form)
            {
                using (var server = new PullSocket(">tcp://localhost:" + port))
                {
                    while (true)
                    {
                        var message = server.ReceiveMultipartMessage();

                        var resource = message[0].ConvertToString();
                        var packet = message[1].ConvertToString();

                        switch (resource)
                        {
                            case "/NewLiveJob":
                                var liveJobModel = Bind<LiveNodePacket>(packet);
                                form.Initialize(liveJobModel.packet);
                                break;
                            case "/NewBacktestingJob":
                                var backtestJobModel = Bind<BacktestNodePacket>(packet);
                                form.Initialize(backtestJobModel.packet);
                                break;
                            case "/DebugEvent":
                                var debugEventModel = Bind<DebugPacket>(packet);
                                form.MessagingOnDebugEvent(debugEventModel.packet);
                                break;
                            case "/HandledErrorEvent":
                                var handleErrorEventModel = Bind<HandledErrorPacket>(packet);
                                form.MessagingOnHandledErrorEvent(handleErrorEventModel.packet);
                                break;
                            case "/BacktestResultEvent":
                                var backtestResultEventModel = Bind<BacktestResultPacket>(packet);
                                form.MessagingOnBacktestResultEvent(backtestResultEventModel.packet);
                                break;
                            case "/RuntimeErrorEvent":
                                var runtimeErrorEventModel = Bind<RuntimeErrorPacket>(packet);
                                form.MessagingOnRuntimeErrorEvent(runtimeErrorEventModel.packet);
                                break;
                            case "/LogEvent":
                                var logEventModel = Bind<LogPacket>(packet);
                                form.MessagingOnLogEvent(logEventModel.packet);
                                break;
                        }
                    }
                }
            }

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
                                    packet = packet,
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
                    packet = default(T),
                    Errors = true
                };
            }

            public static Stream GenerateStreamFromString(string s)
            {
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
                return stream;
            }
        }

        public class Model<T>
        {
            public T packet { get; set; }
            public bool Errors { get; set; }
        }
    }
}