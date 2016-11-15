using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using QuantConnect.Configuration;
using QuantConnect.Data.Market;
using QuantConnect.ToolBox;
using QuantConnect.ToolBox.QuoteBarConverter;

namespace QuantConnect.Tests.Toolbox
{
    [TestFixture]
    public class QuoteBarConverterTests
    {
        private string _destinationDirectory = Config.Get("data-source-directory", "../../../Data/DataConversionTests/");
        private string _sourceDirectory = Config.Get("data-directory", "../../../Data/");
        private Symbol _sym = new Symbol(SecurityIdentifier.GenerateForex("eurusd", "fxcm"), "eurusd");

        [TestFixtureSetUp]
        public void Setup()
        {
            Program.Main(new string[] { _destinationDirectory });
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Directory.Delete(_destinationDirectory, true);
        }

        [Test]
        public void DailyConverstionTests()
        {
            // Get daily quotebars
            var dailyQuoteBarReader = new LeanDataReader<QuoteBar>(_sym,
                                                                   Resolution.Daily,
                                                                   _destinationDirectory,
                                                                   DateTime.ParseExact("20140501", "yyyyMMdd", null));

            var dailyQuoteBars = dailyQuoteBarReader.Read();

            var from = DateTime.ParseExact("20140504", "yyyyMMdd", null);
            var thru = DateTime.ParseExact("20140509", "yyyyMMdd", null);
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            {
                var tickReader = new LeanDataReader<Tick>(_sym, Resolution.Tick, _sourceDirectory, day);
                var ticks = tickReader.Read();

                Assert.IsTrue(ticks.Count() != 0);
                Assert.IsTrue(dailyQuoteBars.Count() != 0);

                var requestedDateDailyQuoteBar = dailyQuoteBars.FirstOrDefault(x => x.Time == day.Date);
                Assert.IsNotNull(requestedDateDailyQuoteBar);

                // Bid
                Assert.AreEqual(ticks.Max(x => x.BidPrice), requestedDateDailyQuoteBar.Bid.High);
                Assert.AreEqual(ticks.Min(x => x.BidPrice), requestedDateDailyQuoteBar.Bid.Low);
                Assert.AreEqual(ticks.First().BidPrice, requestedDateDailyQuoteBar.Bid.Open);
                Assert.AreEqual(ticks.Last().BidPrice, requestedDateDailyQuoteBar.Bid.Close);

                // Ask
                Assert.AreEqual(ticks.Max(x => x.AskPrice), requestedDateDailyQuoteBar.Ask.High);
                Assert.AreEqual(ticks.Min(x => x.AskPrice), requestedDateDailyQuoteBar.Ask.Low);
                Assert.AreEqual(ticks.First().AskPrice, requestedDateDailyQuoteBar.Ask.Open);
                Assert.AreEqual(ticks.Last().AskPrice, requestedDateDailyQuoteBar.Ask.Close);
            }
        }

        [Test]
        public void HourlyConversionTests()
        {
            var requestedDate = DateTime.ParseExact("20140501", "yyyyMMdd", null);

            // Get quotebars
            var quoteBarReader = new LeanDataReader<QuoteBar>(_sym,
                                                              Resolution.Hour,
                                                              _destinationDirectory,
                                                              requestedDate);
            var hourlyQuoteBars = quoteBarReader.Read();

            var tickReader = new LeanDataReader<Tick>(_sym,
                                                      Resolution.Tick,
                                                      _sourceDirectory,
                                                      requestedDate);
            var ticks = tickReader.Read();

            Assert.IsTrue(ticks.Count() != 0);
            Assert.IsTrue(hourlyQuoteBars.Count() != 0);

            for (var hour = 0; hour <= 23; hour++)
            {
                var hourQuoteBar = hourlyQuoteBars.First(x => x.Time.Hour == hour);
                var hourTicks = ticks.Where(x => x.Time.Hour == hour);

                // Bid
                Assert.AreEqual(hourTicks.Max(x => x.BidPrice), hourQuoteBar.Bid.High);
                Assert.AreEqual(hourTicks.Min(x => x.BidPrice), hourQuoteBar.Bid.Low);
                Assert.AreEqual(hourTicks.First().BidPrice, hourQuoteBar.Bid.Open);
                Assert.AreEqual(hourTicks.Last().BidPrice, hourQuoteBar.Bid.Close);

                // Ask
                Assert.AreEqual(hourTicks.Max(x => x.AskPrice), hourQuoteBar.Ask.High);
                Assert.AreEqual(hourTicks.Min(x => x.AskPrice), hourQuoteBar.Ask.Low);
                Assert.AreEqual(hourTicks.First().AskPrice, hourQuoteBar.Ask.Open);
                Assert.AreEqual(hourTicks.Last().AskPrice, hourQuoteBar.Ask.Close);
            }
        }

