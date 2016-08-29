using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using QuantConnect.Orders;
using QuantConnect.Packets;
using NetMQ;
using NetMQ.Sockets;

namespace QuantConnect.Views
{
    public class DesktopMessageHandler
    {
        private readonly DesktopServer _server;

        public DesktopMessageHandler(string serverPort)
        {
            // Start server on another thread
            _server = new DesktopServer(this);
            var thread = new Thread(() => _server.StartServer(serverPort));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

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

        public delegate void ReceivedJobEventRaised(AlgorithmNodePacket packet);
        public event ReceivedJobEventRaised ReceivedJobEvent;
        #endregion

        /// <summary>
        /// Raise a debug event safely
        /// </summary>
        public virtual void OnJobEvent(AlgorithmNodePacket packet)
        {
            var handler = ReceivedJobEvent;

            if (handler != null)
            {
                handler(packet);
            }
        }

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
    }

    #region Server
    public class DesktopServer
    {
        private static ResponseSocket _server;
        private static DesktopMessageHandler _handler;

        public DesktopServer(DesktopMessageHandler handler)
        {
            _handler = handler;
        }

        public void StartServer(string port)
        {
            using (var server = new ResponseSocket("@tcp://127.0.0.1:5556"))
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
                            _handler.OnJobEvent(liveJobModel.packet);
                            break;
                        case "/NewBacktestingJob":
                            var backtestJobModel = Bind<BacktestNodePacket>(packet);
                            _handler.OnJobEvent(backtestJobModel.packet);
                            break;
                        case "/DebugEvent":
                            var debugEventModel = Bind<DebugPacket>(packet);
                            _handler.OnDebugEvent(debugEventModel.packet);
                            break;
                        case "/HandledErrorEvent":
                            var handleErrorEventModel = Bind<HandledErrorPacket>(packet);
                            _handler.OnHandledErrorEvent(handleErrorEventModel.packet);
                            break;
                        case "/BacktestResultEvent":
                            var backtestResultEventModel = Bind<BacktestResultPacket>(packet);
                            _handler.OnBacktestResultEvent(backtestResultEventModel.packet);
                            break;
                        case "/RuntimeErrorEvent":
                            var runtimeErrorEventModel = Bind<RuntimeErrorPacket>(packet);
                            _handler.OnRuntimeErrorEvent(runtimeErrorEventModel.packet);
                            break;
                        case "/LogEvent":
                            var logEventModel = Bind<LogPacket>(packet);
                            _handler.OnLogEvent(logEventModel.packet);
                            break;
                    }
                }
            }
        }

        public void StopServer()
        {
            _server.Close();
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

    #endregion
}