using System;
using System.Linq;
using NodaTime;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Market;

namespace QuantConnect.Tests.Common.Data
{
    [TestFixture]
    public class SubscriptionManagerTests
    {
        private readonly Symbol _forexSym  = Symbol.Create("EURUSD", SecurityType.Forex, "usa");
        private readonly Symbol _equitySym = Symbol.Create("AAPL", SecurityType.Equity, "usa");
        private readonly Symbol _cfdSym    = Symbol.Create("abc", SecurityType.Cfd, "usa");

        [Test]
        public void SubscriptionManager_AddsNewForexSubscription_WithTypeQuoteBar()
        {
            var subscriptionManager = new SubscriptionManager(new TimeKeeper(DateTime.Now));
            // 1st Add() method
            subscriptionManager.Add(typeof(QuoteBar), _forexSym, Resolution.Hour,
                DateTimeZone.Utc, DateTimeZone.Utc, false);

            Assert.IsTrue(subscriptionManager.Subscriptions.Any());

            var subscription = subscriptionManager.Subscriptions.First();

            Assert.IsTrue(subscription.SecurityType == SecurityType.Forex);
            Assert.IsTrue(subscription.Type == typeof(QuoteBar));
        }


        [Test]
        public void SubscriptionManager_AddsNewForexSecurity_WithTypeQuoteBar()
        {
            var subscriptionManager = new SubscriptionManager(new TimeKeeper(DateTime.Now));
            // 2nd Add() method
            subscriptionManager.Add(_forexSym, Resolution.Daily, DateTimeZone.Utc, DateTimeZone.Utc);

            Assert.IsTrue(subscriptionManager.Subscriptions.Any());

            var subscription = subscriptionManager.Subscriptions.First();

            Assert.IsTrue(subscription.SecurityType == SecurityType.Forex);
            Assert.IsTrue(subscription.Type == typeof(QuoteBar));
        }

        [Test]
        public void SubscriptionManager_AddsNewCfdSubscription_WithTypeQuoteBar()
        {
            var subscriptionManager = new SubscriptionManager(new TimeKeeper(DateTime.Now));
            // 1st Add() method
            subscriptionManager.Add(typeof(QuoteBar), _cfdSym, Resolution.Hour,
                DateTimeZone.Utc, DateTimeZone.Utc, false);

            Assert.IsTrue(subscriptionManager.Subscriptions.Any());

            var subscription = subscriptionManager.Subscriptions.First();

            Assert.IsTrue(subscription.SecurityType == SecurityType.Cfd);
            Assert.IsTrue(subscription.Type == typeof(QuoteBar));
        }


        [Test]
        public void SubscriptionManager_AddsNewCfdSecurity_WithTypeQuoteBar()
        {
            var subscriptionManager = new SubscriptionManager(new TimeKeeper(DateTime.Now));
            // 2nd Add() method
            subscriptionManager.Add(_cfdSym, Resolution.Minute, DateTimeZone.Utc, DateTimeZone.Utc);

            Assert.IsTrue(subscriptionManager.Subscriptions.Any());

            var subscription = subscriptionManager.Subscriptions.First();

            Assert.IsTrue(subscription.SecurityType == SecurityType.Cfd);
            Assert.IsTrue(subscription.Type == typeof(QuoteBar));
        }

        [Test]
        public void SubscriptionManager_AddsNewEquitySubscription_WithTypeTradeBar()
        {
            var subscriptionManager = new SubscriptionManager(new TimeKeeper(DateTime.Now));
            // 1st Add() method
            subscriptionManager.Add(typeof(TradeBar), _equitySym, Resolution.Hour,
                DateTimeZone.Utc, DateTimeZone.Utc, false);

            Assert.IsTrue(subscriptionManager.Subscriptions.Any());

            var subscription = subscriptionManager.Subscriptions.First();

            Assert.IsTrue(subscription.SecurityType == SecurityType.Equity);
            Assert.IsTrue(subscription.Type == typeof(TradeBar));
        }


        [Test]
        public void SubscriptionManager_AddsNewEquitySecurity_WithTypeTradeBar()
        {
            var subscriptionManager = new SubscriptionManager(new TimeKeeper(DateTime.Now));
            // 2nd Add() method
            subscriptionManager.Add(_equitySym, Resolution.Minute, DateTimeZone.Utc, DateTimeZone.Utc);

            Assert.IsTrue(subscriptionManager.Subscriptions.Any());

            var subscription = subscriptionManager.Subscriptions.First();

            Assert.IsTrue(subscription.SecurityType == SecurityType.Equity);
            Assert.IsTrue(subscription.Type == typeof(TradeBar));
        }
    }
}
