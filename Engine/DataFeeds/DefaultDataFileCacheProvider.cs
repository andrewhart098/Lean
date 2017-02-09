using System;
using System.IO;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds.Transport;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    public class DefaultDataFileCacheProvider : IDataFileCacheProvider
    {
        public byte[] Fetch(string source, DateTime date)
        {
            return File.ReadAllBytes(source);
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
