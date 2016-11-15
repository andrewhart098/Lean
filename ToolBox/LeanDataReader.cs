/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Util;

namespace QuantConnect.ToolBox
{
    /// <summary>
    /// Reads data from file and returns the data as an IEnumerable of <see cref="BaseData"/>
    /// </summary>
    public class LeanDataReader<T> where T : BaseData
    {
        private readonly DateTime _date;
        private readonly Resolution _resolution;
        private readonly SubscriptionDataConfig _config;
        private readonly string _path;
        private readonly TickType _dataType;


        /// <summary>
        /// Create a new lean data reader.
        /// </summary>
        /// <param name="symbol">Symbol of data to be read</param>
        /// <param name="dataDirectory">Base data directory</param>
        /// <param name="resolution">Resolution of the data to be read</param>
        /// <param name="date">Date of the data to be read</param>
        public LeanDataReader(Symbol symbol, Resolution resolution, string dataDirectory, DateTime date)
        {
            _resolution = resolution;
            _date = date;
            _dataType = LeanData.GetCommonTickType(symbol.ID.SecurityType);

            _config = new SubscriptionDataConfig(typeof(T),
                                                 symbol,
                                                 _resolution,
                                                 DateTimeZone.Utc,
                                                 DateTimeZone.Utc,
                                                 false,
                                                 false,
                                                 false);

            _path = LeanData.GenerateZipFilePath(dataDirectory,
                                                 symbol,
                                                 _date,
                                                 _resolution,
                                                 _dataType);
        }

        /// <summary>
        /// Read the data based on <see cref="Resolution"/> and <see cref="TickType"/>
        /// </summary>
        public IEnumerable<T> Read()
        {
            if (_resolution == Resolution.Tick)
                return (IEnumerable<T>) ReadTicks();

            if (_dataType == TickType.Quote)
                return (IEnumerable<T>) ReadQuoteBars();

            if (_dataType == TickType.Trade)
                return (IEnumerable<T>) ReadTradeBars();

            throw new ArgumentException("LeanDataReader.Read(): TickType or Resolution not supported.");
        }

        /// <summary>
        /// Read CSV data formatted as <see cref="TradeBar"/>
        /// </summary>
        private IEnumerable<TradeBar> ReadTradeBars()
        {
            using (var archive = ZipFile.OpenRead(_path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    using (var s = new StreamReader(entry.Open()))
                    {
                        string line;
                        while ((line = s.ReadLine()) != null)
                        {
                            yield return TradeBar.Parse(_config, line, _date);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Read CSV data formatted as <see cref="Tick"/>
        /// </summary>
        private IEnumerable<Tick> ReadTicks()
        {
            using (var archive = ZipFile.OpenRead(_path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    using (var s = new StreamReader(entry.Open()))
                    {
                        string line;
                        while ((line = s.ReadLine()) != null)
                        {
                            yield return new Tick(_config, line, _date);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Read CSV data formatted as <see cref="QuoteBar"/>
        /// </summary>
        private IEnumerable<QuoteBar> ReadQuoteBars()
        {
            using (var archive = ZipFile.OpenRead(_path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    using (var s = new StreamReader(entry.Open()))
                    {
                        string line;
                        while ((line = s.ReadLine()) != null)
                        {
                            yield return QuoteBar.Parse(_config, line, _date);
                        }
                    }
                }
            }
        }
    }
}
