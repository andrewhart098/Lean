using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Packets;

namespace QuantConnect.AlgorithmFactory
{
    /// <summary>
    /// This class is provided to allow to run the QCAlgorithm.Initialize method during analysis
    /// </summary>
    internal class HistoryProviderStub : IHistoryProvider
    {
        public HistoryProviderStub()
        {
        }

        public bool WasUsed { get; private set; }

        public int DataPointCount
        {
            get { return 0; }
        }

        public void Initialize(AlgorithmNodePacket job, IDataProvider dataProvider,
            IDataCacheProvider dataCacheProvider, IMapFileProvider mapFileProvider,
            IFactorFileProvider factorFileProvider, Action<int> statusUpdate)
        {
        }

        public IEnumerable<Slice> GetHistory(IEnumerable<Data.HistoryRequest> requests, DateTimeZone sliceTimeZone)
        {
            WasUsed = true;

            // send back some fake information to prevent weirdness/exceptions in algorithm.Initialize

            var random = new Random();
            var items = new List<BaseData>();
            foreach (var request in requests)
            {
                var increment = request.Resolution.ToTimeSpan().Ticks;
                increment = increment == 0 ? Time.OneSecond.Ticks : increment;
                var count = (int) ((request.EndTimeUtc - request.StartTimeUtc).Ticks / (double) increment);
                var reference = request.StartTimeUtc.ConvertFromUtc(request.TimeZone).Ticks;
                for (int i = 0; i < count; i++)
                {
                    var time = new DateTime(reference + increment * i);
                    if (request.Resolution == Resolution.Tick)
                    {
                        items.Add(new Tick(time, request.Symbol, 2 + random.Next(), 1 + random.Next()));
                    }
                    else
                    {
                        items.Add(new TradeBar(time, request.Symbol, 1 + random.Next(), 1 + random.Next(),
                            1 + random.Next(), 1 + random.Next(), 1 + random.Next()));
                    }
                }
            }
            return items.GroupBy(x => x.EndTime).Select(x => new Slice(x.Key, x));
        }
    }
}
