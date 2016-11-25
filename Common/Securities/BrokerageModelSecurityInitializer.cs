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
 *
*/

using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Interfaces;
using QuantConnect.Logging;

namespace QuantConnect.Securities
{
    /// <summary>
    /// Provides an implementation of <see cref="ISecurityInitializer"/> that initializes a security
    /// by settings the <see cref="Security.FillModel"/>, <see cref="Security.FeeModel"/>, 
    /// <see cref="Security.SlippageModel"/>, and the <see cref="Security.SettlementModel"/> properties
    /// </summary>
    public class BrokerageModelSecurityInitializer : ISecurityInitializer
    {
        private readonly IBrokerageModel _brokerageModel;
        private readonly IHistoryProvider _historyProvider;
        private readonly TimeKeeper _timeKeeper;
        private readonly LocalTimeKeeper _localTimeKeeper;
        private readonly bool _isLiveMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrokerageModelSecurityInitializer"/> class
        /// for the specified algorithm
        /// </summary>
        /// <param name="brokerageModel">The brokerage model used to initialize the security models</param>
        /// <param name="historyProvider"><see cref="IHistoryProvider"/> for the algorithm</param>
        /// <param name="timeKeeper">Used to get the current <see cref="DateTime"/> in UTC</param>
        /// <param name="localTimeKeeper">Gets the local algorithm time</param>
        /// <param name="isLiveMode">Indicates whether the algorithm is Live</param>
        public BrokerageModelSecurityInitializer(IBrokerageModel brokerageModel,
                                                 IHistoryProvider historyProvider,
                                                 TimeKeeper timeKeeper,
                                                 LocalTimeKeeper localTimeKeeper,
                                                 bool isLiveMode)
        {
            _brokerageModel = brokerageModel;
            _historyProvider = historyProvider;
            _timeKeeper = timeKeeper;
            _localTimeKeeper = localTimeKeeper;
            _isLiveMode = isLiveMode;
        }

        /// <summary>
        /// Initializes the specified security by setting up the models
        /// </summary>
        /// <param name="security">The security to be initialized</param>
        public virtual void Initialize(Security security)
        {
            // set leverage and models
            security.SetLeverage(_brokerageModel.GetLeverage(security));
            security.FillModel = _brokerageModel.GetFillModel(security);
            security.FeeModel = _brokerageModel.GetFeeModel(security);
            security.SlippageModel = _brokerageModel.GetSlippageModel(security);
            security.SettlementModel = _brokerageModel.GetSettlementModel(security, _brokerageModel.AccountType);

            // Seed the correct price from the history provider if needed
            if (_isLiveMode 
                && security.Price == 0
                && _brokerageModel.GetSupportedSecurityTypes().Contains(security.Symbol.ID.SecurityType))
            {
                var request = CreateSingleBarCountHistoryRequests(security);
                var history = _historyProvider.GetHistory(new List<HistoryRequest>() { request }, _localTimeKeeper.TimeZone);

                if (history.Any())
                    security.SetMarketPrice(history.First().Values.First());
                else
                    Log.Trace("BrokerageSecurityModel.Initialize(): Could not seed price for security {0} from history.",
                              security.Symbol.ID.Symbol);
            }
        }

        /// <summary>
        /// Create a <see cref="HistoryRequest"/> for 1 bar
        /// </summary>
        /// <param name="security">The <see cref="Security"/> for which the history is requested</param>
        /// <returns><see cref="HistoryRequest"/></returns>
        private HistoryRequest CreateSingleBarCountHistoryRequests(Security security)
        {
            var timeSpan = security.Resolution.ToTimeSpan() < Time.OneSecond ? Time.OneSecond : security.Resolution.ToTimeSpan();
            var localStartTime = Time.GetStartTimeForTradeBars(security.Exchange.Hours,
                                                               _timeKeeper.UtcTime.ConvertFromUtc(security.Exchange.TimeZone),
                                                               timeSpan,
                                                               1,
                                                               security.IsExtendedMarketHours);
            var start = localStartTime.ConvertTo(security.Exchange.TimeZone, _localTimeKeeper.TimeZone);

            return CreateHistoryRequest(security,
                                        start,
                                        _localTimeKeeper.LocalTime.RoundDown(security.Resolution.ToTimeSpan()));
        }

        /// <summary>
        /// Create the <see cref="HistoryRequest"/>
        /// </summary>
        /// <param name="security">The <see cref="Security"/> for which the history is requested</param>
        /// <param name="startAlgoTz">Start time of the <see cref="HistoryRequest"/></param>
        /// <param name="endAlgoTz">End time of the <see cref="HistoryRequest"/></param>
        /// <returns><see cref="HistoryRequest"/></returns>
        private HistoryRequest CreateHistoryRequest(Security security, DateTime startAlgoTz, DateTime endAlgoTz)
        {
            return new HistoryRequest()
            {
                StartTimeUtc = startAlgoTz.ConvertToUtc(_localTimeKeeper.TimeZone),
                EndTimeUtc = endAlgoTz.ConvertToUtc(_localTimeKeeper.TimeZone),
                DataType = security.Resolution == Resolution.Tick ? typeof(Tick) : typeof(TradeBar),
                Resolution = security.Resolution,
                FillForwardResolution = security.Resolution,
                Symbol = security.Symbol,
                Market = security.Symbol.ID.Market,
                ExchangeHours = security.Exchange.Hours
            };
        }
    }
}
