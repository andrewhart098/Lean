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

        public static T Deserialize<T>(Stream s)
        {
            try
            {
                using (StreamReader reader = new StreamReader(s))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        JsonSerializer ser = new JsonSerializer();
                        return ser.Deserialize<T>(jsonReader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("********************************");
                Console.WriteLine("There was an error deserializing");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("********************************");
            }
            return default(T);
        }

        // Routes
        public sealed class Resources : RESTResource
        {
            // POST routes

            // Debug endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/NewLiveJob")]
            public void ReceiveNewLiveJobEvent(HttpListenerContext context)
            {
                Console.WriteLine("New Job Received");
                var packet = Deserialize<LiveNodePacket>(context.Request.InputStream);
                SendTextResponse(context, CreateResponse("Success", "Successfully captured new job."));
                _handler.OnJobEvent(packet);
            }

            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/NewBacktestingJob")]
            public void ReceiveNewBacktestingJobEvent(HttpListenerContext context)
            {
                Console.WriteLine("New Job Received");
                var packet = Deserialize<BacktestNodePacket>(context.Request.InputStream);
                SendTextResponse(context, CreateResponse("Success", "Successfully captured new job."));
                _handler.OnJobEvent(packet);
            }

            // Debug endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/DebugEvent")]
            public void ReceiveDebugEvent(HttpListenerContext context)
            {
                Console.WriteLine("Debug Event");
                var packet = Deserialize<DebugPacket>(context.Request.InputStream);
                SendTextResponse(context, CreateResponse("Success", "Successfully captured Debug event."));
                _handler.OnDebugEvent(packet);
            }

            // Error endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/HandledErrorEvent")]
            public void ReceiveHandledErrorEvent(HttpListenerContext context)
            {
                Console.WriteLine("Handled Error Event");
                var packet = Deserialize<HandledErrorPacket>(context.Request.InputStream);
                SendTextResponse(context, CreateResponse("Success", "Successfully captured Handled Error Event."));
                _handler.OnHandledErrorEvent(packet);
            }


            // Backtest endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/BacktestResultEvent")]
            public void ReceiveBacktestResultEvent(HttpListenerContext context)
            {
                Console.WriteLine("Backtest Result Event");
                BacktestResultPacket packet; // Deserialize<BacktestResultPacket>(context.Request.InputStream);
                try
                {
                    using (StreamReader reader = new StreamReader(context.Request.InputStream))
                    {
                        using (JsonTextReader jsonReader = new JsonTextReader(reader))
                        {
                            var converter = new OrderJsonConverter();
                            JsonSerializer ser = new JsonSerializer();
                            ser.Converters.Add(converter);
                            packet = ser.Deserialize<BacktestResultPacket>(jsonReader);
                        }
                    }

                    SendTextResponse(context, CreateResponse("Success", "Successfully captured Backtest Result Event."));
                    _handler.OnBacktestResultEvent(packet);
                }
                catch (Exception e)
                {
                    SendTextResponse(context, CreateResponse("Error", e.ToString()));
                } 
            }

            // Runtime Error endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/RuntimeErrorEvent")]
            public void ReceiveRuntimeErrorEvent(HttpListenerContext context)
            {
                Console.WriteLine("Runtime Error Event");
                var packet = Deserialize<RuntimeErrorPacket>(context.Request.InputStream);
                SendTextResponse(context, CreateResponse("Success", "Successfully captured Runtime Error Event."));
                _handler.OnRuntimeErrorEvent(packet);
            }

            // Log endpoint
            [RESTRoute(Method = HttpMethod.POST, PathInfo = @"^/LogEvent")]
            public void ReceiveLogEvent(HttpListenerContext context)
            {
                Console.WriteLine("Log Event.");
                var packet = Deserialize<LogPacket>(context.Request.InputStream);
                SendTextResponse(context, CreateResponse("Success", "Successfully captured Log Event."));
                _handler.OnLogEvent(packet);
            }

            // GET routes
            [RESTRoute]
            public void Heartbeat(HttpListenerContext context)
            {
                this.SendTextResponse(context, "Server is alive!");
            }


            private string CreateResponse(string type, string message)
            {
                var response = new Response
                {
                    Type = type,
                    Message = message
                };

                return JsonConvert.SerializeObject(response);
            }
        }


        /// <summary>
        /// Response object from the Streaming API.
        /// </summary>
        private class Response
        {
            /// <summary>
            /// Type of response from the streaming api.
            /// </summary>
            /// <remarks>success or error</remarks>
            public string Type;

            /// <summary>
            /// Message description of the error or success state.
            /// </summary>
            public string Message;
        }
    }
    #endregion
}
