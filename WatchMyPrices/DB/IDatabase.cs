namespace WatchMyPrices.DB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WatchMyPrices.Model;

    public interface IDatabase : IPriceQueryable
    {
        void Initialize();

        long AddPriceForProductOnSite(string productName, string siteName, decimal price, DateTime occurance);

        long GetSiteId(string name);

        long GetProductId(string name);

        long GetProductOnSiteId(string productName, string siteName);

        IEnumerable<ProductPriceHistory> GetPrices(string productName);

        ProductPriceHistory GetBestPrice(string productName);

        ProductPriceHistory GetBestHistoricalPrice(string productName);

        IEnumerable<string> GetProductNames();

        IEnumerable<string> GetSiteNames();

        IEnumerable<ProductSite> GetSitesPerProduct();

        IEnumerable<string> GetSitesForProduct(string product);
    }
}
