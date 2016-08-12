using System;
using System.Collections.Generic;
using Akka.Actor;
using Newtonsoft.Json;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp.Akka.Actors
{
    #region Messages 
    /// <summary>
    /// The message sent to the Actors from the Algorithm class' Buy method.
    /// Contains a reference to the Algorithm
    /// </summary>
    public class UpdateMessage
    {
        public BasicTemplateAlgorithm Algo { get; private set; }
        public Security Security { get; private set; }
        public DateTime Time { get; private set; }

        public UpdateMessage(BasicTemplateAlgorithm algo, Security sec, DateTime time)
        {
            Algo = algo;
            Security = sec;
            Time = time;
        }
    }

    #endregion


    /// <summary>
    /// The newswire actor acts as the entry point into the actor system from the Algorithm class.
    /// This actor passes the appropriate information along to the logging actor.
    /// This actor also checks the 'news' for positive stories about a security.
    /// </summary>
    public class NewswireActor : ReceiveActor
    {
        /// <summary>
        /// Constructor for the newswire actor.  The newswire actor can receive UpdateMessages.
        /// </summary>
        /// <param name="tradeActor">Reference to the trade actor that performs the trade.</param>
        /// <param name="logActor">Reference to the logging actor.</param>
        public NewswireActor(IActorRef tradeActor, IActorRef logActor)
        {
            // Handle UpdateMessage
            Receive<UpdateMessage>(am =>
            {
                logActor.Tell(am.Time);
                logActor.Tell(am.Security);

                if (HasGoodNews(am))
                {
                    tradeActor.Tell(new BuyMessage(am.Algo, am.Security));
                }
            });
        }

        // A string representing an indication from a "news story" to buy SPY.
        private const string _news =
            "{ \"Stories\": [{ \"Symbol\": \"SPY\", \"NewsDateTime\": \"10/7/2013 9:30:32 AM\"}]}";

        // Checks whether there is a buy indication from the "news"
        private bool HasGoodNews(UpdateMessage am)
        {
            News news = JsonConvert.DeserializeObject<News>(_news);

            foreach (var story in news.Stories)
            {
                if (am.Time == story.NewsDateTime)
                {
                    return true;
                }
            }

            return false;
        }
    }

    // Class to represent the "News" which is an array of Stories.
    internal class News
    {
        public IEnumerable<Story> Stories { get; internal set; }
    }

    // An story about a security.
    internal class Story
    {
        public DateTime NewsDateTime { get; set; }
        public string Symbol { get; set; }
    }
}
