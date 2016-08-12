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

using System.IO;
using Akka.Actor;
using QuantConnect.Algorithm.CSharp.Akka.Actors;
using QuantConnect.Data;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Basic template algorithm simply initializes the date range and cash
    /// </summary>
    public class BasicTemplateAlgorithm : QCAlgorithm
    {
        private Symbol _spy = QuantConnect.Symbol.Create("SPY", SecurityType.Equity, Market.USA);

        // Static references to the actors in the system
        public static IActorRef LogActor;
        public static IActorRef TradeActor;
        public static IActorRef NewswireActor;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// This method is a modified version of the Initilize method that can accept a reference to the actor system.
        /// </summary>
        /// <param name="actorSystem">A heavyweight structure that will allocate 1...N Threads, so create one per logical application.</param>
        public override void Initialize(ActorSystem actorSystem)
        {
            SetStartDate(2013, 10, 07);  //Set Start Date
            SetEndDate(2013, 10, 11);    //Set End Date
            SetCash(100000);             //Set Strategy Cash
            AddEquity("SPY", Resolution.Second);

            // This file serves as a simple log file that the log actor will use
            File.Delete(@".\..\..\..\Logs.txt");
            
            // Create an actor that can log 
            LogActor = actorSystem.ActorOf(Props.Create(() => new LogActor()),
               "logActor");

            // Create actor that can trade - i.e. call Buy() in this class
            TradeActor = actorSystem.ActorOf(Props.Create(() => new TradeActor()),
               "tradeActor");

            // Create actor that checks for news about stocks in 
            NewswireActor = actorSystem.ActorOf(Props.Create(() => new NewswireActor(TradeActor, LogActor)),
                "newswireActor");
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            // Check the newswire actor for good news
            NewswireActor.Tell(new UpdateMessage(this, Securities[_spy], data.Time));
        }

        /// <summary>
        /// This method is called from the Trading actor
        /// </summary>
        public void Buy()
        {
            SetHoldings(_spy, 1);
            Debug("Purchased Stock");
            LogActor.Tell("Purchased Stock");
        }
    }
}