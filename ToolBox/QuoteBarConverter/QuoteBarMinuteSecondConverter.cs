﻿/*
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
using System.Linq;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.Consolidators;
using QuantConnect.Data.Market;
using QuantConnect.Securities.Forex;

namespace QuantConnect.ToolBox.QuoteBarConverter
{
    /// <summary>
    /// Convert a <see cref="Tick"/> file into minute and second resolution <see cref="QuoteBar"/> files
    /// </summary>
    public class QuoteBarMinuteSecondConverter
    {
        private readonly Symbol _symbol;
        private readonly List<QuoteBar> _quoteBars;
        private readonly string _destinationPath;
        private readonly string _sourcePath;

        /// <summary>
        /// Create a new instance of the QuoteBar Converter. Parse a single input directory into an output.
        /// </summary>
        /// <param name="destinationPath">Where the data should be saved</param>
        /// <param name="sourcePath">Where the data currently is saved</param>
        /// <param name="symbol">Symbol being converted</param>
        public QuoteBarMinuteSecondConverter(string sourcePath, string destinationPath, Symbol symbol)
        {
            _symbol = symbol;
            _sourcePath = sourcePath;
            _destinationPath = destinationPath;
            _quoteBars = new List<QuoteBar>();
        }

        /// <summary>
        /// Convert <see cref="Tick"/> file into minute and second resolution <see cref="QuoteBar"/> files
        /// </summary>
        public void Convert()
        {
            var ticks = ReadTickFile(_sourcePath);

            foreach (var resolution in new List<Resolution>() { Resolution.Minute,  Resolution.Second })
            {
                var consolidator = new TickQuoteBarConsolidator(resolution.ToTimeSpan());

                consolidator.DataConsolidated += (sender, args) =>
                {
                    _quoteBars.Add(args);
                };

                var dataWriter = new LeanDataWriter(resolution, _symbol, _destinationPath);

                foreach (var tick in ticks)
                {
                    consolidator.Update(tick);
                }

                dataWriter.Write(_quoteBars.OrderBy(x => x.Time));
            }
        }

        /// <summary>
        /// Read <see cref="Tick"/> file
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns><see cref="IEnumerable{Tick}"/></returns>
        private IEnumerable<Tick> ReadTickFile(string path)
        {
            var config = new SubscriptionDataConfig(typeof(Forex),
                                                    _symbol,
                                                    Resolution.Tick,
                                                    DateTimeZone.Utc,
                                                    DateTimeZone.Utc,
                                                    false,
                                                    false,
                                                    false);

            using (var archive = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    var fileNameData = entry.FullName.Split('_');
                    var dateTime = DateTime.ParseExact(fileNameData[0], "yyyyMMdd", null);

                    using (var s = new StreamReader(entry.Open()))
                    {
                        string line;
                        while ((line = s.ReadLine()) != null)
                        {
                            yield return new Tick(config, line, dateTime);
                        }
                    }
                }
            }
        } // End ReadTickFile
    } // End class QuoteBarMinuteSecondConverter
}