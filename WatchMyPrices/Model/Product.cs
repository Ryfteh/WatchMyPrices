namespace WatchMyPrices.Model
{
    using System;
    using System.Collections.Generic;

    public class Product : IEquatable<Product>
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public IList<ProductSite> ProductSites { get; set; }

        public bool Equals(Product other)
        {
            return this.Id.Equals(other.Id) &&
                this.Name.Equals(other.Name);
        }
    }
}
