using System;
using System.IO;
using System.Net;
using System.Threading;
using Grapevine;
using Grapevine.Server;
using Newtonsoft.Json;
using QuantConnect.Orders;
using QuantConnect.Packets;

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
        private static RESTServer _server;
        private static DesktopMessageHandler _handler;

        public DesktopServer(DesktopMessageHandler handler)
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


        public static Model<T> Bind<T>(Stream s)
        {
            try
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



        // Routes
        public sealed class Resources : RESTResource
        {
            // POST routes

            // Debug endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/NewLiveJob")]
            public void ReceiveNewLiveJobEvent(HttpListenerContext context)
            {
                var model = Bind<LiveNodePacket>(context.Request.InputStream);

                if (model.Errors)
                {
                    InternalServerError(context);
                }
                else
                {
                    SendTextResponse(context, "Success");
                    _handler.OnJobEvent(model.packet);
                }
            }

            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/NewBacktestingJob")]
            public void ReceiveNewBacktestingJobEvent(HttpListenerContext context)
            {
                var model = Bind<BacktestNodePacket>(context.Request.InputStream);

                if (model.Errors)
                {
                    InternalServerError(context);
                }
                else
                {
                    SendTextResponse(context, "Success");
                    _handler.OnJobEvent(model.packet);
                }
            }

            // Debug endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/DebugEvent")]
            public void ReceiveDebugEvent(HttpListenerContext context)
            {
                var model = Bind<DebugPacket>(context.Request.InputStream);
                if (model.Errors)
                {
                    InternalServerError(context);
                }
                else
                {
                    SendTextResponse(context, "Success");
                    _handler.OnDebugEvent(model.packet);
                }
            }

            // Error endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/HandledErrorEvent")]
            public void ReceiveHandledErrorEvent(HttpListenerContext context)
            {
                var model = Bind<HandledErrorPacket>(context.Request.InputStream);
                if (model.Errors)
                {
                    InternalServerError(context);
                }
                else
                {
                    SendTextResponse(context, "Success");
                    _handler.OnHandledErrorEvent(model.packet);
                }
                SendTextResponse(context, "Success");
            }


            // Backtest endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/BacktestResultEvent")]
            public void ReceiveBacktestResultEvent(HttpListenerContext context)
            {
                var model = Bind<BacktestResultPacket>(context.Request.InputStream);
                if (model.Errors)
                {
                    InternalServerError(context);
                }
                else
                {
                    SendTextResponse(context, "Success");
                    _handler.OnBacktestResultEvent(model.packet);
                }
            }

            // Runtime Error endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/RuntimeErrorEvent")]
            public void ReceiveRuntimeErrorEvent(HttpListenerContext context)
            {
                var model = Bind<RuntimeErrorPacket>(context.Request.InputStream);
                if (model.Errors)
                {
                    InternalServerError(context);
                }
                else
                {
                    SendTextResponse(context, "Success");
                    _handler.OnRuntimeErrorEvent(model.packet);
                }
            }

            // Log endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/LogEvent")]
            public void ReceiveLogEvent(HttpListenerContext context)
            {
                var model = Bind<LogPacket>(context.Request.InputStream);
                if (model.Errors)
                {
                    InternalServerError(context);
                }
                else
                {
                    SendTextResponse(context, "Success");
                    _handler.OnLogEvent(model.packet);
                }
            }

            // GET routes
            [RESTRoute]
            public void Heartbeat(HttpListenerContext context)
            {
                this.SendTextResponse(context, "Server is alive!");
            }
        }
    }

    public class Model<T>
    {
        public T packet { get; set; }
        public bool Errors { get; set; }
    }

    #endregion
}
