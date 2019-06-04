namespace WatchMyPrices.New.Model
{
    using System;

    public class PriceHistory
    {
        private decimal price;

        public int? Id { get; set; }

        public ProductSiteInfo ProducatSiteInfo { get; set; }

        public decimal Price
        {
            get { return this.price; }
            set { this.price = Math.Round(value, 2); }
        }

        public DateTime Occurance { get; set; }
    }
}
