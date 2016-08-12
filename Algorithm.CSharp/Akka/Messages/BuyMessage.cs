using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Securities;

namespace QuantConnect.Algorithm.CSharp.Akka.Messages
{
    public class BuyMessage 
    {
        public BasicTemplateAlgorithm Algo { get; private set; }
        public Security Security { get; private set; }
        public bool Buy { get; set; }

        public BuyMessage(BasicTemplateAlgorithm algo, Security sec, bool buy)
        {
            Algo = algo;
            Security = sec;
            Buy = buy;
        }
    }
}
