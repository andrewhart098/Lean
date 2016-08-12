using Akka.Actor;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using QuantConnect.Algorithm.CSharp.Akka.Messages;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp.Akka.Actors
{
    public class NewswireActor : ReceiveActor
    {
        public NewswireActor(IActorRef tradeActor)
        {
            Receive<AlgoMessage>(am =>
            {
                if (HasGoodNews(am))
                {
                    tradeActor.Tell(new BuyMessage(am.Algo, am.Security, true));
                }
            });
        }

        private bool HasGoodNews(AlgoMessage am)
        {
            News news = JsonConvert.DeserializeObject<News>("{ \"Stories\": [{ \"Symbol\": \"SPY\", \"TickDateTime\": \"10/7/2013 9:30:32 AM\"}]}");

            foreach (var story in news.Stories)
            {
                if (am.Time == story.TickDateTime)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
