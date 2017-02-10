using System;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds.Transport;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    public class DefaultDataFileCacheProvider : IDataFileCacheProvider
    {
        public IStreamReader Fetch(string source, DateTime date)
        {
            return new LocalFileSubscriptionStreamReader(this, source, date);
        }

        public void Store(string source, byte[] data)
        {
            //
        }

        public void Dispose()
        {
            //
        }
    }
}
