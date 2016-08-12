/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System.Collections.Generic;
using System.IO;
using Akka.Actor;
using QuantConnect.Algorithm.CSharp.Akka.Actors;
using QuantConnect.Algorithm.CSharp.Akka.Messages;
using QuantConnect.Data;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Basic template algorithm simply initializes the date range and cash
    /// </summary>
    public class BasicTemplateAlgorithm : QCAlgorithm
    {
        private Symbol _spy = QuantConnect.Symbol.Create("SPY", SecurityType.Equity, Market.USA);

        public static IActorRef LogActor;
        public static IActorRef TradeActor;
        public static IActorRef NewswireActor;

        public List<string> SymbolsToBuys = new List<string>();
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize(ActorSystem actorSystem)
        {
            SetStartDate(2013, 10, 07);  //Set Start Date
            SetEndDate(2013, 10, 11);    //Set End Date
            SetCash(100000);             //Set Strategy Cash
            AddEquity("SPY", Resolution.Second);
            File.Delete(@".\..\..\..\Logs.txt");
            // make our first actors!
            LogActor = actorSystem.ActorOf(Props.Create(() => new LogActor()),
               "logActor");

            TradeActor = actorSystem.ActorOf(Props.Create(() => new TradeActor()),
               "tradeActor");

            NewswireActor = actorSystem.ActorOf(Props.Create(() => new NewswireActor(TradeActor)),
                "newswireActor");
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            var sec = Securities[_spy];
            var message = new AlgoMessage(this, sec, data.Time);

            NewswireActor.Tell(message);
        }

        public void Buy()
        {
            SetHoldings(_spy, 1);
            Debug("Purchased Stock");

            LogActor.Tell("Purchased Stock");
        }
    }
}