namespace WatchMyPrices.DB
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using WatchMyPrices.Model;

    public class SQLite : IDatabase
    {
        public SQLite(string path)
        {
            this.Path = path;
            this.ConnectionString = string.Format("Data Source={0};Version=3;", path);
            this.Initialize();
        }

        public string ConnectionString { get; private set; }

        public string Path { get; private set; }

        public long AddPriceForProductOnSite(string productName, string siteName, decimal price, DateTime occurance)
        {
            var productSiteId = this.GetProductOnSiteId(productName, siteName);
            if (productSiteId < 0)
            {
                throw new NotImplementedException();
            }

            int numRowsInserted;

            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"INSERT INTO ProductPriceHistory (
                                ProductSiteId,
                                Price,
                                Occurance
                            ) VALUES (
                                @productSiteId,
                                @price,
                                @occurance
                            )";
                var productSiteParam = new SQLiteParameter("@productSiteId", productSiteId);
                var priceParam = new SQLiteParameter("@price", Math.Round(price, 2));
                var occuranceParam = new SQLiteParameter("@occurance", this.DateTimeToUnixEpoch(occurance));

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add(productSiteParam);
                    command.Parameters.Add(priceParam);
                    command.Parameters.Add(occuranceParam);
                    numRowsInserted = command.ExecuteNonQuery();

                    connection.Close();
                }
            }

            if (numRowsInserted <= 0)
            {
                throw new NotSupportedException();
            }

            var productPriceHistoryId = this.GetProductOnSiteId(productName, siteName);

            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"UPDATE ProductSite
                               SET LastCheck = @occurance
                             WHERE Id = @productSiteId";

                var productSiteParam = new SQLiteParameter("@productSiteId", productSiteId);
                var occuranceParam = new SQLiteParameter("@occurance", this.DateTimeToUnixEpoch(occurance));

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add(productSiteParam);
                    command.Parameters.Add(occuranceParam);
                    numRowsInserted = command.ExecuteNonQuery();

                    connection.Close();
                }
            }

            if (numRowsInserted <= 0)
            {
                throw new NotSupportedException();
            }

            return productPriceHistoryId;
        }

        public long GetProductId(string name)
        {
            object sqlValue;

            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"SELECT Id
                              FROM Product
                             WHERE Name = @productName";
                var parameter = new SQLiteParameter("@productName", name);

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add(parameter);
                    sqlValue = command.ExecuteScalar();

                    connection.Close();
                }
            }

            return sqlValue != null ? (long)sqlValue : -1;
        }

        public long GetProductOnSiteId(string productName, string siteName)
        {
            var siteId = this.GetSiteId(siteName);

            if (siteId == -1)
            {
                throw new NotSupportedException();
            }

            var productId = this.GetProductId(productName);

            if (productId == -1)
            {
                throw new NotSupportedException();
            }

            object sqlValue;

            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"SELECT Id
                              FROM ProductSite
                             WHERE ProductId = @productId
                                AND SiteId = @siteId";
                var productParameter = new SQLiteParameter("@productId", productId);
                var siteParameter = new SQLiteParameter("@siteId", siteId);

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add(productParameter);
                    command.Parameters.Add(siteParameter);
                    sqlValue = command.ExecuteScalar();

                    connection.Close();
                }
            }

            return sqlValue != null ? (long)sqlValue : -1;
        }

        public long GetSiteId(string name)
        {
            object sqlValue;

            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"SELECT Id
                              FROM Site
                             WHERE Name = @siteName";
                var parameter = new SQLiteParameter("@siteName", name);

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add(parameter);
                    sqlValue = command.ExecuteScalar();

                    connection.Close();
                }
            }

            return sqlValue != null ? (long)sqlValue : -1;
        }

        public void Initialize()
        {
            if (!File.Exists(this.Path))
            {
                SQLiteConnection.CreateFile(this.Path);
            }

            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                connection.Open();
                string sql;
                SQLiteCommand command;

                sql = @"CREATE TABLE IF NOT EXISTS Product(
                            Id INTEGER PRIMARY KEY,
                            Name TEXT
                        )";

                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                command.Dispose();

                sql = @"CREATE TABLE IF NOT EXISTS Site(
                            Id INTEGER PRIMARY KEY,
                            Name TEXT,
                            XPath TEXT,
                            UrlFormat TEXT
                        )";

                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                command.Dispose();

                sql = @"CREATE TABLE IF NOT EXISTS ProductSite(
                            Id INTEGER PRIMARY KEY,
                            ProductId INTEGER,
                            SiteId INTEGER,
                            Url TEXT,
                            LastCheck INTEGER,
                            FOREIGN KEY(ProductId) REFERENCES Product(Id),
                            FOREIGN KEY(SiteId) REFERENCES Site(Id),
                            UNIQUE (ProductId, SiteId)
                        )";

                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                command.Dispose();

                sql = @"CREATE TABLE IF NOT EXISTS ProductPriceHistory(
                            Id INTEGER PRIMARY KEY,
                            ProductSiteId INTEGER,
                            Price REAL,
                            Occurance INTEGER,
                            FOREIGN KEY(ProductSiteId) REFERENCES ProductSite(Id)
                        )";

                command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                command.Dispose();

                connection.Close();
            }
        }

        public IEnumerable<ProductPriceHistory> GetPrices(string productName)
        {
            var pphList = new List<ProductPriceHistory>();
            var psList = new List<ProductSite>();
            var pList = new List<Product>();
            var sList = new List<Site>();

            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"SELECT ps.ProductId,
                                   p.Name as ProductName,
                                   ps.SiteId,
                                   s.Name as SiteName,
                                   s.UrlFormat,
                                   s.XPath,
                                   ps.Id as ProductSiteId,
                                   ps.Url,
                                   ps.LastCheck,
                                   pph.Id as ProductPriceHistoryId,
                                   pph.Price,
                                   pph.Occurance
                              FROM Product p,
                                   Site s,
                                   ProductSite ps,
                                   ProductPriceHistory pph
                             WHERE p.Name = @productName
                               AND p.Id = ps.ProductId
                               AND s.Id = ps.SiteId
                               AND ps.Id = pph.ProductSiteId";

                var productParameter = new SQLiteParameter("@productName", productName);

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add(productParameter);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var site = sList.Where(s => s.Id == reader.GetInt32(2)).FirstOrDefault();
                            if (site == null)
                            {
                                site = new Site
                                {
                                    Id = reader.GetInt32(2),
                                    Name = reader.GetString(3),
                                    UrlFormat = reader.GetString(4),
                                    XPath = reader.GetString(5),
                                    ProductSites = new List<ProductSite>()
                                };
                                sList.Add(site);
                            }

                            var product = pList.Where(s => s.Id == reader.GetInt32(0)).FirstOrDefault();
                            if (product == null)
                            {
                                product = new Product
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    ProductSites = new List<ProductSite>()
                                };
                                pList.Add(product);
                            }

                            var productSite = psList.Where(ps => ps.Id == reader.GetInt32(6)).FirstOrDefault();
                            if (productSite == null)
                            {
                                productSite = new ProductSite
                                {
                                    Id = reader.GetInt32(6),
                                    Url = reader.GetString(7),
                                    ProductPriceHistories = new List<ProductPriceHistory>(),
                                    LastCheck = this.UnixEpochToDateTime(reader.GetDouble(8)),
                                    Site = site,
                                    Product = product
                                };
                                psList.Add(productSite);
                            }

                            var productPriceHistory = new ProductPriceHistory
                            {
                                Id = reader.GetInt32(9),
                                Price = reader.GetDecimal(10),
                                Occurance = this.UnixEpochToDateTime(reader.GetDouble(11)),
                                ProductSite = productSite
                            };
                            pphList.Add(productPriceHistory);

                            if (!productSite.ProductPriceHistories.Any(pph => pph.Id == productPriceHistory.Id))
                            {
                                productSite.ProductPriceHistories.Add(productPriceHistory);
                            }

                            if (!product.ProductSites.Any(ps => ps.Id == productSite.Id))
                            {
                                product.ProductSites.Add(productSite);
                            }

                            if (!site.ProductSites.Any(ps => ps.Id == productSite.Id))
                            {
                                site.ProductSites.Add(productSite);
                            }
                        }
                    }

                    connection.Close();
                }
            }

            return pphList;
        }

        public ProductPriceHistory GetBestPrice(string productName)
        {
            return this.GetPrices(productName)
                .GroupBy(pph => pph.ProductSite.Site.Id)
                .Select(grp => grp.OrderByDescending(pph => pph.Occurance).First())
                .OrderBy(pph => pph.Price)
                .FirstOrDefault();
        }

        public ProductPriceHistory GetBestHistoricalPrice(string productName)
        {
            return this.GetPrices(productName).OrderBy(pph => pph.Price).FirstOrDefault();
        }

        public IEnumerable<string> GetProductNames()
        {
            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"SELECT Name
                              FROM Product";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return reader.GetString(0);
                        }
                    }

                    connection.Close();
                }
            }
        }

        public IEnumerable<string> GetSiteNames()
        {
            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"SELECT Name
                              FROM Site";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return reader.GetString(0);
                        }
                    }

                    connection.Close();
                }
            }
        }

        public IEnumerable<ProductSite> GetSitesPerProduct()
        {
            var psList = new List<ProductSite>();
            var pList = new List<Product>();
            var sList = new List<Site>();

            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"SELECT ps.ProductId,
                                   p.Name as ProductName,
                                   ps.SiteId,
                                   s.Name as SiteName,
                                   s.UrlFormat,
                                   s.XPath,
                                   ps.Id as ProductSiteId,
                                   ps.Url,
                                   ps.LastCheck
                              FROM Product p,
                                   Site s,
                                   ProductSite ps
                             WHERE p.Id = ps.ProductId
                               AND s.Id = ps.SiteId";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var site = sList.Where(s => s.Id == reader.GetInt32(2)).FirstOrDefault();
                            if (site == null)
                            {
                                site = new Site
                                {
                                    Id = reader.GetInt32(2),
                                    Name = reader.GetString(3),
                                    UrlFormat = reader.GetString(4),
                                    XPath = reader.GetString(5),
                                    ProductSites = new List<ProductSite>()
                                };
                                sList.Add(site);
                            }

                            var product = pList.Where(s => s.Id == reader.GetInt32(0)).FirstOrDefault();
                            if (product == null)
                            {
                                product = new Product
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    ProductSites = new List<ProductSite>()
                                };
                                pList.Add(product);
                            }

                            var productSite = new ProductSite
                            {
                                Id = reader.GetInt32(6),
                                Url = reader.GetString(7),
                                ProductPriceHistories = null,
                                LastCheck = this.UnixEpochToDateTime(reader.GetDouble(8)),
                                Site = site,
                                Product = product
                            };
                            psList.Add(productSite);

                            if (!product.ProductSites.Any(ps => ps.Id == productSite.Id))
                            {
                                product.ProductSites.Add(productSite);
                            }

                            if (!site.ProductSites.Any(ps => ps.Id == productSite.Id))
                            {
                                site.ProductSites.Add(productSite);
                            }
                        }
                    }

                    connection.Close();
                }
            }

            return psList;
        }

        public ProductPriceHistory GetCurrentProductPriceHistory(string product, string site)
        {
            return this.GetCurrentProductPriceHistories()
                .Where(pph => pph.ProductSite.Site.Name.Equals(site, StringComparison.InvariantCultureIgnoreCase)
                    && pph.ProductSite.Product.Name.Equals(product, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();
        }

        public IEnumerable<ProductPriceHistory> GetCurrentProductPriceHistories()
        {
            var pphList = new List<ProductPriceHistory>();
            var psList = new List<ProductSite>();
            var pList = new List<Product>();
            var sList = new List<Site>();

            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"SELECT ps.ProductId,
	                               p.Name as ProductName,
	                               ps.SiteId,
	                               s.Name as SiteName,
	                               s.UrlFormat,
	                               s.XPath,
	                               ps.Id as ProductSiteId,
	                               ps.Url,
	                               ps.LastCheck,
	                               pph.Id as ProductPriceHistoryId,
	                               pph.Price,
	                               MAX(pph.Occurance)
                              FROM Product p,
	                               Site s,
	                               ProductSite ps,
	                               ProductPriceHistory pph
                             WHERE p.Id = ps.ProductId
                               AND s.Id = ps.SiteId
                               AND ps.Id = pph.ProductSiteId
                          GROUP BY ps.Id";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var site = sList.Where(s => s.Id == reader.GetInt32(2)).FirstOrDefault();
                            if (site == null)
                            {
                                site = new Site
                                {
                                    Id = reader.GetInt32(2),
                                    Name = reader.GetString(3),
                                    UrlFormat = reader.GetString(4),
                                    XPath = reader.GetString(5),
                                    ProductSites = new List<ProductSite>()
                                };
                                sList.Add(site);
                            }

                            var product = pList.Where(s => s.Id == reader.GetInt32(0)).FirstOrDefault();
                            if (product == null)
                            {
                                product = new Product
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    ProductSites = new List<ProductSite>()
                                };
                                pList.Add(product);
                            }

                            var productSite = psList.Where(ps => ps.Id == reader.GetInt32(6)).FirstOrDefault();
                            if (productSite == null)
                            {
                                productSite = new ProductSite
                                {
                                    Id = reader.GetInt32(6),
                                    Url = reader.GetString(7),
                                    ProductPriceHistories = new List<ProductPriceHistory>(),
                                    LastCheck = this.UnixEpochToDateTime(reader.GetDouble(8)),
                                    Site = site,
                                    Product = product
                                };
                                psList.Add(productSite);
                            }

                            var productPriceHistory = new ProductPriceHistory
                            {
                                Id = reader.GetInt32(9),
                                Price = reader.GetDecimal(10),
                                Occurance = this.UnixEpochToDateTime(reader.GetDouble(11)),
                                ProductSite = productSite
                            };
                            pphList.Add(productPriceHistory);

                            if (!productSite.ProductPriceHistories.Any(pph => pph.Id == productPriceHistory.Id))
                            {
                                productSite.ProductPriceHistories.Add(productPriceHistory);
                            }

                            if (!product.ProductSites.Any(ps => ps.Id == productSite.Id))
                            {
                                product.ProductSites.Add(productSite);
                            }

                            if (!site.ProductSites.Any(ps => ps.Id == productSite.Id))
                            {
                                site.ProductSites.Add(productSite);
                            }
                        }
                    }

                    connection.Close();
                }
            }

            return pphList;
        }

        public IEnumerable<string> GetSitesForProduct(string product)
        {
            using (var connection = new SQLiteConnection(this.ConnectionString))
            {
                var sql = @"SELECT DISTINCT s.Name
                              FROM Product p,
	                               Site s,
	                               ProductSite ps,
	                               ProductPriceHistory pph
                             WHERE p.Name = @productName
                               AND p.Id = ps.ProductId
                               AND s.Id = ps.SiteId
                               AND ps.Id = pph.ProductSiteId";
                var productParameter = new SQLiteParameter("@productName", product);

                using (var command = new SQLiteCommand(sql, connection))
                {
                    connection.Open();

                    command.Parameters.Add(productParameter);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return reader.GetString(0);
                        }
                    }

                    connection.Close();
                }
            }
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
