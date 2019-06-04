namespace WatchMyPrices.New
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using WatchMyPrices.New.Model;

    public class WebPriceRequest
    {
        public WebPriceRequest(SqlLiteDatabase database)
        {
            this.Database = database;
        }

        public SqlLiteDatabase Database { get; }

        public PriceHistory GetPrice(string product, string site)
        {
            if (string.IsNullOrWhiteSpace(product))
            {
                throw new ArgumentNullException("Product cannot be null, empty or a whitespace for GetPrice(product, site) calls");
            }

            if (string.IsNullOrWhiteSpace(site))
            {
                throw new ArgumentNullException("Site cannot be null, empty or a whitespace for GetPrice(product, site) calls");
            }

            var productSiteInfo = this.Database.GetProductSiteInfo(product, site);


            throw new NotImplementedException();
        }
    }
}
