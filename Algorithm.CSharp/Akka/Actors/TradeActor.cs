using Akka.Actor;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp.Akka.Actors
{
    #region Messages
    /// <summary>
    /// Indicates a security that should be bought according to the NewswireActor.
    /// </summary>
    public class BuyMessage
    {
        public BasicTemplateAlgorithm Algo { get; private set; }
        public Security Security { get; private set; }

        public BuyMessage(BasicTemplateAlgorithm algo, Security sec)
        {
            Algo = algo;
            Security = sec;
        }
    }
    #endregion


    /// <summary>
    /// The trade actor will call the buy method on the Algorithm class that is feeding
    /// the actor system information  
    /// </summary>
    public class TradeActor : ReceiveActor
    {
        public TradeActor()
        {
            // If the trade actor receives a BuyMessage, call the Buy method.
            Receive<BuyMessage>(bm =>
            {
                bm.Algo.Buy();
            });
        }
    }
}
