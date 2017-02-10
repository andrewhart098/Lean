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
using System.IO;
using Ionic.Zip;
using QuantConnect.Interfaces;

namespace QuantConnect.Lean.Engine.DataFeeds
{
    /// <summary>
    /// Default file provider functionality that does not attempt to retrieve any data
    /// </summary>
    public class DefaultDataProvider : IDataProvider, IDisposable
    {
        private ZipFile _zipFile;

        /// <summary>
        /// Read data from disc
        /// </summary>
        public Stream Fetch(string source, string entryName)
        {
            return source.GetExtension() == ".zip"
                ? Compression.UnzipBaseStream(source, entryName, out _zipFile)
                : new FileStream(source, FileMode.Open, FileAccess.Read);
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
