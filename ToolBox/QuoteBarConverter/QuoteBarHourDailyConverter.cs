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
using System.Linq;
using QuantConnect.Data.Market;
using QuantConnect.Util;

namespace QuantConnect.ToolBox.QuoteBarConverter
{
    /// <summary>
    /// Converts <see cref="QuoteBar"/> data from minute to hour and daily resolution
    /// </summary>
    public class QuoteBarHourDailyConverter
    {
        private readonly Symbol _symbol;
        private readonly string _dataFolderPath;

        /// <summary>
        /// Primary constructor
        /// </summary>
        /// <param name="dataFolderPath">Path to data folder</param>
        /// <param name="symbol"><see cref="_symbol"/> to be converted</param>
        public QuoteBarHourDailyConverter(string dataFolderPath, Symbol symbol)
        {
            _symbol = symbol;
            _dataFolderPath = dataFolderPath;
        }

        /// <summary>
        /// Convert minute resolution <see cref="QuoteBar"/> data to hourly and daily resolution data
        /// </summary>
        public void Convert()
        {
            // Read all minute resolution data for a Symbol
            var quoteBars = ReadAllMinuteQuoteBars();

            // Group quotebars by date
            var quoteBarsByDate = quoteBars.GroupBy(x => x.Time.Date);


            // Convert daily data
            var dailyQuoteBars = from quoteBarGroup in quoteBarsByDate
                                 select AggregateQuoteBars(quoteBarGroup.ToList(), Resolution.Daily);
            // Write daily data
            var dailyDataWriter = new LeanDataWriter(Resolution.Daily, _symbol, _dataFolderPath);
            dailyDataWriter.Write(dailyQuoteBars);


            // Convert hourly data
            var hourlyQuoteBars = from quoteBarGroup in quoteBarsByDate
                                    // group quotebars by hour
                                    from hourlyGroupedQuoteBar in quoteBarGroup.GroupBy(x => x.Time.Hour).Select(t => t)
                                        // Aggregate quotebar hour groups
                                        select AggregateQuoteBars(hourlyGroupedQuoteBar.ToList(), Resolution.Hour);
            // Write hourly data
            var hourlyDataWriter = new LeanDataWriter(Resolution.Hour, _symbol, _dataFolderPath);
            hourlyDataWriter.Write(hourlyQuoteBars);
        }

        /// <summary>
        /// Convert a list of <see cref="QuoteBar"/> into a single <see cref="QuoteBar"/>
        /// </summary>
        /// <param name="quoteBars">A list of <see cref="QuoteBar"/> to convert</param>
        /// <param name="resolution">The <see cref="Resolution"/> of the data being aggregated</param>
        /// <returns>A single <see cref="QuoteBar"/></returns>
        private QuoteBar AggregateQuoteBars(List<QuoteBar> quoteBars, Resolution resolution)
        {
            // Ensure that the quotebars are ordered by time
            quoteBars = quoteBars.OrderBy(x => x.Time).ToList();

            var bidBar = new Bar()
            {
                High  = quoteBars.Max(x => x.Bid.High),
                Low   = quoteBars.Min(x => x.Bid.Low),
                Open  = quoteBars.First().Bid.Open,
                Close = quoteBars.Last().Bid.Close
            };

            var askBar = new Bar()
            {
                High  = quoteBars.Max(x => x.Ask.High),
                Low   = quoteBars.Min(x => x.Ask.Low),
                Open  = quoteBars.First().Ask.Open,
                Close = quoteBars.Last().Ask.Close
            };

            if (resolution == Resolution.Daily)
                return new QuoteBar(quoteBars.First().Time.Date, _symbol, bidBar, 0, askBar, 0);
            else
                return new QuoteBar(quoteBars.First().Time, _symbol, bidBar, 0, askBar, 0);
        }
        
        /// <summary>
        /// Read files that contain minute resolution <see cref="QuoteBar"/> data
        /// </summary>
        /// <returns><see cref="List{QuoteBar}"/></returns>
        private List<QuoteBar> ReadAllMinuteQuoteBars()
        {
            // Get minute resolution files
            var relativeZipFileDirectory = LeanData.GenerateRelativeZipFileDirectory(_symbol, Resolution.Minute);
            var directory = System.IO.Path.Combine(_dataFolderPath, relativeZipFileDirectory);
            var files = Directory.GetFiles(directory);

            // Read minute resolution quotebar data
            var quoteBars = new List<QuoteBar>();
            foreach (var file in files)
            {
                using (var archive = ZipFile.OpenRead(file))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        var date = DateTime.ParseExact(entry.Name.Split('_')[0], "yyyyMMdd", null);
                        using (var s = new StreamReader(entry.Open()))
                        {
                            string line;
                            while ((line = s.ReadLine()) != null)
                            {
                                quoteBars.Add(ReadStringAsQuoteBar(line, date));
                            }
                        }
                    }
                }
            }
            return quoteBars;
        }

        /// <summary>
        /// Read a line as a <see cref="QuoteBar"/> WITH NO SCALING FACTOR
        /// </summary>
        /// <param name="line">CSV line to be read</param>
        /// <param name="dateTime"><see cref="DateTime"/> the data represents</param>
        /// <returns><see cref="QuoteBar"/></returns>
        public QuoteBar ReadStringAsQuoteBar(string line, DateTime dateTime)
        {
            var quoteBar = new QuoteBar();
            var csv = line.ToCsv(10);

            var ms = csv[0];
            quoteBar.Time = dateTime.AddMilliseconds(System.Convert.ToDouble(ms));

            // only create the bid if it exists in the file
            if (csv[1].Length != 0 || csv[2].Length != 0 || csv[3].Length != 0 || csv[4].Length != 0)
            {
                quoteBar.Bid = new Bar
                {
                    Open = csv[1].ToDecimal(),
                    High = csv[2].ToDecimal(),
                    Low = csv[3].ToDecimal(),
                    Close = csv[4].ToDecimal()
                };
                quoteBar.LastBidSize = csv[5].ToInt64();
            }
            else
            {
                quoteBar.Bid = null;
            }

            // only create the ask if it exists in the file
            if (csv[6].Length != 0 || csv[7].Length != 0 || csv[8].Length != 0 || csv[9].Length != 0)
            {
                quoteBar.Ask = new Bar
                {
                    Open = csv[6].ToDecimal(),
                    High = csv[7].ToDecimal(),
                    Low = csv[8].ToDecimal(),
                    Close = csv[9].ToDecimal()
                };
                quoteBar.LastAskSize = csv[10].ToInt64();
            }
            else
            {
                quoteBar.Ask = null;
            }

            quoteBar.Value = quoteBar.Close;

            return quoteBar;
        }
    }
}
