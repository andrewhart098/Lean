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

using System.IO;
using System.Threading.Tasks;
using QuantConnect.Logging;

namespace QuantConnect.ToolBox.QuoteBarConverter
{
    public class Program
    {
        /// <summary>
        /// QuoteBar Converter: Convert QuantConnect Ticks into QuantConnect QuoteBars 
        /// at second, minute, hour and daily resolution
        /// </summary>
        public static void Main()
        {
            var sourceDirectory = @"C:\Users\Roo\Source\Repos\Lean\Data\forex\fxcm\tick";
            var destinationPath = @"C:\Users\Roo\Documents\LEAN\QuoteBarData\";

            Log.Trace("QuoteBarConverter.Main(): Beginning to convert tick data into minute and second quotebars.");
            var tickZipFiles = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories);
            Parallel.ForEach(tickZipFiles, file =>
            {
                var fileNameData = file.Split('\\');
                var permtick = fileNameData[fileNameData.Length - 2];
                var symbol = new Symbol(SecurityIdentifier.GenerateForex(permtick, "usa"), permtick);
                var quoteBarConverter = new QuoteBarMinuteSecondConverter(file, destinationPath, symbol);

                quoteBarConverter.Convert();
            });
            Log.Trace("QuoteBarConverter.Main(): Done converting minute and second resolution data.");


            Log.Trace("QuoteBarConverter.Main(): Beginning to create hour and daily resolution quotebars.");
            var topLevelTickDirectories = Directory.GetDirectories(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var directory in topLevelTickDirectories)
            {
                var fileNameData = directory.Split('\\');
                var permtick = fileNameData[fileNameData.Length - 1];
                var symbol = new Symbol(SecurityIdentifier.GenerateForex(permtick, "usa"), permtick);
                var quoteBarConverter = new QuoteBarHourDailyConverter(destinationPath, symbol);

                quoteBarConverter.Convert();
            }
            Log.Trace("QuoteBarConverter.Main(): Done converting to hour and daily resolution data.");

            Log.Trace("QuoteBarConverter.Main(): Done converting tick data. Exiting.");
        }
    }
}
