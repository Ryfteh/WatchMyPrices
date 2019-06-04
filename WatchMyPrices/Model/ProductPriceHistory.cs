namespace WatchMyPrices.Model
{
    using System;

    public class ProductPriceHistory : IEquatable<ProductPriceHistory>
    {
        private decimal price;

        public int? Id { get; set;  }

        public ProductSite ProductSite { get; set; }

        public decimal Price
        {
            get { return this.price; }
            set { this.price = Math.Round(value, 2); }
        }

        public DateTime Occurance { get; set; }

        public bool Equals(ProductPriceHistory other)
        {
            return this.Id.Equals(other.Id) &&
                this.ProductSite.Equals(other.ProductSite) &&
                this.Price.Equals(other.Price) &&
                this.Occurance.Equals(other.Occurance);
        }
    }
}
