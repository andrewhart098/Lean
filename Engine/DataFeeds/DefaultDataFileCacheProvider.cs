using System;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds.Transport;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    class DefaultDataFileCacheProvider : IDataFileCacheProvider
    {
        public IStreamReader Fetch(Symbol symbol, SubscriptionDataSource source, DateTime date, Resolution resolution,
            TickType tickType)
        {
            return new LocalFileSubscriptionStreamReader(source.Source);
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
