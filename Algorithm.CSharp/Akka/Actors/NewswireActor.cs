using Akka.Actor;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp.Akka.Actors
{
    public class NewswireActor : ReceiveActor
    {
        public NewswireActor(IActorRef tradeActor)
        {
            Receive<Security>(s =>
            {
                if (HasGoodNews(s))
                {
                    tradeActor.Tell(s);
                }
            });
        }

        private bool HasGoodNews(Security sec)
        {
            return false;
        }
    }
}
