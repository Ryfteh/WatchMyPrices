namespace WatchMyPrices.Model
{
    using System;
    using System.Collections.Generic;

    public class Site : IEquatable<Site>
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string UrlFormat { get; set; }

        public string XPath { get; set; }

        public IList<ProductSite> ProductSites { get; set; }

        public bool Equals(Site other)
        {
            return this.Id.Equals(other.Id) &&
                this.Name.Equals(other.Name) &&
                this.UrlFormat.Equals(other.UrlFormat) &&
                this.XPath.Equals(other.XPath);
        }
    }
}
