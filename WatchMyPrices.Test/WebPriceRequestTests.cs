namespace WatchMyPrices.Test
{
    using System;
    using NSubstitute.ExceptionExtensions;
    using NUnit.Framework;
    using WatchMyPrices.New;

    [TestFixture]
    public class WebPriceRequestTests
    {
        [Test]
        public void GetPrice_ProductIsEmptyOrNull_ThrowsError()
        {
            Assert.Throws<ArgumentNullException>(() => new WebPriceRequest(null).GetPrice(null, "x"));
        }

        [Test]
        public void GetPrice_SiteIsEmptyOrNull_ThrowsError()
        {
            Assert.Throws<ArgumentNullException>(() => new WebPriceRequest(null).GetPrice("x", null));
        }

        [Test]
        public void GetPrice_PriceExists_ReturnsPrice()
        {
            var price = 3.50M;
            var webPriceRequest = new WebPriceRequest(null);
            Assert.That(price == webPriceRequest.GetPrice(null, null).Price);
        }

        [Test]
        public void GetPrice_PriceNotExists_NotReturnsPrice()
        {
            var webPriceRequest = new WebPriceRequest(null);
            Assert.That(webPriceRequest.GetPrice(null, null) == null);
        }
    }
}
