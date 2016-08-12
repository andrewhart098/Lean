using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp.Akka.Messages
{
    public class AlgoMessage
    {
        public BasicTemplateAlgorithm Algo { get; private set; }
        public Security Security { get; private set; }
        public DateTime Time { get; private set; }

        public AlgoMessage(BasicTemplateAlgorithm algo, Security sec, DateTime time)
        {
            Algo = algo;
            Security = sec;
            Time = time;
        }
    }
}
