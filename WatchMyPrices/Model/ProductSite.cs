namespace WatchMyPrices.Model
{
    using System;
    using System.Collections.Generic;

    public class ProductSite : IEquatable<ProductSite>
    {
        public int? Id { get; set; }

        public Product Product { get; set; }

        public Site Site { get; set; }

        public string Url { get; set; }

        public DateTime LastCheck { get; set; }

        public IList<ProductPriceHistory> ProductPriceHistories { get; set; }

        public bool Equals(ProductSite other)
        {
            return this.Id.Equals(other.Id) &&
                this.Product.Equals(other.Product) &&
                this.Site.Equals(other.Site) &&
                this.Url.Equals(other.Url) &&
                this.LastCheck.Equals(other.LastCheck);
        }
    }
}