        [Test]
        public void MinuteConversionTests()
        {
            var requestedDate = DateTime.ParseExact("20140501", "yyyyMMdd", null);

            // Get quotebars
            var quoteBarReader = new LeanDataReader<QuoteBar>(_sym,
                                                              Resolution.Minute,
                                                              _destinationDirectory,
                                                              requestedDate);
            var minuteQuoteBars = quoteBarReader.Read();

            var tickReader = new LeanDataReader<Tick>(_sym,
                                                      Resolution.Tick,
                                                      _sourceDirectory,
                                                      requestedDate);
            var ticks = tickReader.Read();

            Assert.IsTrue(ticks.Count() != 0);
            Assert.IsTrue(minuteQuoteBars.Count() != 0);

            for (var minute = 0; minute <= 59; minute++)
            {
                var quoteBars = minuteQuoteBars.First(x => x.Time.Minute == minute && x.Time.Hour == 7);
                var minuteTicks = ticks.Where(x => x.Time.Minute == minute && x.Time.Hour == 7);

                // Bid
                Assert.AreEqual(minuteTicks.Max(x => x.BidPrice), quoteBars.Bid.High);
                Assert.AreEqual(minuteTicks.Min(x => x.BidPrice), quoteBars.Bid.Low);
                Assert.AreEqual(minuteTicks.First().BidPrice, quoteBars.Bid.Open);
                Assert.AreEqual(minuteTicks.Last().BidPrice, quoteBars.Bid.Close);

                // Ask
                Assert.AreEqual(minuteTicks.Max(x => x.AskPrice), quoteBars.Ask.High);
                Assert.AreEqual(minuteTicks.Min(x => x.AskPrice), quoteBars.Ask.Low);
                Assert.AreEqual(minuteTicks.First().AskPrice, quoteBars.Ask.Open);
                Assert.AreEqual(minuteTicks.Last().AskPrice, quoteBars.Ask.Close);
            }
        }

        [Test]
        public void SecondConversionTests()
        {
            var requestedDate = DateTime.ParseExact("20140501", "yyyyMMdd", null);

            // Get quotebars
            var quoteBarReader = new LeanDataReader<QuoteBar>(_sym,
                                                              Resolution.Second,
                                                              _destinationDirectory,
                                                              requestedDate);
            var secondQuoteBars = quoteBarReader.Read();

            var tickReader = new LeanDataReader<Tick>(_sym,
                                                      Resolution.Tick,
                                                      _sourceDirectory,
                                                      requestedDate);
            var ticks = tickReader.Read();

            Assert.IsTrue(ticks.Count() != 0);
            Assert.IsTrue(secondQuoteBars.Count() != 0);

            for (var second = 1; second <= 59; second++)
            {
                var quoteBars = secondQuoteBars.FirstOrDefault(x => x.Time.Second == second
                                                           && x.Time.Minute == 0
                                                           && x.Time.Hour == 0);
                // There isn't data for every second
                if (quoteBars == null) continue;

                var secondTicks = ticks.Where(x => x.Time.Second == second
                                                           && x.Time.Minute == 0
                                                           && x.Time.Hour == 0);

                // Bid
                Assert.AreEqual(secondTicks.Max(x => x.BidPrice), quoteBars.Bid.High);
                Assert.AreEqual(secondTicks.Min(x => x.BidPrice), quoteBars.Bid.Low);
                Assert.AreEqual(secondTicks.First().BidPrice, quoteBars.Bid.Open);
                Assert.AreEqual(secondTicks.Last().BidPrice, quoteBars.Bid.Close);

                // Ask
                Assert.AreEqual(secondTicks.Max(x => x.AskPrice), quoteBars.Ask.High);
                Assert.AreEqual(secondTicks.Min(x => x.AskPrice), quoteBars.Ask.Low);
                Assert.AreEqual(secondTicks.First().AskPrice, quoteBars.Ask.Open);
                Assert.AreEqual(secondTicks.Last().AskPrice, quoteBars.Ask.Close);
            }
        }
    }
}