using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp.Akka.Actors
{
    public class LogActor: ReceiveActor 
    {
        public LogActor()
        {
            Receive<string>(str =>
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@".\..\..\..\Logs.txt", true))
                {
                    file.WriteLine(str.ToString());
                }
            });

            Receive<DateTime>(dt =>
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@".\..\..\..\Logs.txt", true))
                {
                    file.WriteLine(dt);
                }
            });

            Receive<Security>(s =>
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@".\..\..\..\Logs.txt", true))
                {
                    file.WriteLine(s.Price);
                }
            });
        }
    }
}
