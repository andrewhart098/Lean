using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp.Akka
{
    public class AkkaSymbol
    {
        public string Symbol { get; set; }
        public DateTime TickDateTime { get; set; }
    }
    public class News
    {
        public List<AkkaSymbol> Stories { get; set; }
    }
}
