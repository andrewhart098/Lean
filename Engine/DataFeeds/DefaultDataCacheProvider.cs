using System.IO;
using Ionic.Zip;
using QuantConnect.Interfaces;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    public class DefaultDataCacheProvider : IDataCacheProvider
    {
        private ZipFile _zipFile;
        private IDataProvider _dataProvider;

        public DefaultDataCacheProvider(IDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public Stream Fetch(string source, string entryName)
        {
            return _dataProvider.Fetch(source, entryName);
        }

        public void Store(string source, byte[] data)
        {
            // NOP
        }

        public void Dispose()
        {
            if (_zipFile != null)
            {
                _zipFile.Dispose();
            }
        }
    }
}
