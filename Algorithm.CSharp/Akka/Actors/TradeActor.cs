using System;
using System.Collections.Generic;
using Akka.Actor;
using QuantConnect.Algorithm.CSharp.Akka.Messages;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp.Akka.Actors
{
    public class TradeActor : ReceiveActor
    {
        public TradeActor()
        {
            Receive<BuyMessage>(bm =>
            {
                bm.Algo.Buy();
            });
        }
    }
}
