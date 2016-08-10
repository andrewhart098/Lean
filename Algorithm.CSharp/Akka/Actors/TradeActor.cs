using System;
using System.Collections.Generic;
using Akka.Actor;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp.Akka.Actors
{
    public class TradeActor : ReceiveActor
    {
        public List<string> SymbolsToBuys = new List<string>();
        public TradeActor()
        {
            Receive<Security>(s =>
            {
                SymbolsToBuys.Add(s.Symbol);
            });

            Receive<string>(s =>
            {
                Sender.Tell(SymbolsToBuys.Contains(s), Self);
            });
        }
    }
}
