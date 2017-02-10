using System;
using System.IO;
using Ionic.Zip;
using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds.Transport;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    public class DefaultDataFileCacheProvider : IDataFileCacheProvider
    {
        private ZipFile _zipFile;

        public Stream Fetch(string source, DateTime date, string entryName)
        {
           return source.GetExtension() == ".zip"
                ? Compression.UnzipBaseStream(source, entryName, out _zipFile)
                : new FileStream(source, FileMode.Open, FileAccess.Read);
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
