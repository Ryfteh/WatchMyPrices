namespace WatchMyPrices.DB
{
    using System.Collections.Generic;
    using WatchMyPrices.Model;

    public interface IPriceQueryable
    {
        ProductPriceHistory GetCurrentProductPriceHistory(string product, string site);

        IEnumerable<ProductPriceHistory> GetCurrentProductPriceHistories();
    }
}
