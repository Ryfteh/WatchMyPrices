namespace WatchMyPrices.New
{
    using System;
    using System.Data;
    using System.Data.SQLite;
    using WatchMyPrices.New.Model;

    public class SqlLiteDatabase
    {
        public SqlLiteDatabase(IDbConnection connection)
        {
            this.Connection = connection;
        }

        public IDbConnection Connection { get; }

        public PriceHistory GetPrice(string product, string site, DateTime searchDate)
        {
            if (string.IsNullOrWhiteSpace(product))
            {
                throw new ArgumentNullException("Product cannot be null, empty or a whitespace for GetPrice(product, site) calls");
            }

            if (string.IsNullOrWhiteSpace(site))
            {
                throw new ArgumentNullException("Site cannot be null, empty or a whitespace for GetPrice(product, site) calls");
            }

            this.Connection.Open();
            PriceHistory priceHistory;

            using (IDbCommand command = this.Connection.CreateCommand())
            {
                var sql = @"SELECT pph.Id,
		                           p.Name as ProductName,
		                           s.Name as SiteName,
		                           REPLACE(s.UrlFormat, '{0}', ps.Url) as Url,
		                           s.XPath,
		                           pph.Price,
		                           pph.Occurance,
		                           (@searchDate - pph.Occurance) as TimeFromOccurance
	                          FROM Product p,
		                           Site s,
		                           ProductSite ps,
		                           ProductPriceHistory pph
	                         WHERE p.Name = @product
	                           AND s.Name = @site
	                           AND p.Id = ps.ProductId
	                           AND s.Id = ps.SiteId
	                           AND ps.Id = pph.ProductSiteId
	                           AND TimeFromOccurance >= 0
                          ORDER BY TimeFromOccurance
                             LIMIT 1";

                command.CommandText = sql;
                command.Parameters.Add(new SQLiteParameter("@searchDate", this.DateTimeToUnixEpoch(searchDate)));
                command.Parameters.Add(new SQLiteParameter("@product", product));
                command.Parameters.Add(new SQLiteParameter("@site", site));

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    priceHistory = new PriceHistory
                    {
                        Id = reader.GetInt32(0),
                        ProducatSiteInfo = new ProductSiteInfo
                        {
                            Product = reader.GetString(1),
                            Site = reader.GetString(2),
                            Url = reader.GetString(3),
                            XPath = reader.GetString(4)
                        },
                        Price = reader.GetDecimal(5),
                        Occurance = this.UnixEpochToDateTime(reader.GetDouble(6))
                    };
                }
            }

            this.Connection.Close();

            return priceHistory;
        }

        public ProductSiteInfo GetProductSiteInfo(string product, string site)
        {
            if (string.IsNullOrWhiteSpace(product))
            {
                throw new ArgumentNullException("Product cannot be null, empty or a whitespace for GetPrice(product, site) calls");
            }

            if (string.IsNullOrWhiteSpace(site))
            {
                throw new ArgumentNullException("Site cannot be null, empty or a whitespace for GetPrice(product, site) calls");
            }

            this.Connection.Open();
            ProductSiteInfo productSiteInfo;

            using (IDbCommand command = this.Connection.CreateCommand())
            {
                var sql = @"SELECT p.Name as ProductName,
		                           s.Name as SiteName,
		                           REPLACE(s.UrlFormat, '{0}', ps.Url) as Url,
		                           s.XPath
	                          FROM Product p,
		                           Site s,
		                           ProductSite ps
	                         WHERE p.Name = @product
	                           AND s.Name = @site
	                           AND p.Id = ps.ProductId
	                           AND s.Id = ps.SiteId";

                command.CommandText = sql;
                command.Parameters.Add(new SQLiteParameter("@product", product));
                command.Parameters.Add(new SQLiteParameter("@site", site));

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    productSiteInfo = new ProductSiteInfo
                    {
                        Product = reader.GetString(1),
                        Site = reader.GetString(2),
                        Url = reader.GetString(3),
                        XPath = reader.GetString(4)
                    };
                }
            }

            this.Connection.Close();

            return productSiteInfo;
        }

        private DateTime UnixEpochToDateTime(double epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch).ToLocalTime();
        }

        private double DateTimeToUnixEpoch(DateTime dateTime)
        {
            return Math.Round((dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
        }
    }
}