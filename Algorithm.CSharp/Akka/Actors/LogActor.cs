using System;
using Akka.Actor;
using Akka.Event;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp.Akka.Actors
{
    /// <summary>
    /// An actor for writing various types to a log file.
    /// This actor can handle strings, DateTimes and Securities.
    /// </summary>
    public class LogActor: ReceiveActor 
    {
        public LogActor()
        {
            var log = Context.GetLogger();
            Receive<string>(str =>
            {
                log.Info("The value is {0}", str);
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@".\..\..\..\Logs.txt", true))
                //{
                //    file.WriteLine(str.ToString());
                //}
            });

            Receive<DateTime>(dt =>
            {
                log.Info("The string is {0}", dt);
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@".\..\..\..\Logs.txt", true))
                //{
                //    file.WriteLine(dt);
                //}
            });

            Receive<Security>(s =>
            {
                log.Info("The equity {symbol} cost {price}", s.Symbol, s.Price);
                //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@".\..\..\..\Logs.txt", true))
                //{
                //    file.WriteLine(s.Price);
                //}
            });
        }
    }
}
