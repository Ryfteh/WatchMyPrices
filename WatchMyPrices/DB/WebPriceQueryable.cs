namespace WatchMyPrices.DB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using WatchMyPrices.Model;

    public class WebPriceQueryable : IPriceQueryable
    {
        public WebPriceQueryable(IWebQueryable webQueryable, IDatabase database)
        {
            this.WebQueryable = webQueryable;
            this.Database = database;
        }

        public IWebQueryable WebQueryable { get; }

        public IDatabase Database { get; }

        public IEnumerable<ProductPriceHistory> GetCurrentProductPriceHistories()
        {
            var now = DateTime.Now;
            var productOnSites = this.Database.GetSitesPerProduct();

            using (IWebDriver driver = this.WebQueryable.GetWebDriver())
            {
                foreach (var productOnSite in productOnSites)
                {
                    driver.Url = string.Format(productOnSite.Site.UrlFormat, productOnSite.Url);
                    string text;

                    try
                    {
                        var element = driver.FindElement(By.XPath(productOnSite.Site.XPath));
                        text = !string.IsNullOrWhiteSpace(element.Text) ? element.Text.Trim() : element.GetAttribute("content").Trim();
                    }
                    catch
                    {
                        // Notifications!
                        Console.WriteLine(string.Format("{0} @ {1}: Price not found!", productOnSite.Product.Name, productOnSite.Site.Name));
                        continue;
                    }
                    finally
                    {
                        driver.Close();
                    }

                    var match = Regex.Match(text, @"\d+(\.\d{2})?");

                    if (!match.Success)
                    {
                        // Notifications!
                        Console.WriteLine(string.Format("{0} @ {1}: Price not found!", productOnSite.Product.Name, productOnSite.Site.Name));
                        continue;
                    }

                    var price = decimal.Parse(match.Value);
                    /* Console.WriteLine(string.Format("{0} @ {1}: {2}", productOnSite.Product.Name, productOnSite.Site.Name, price)); */

                    var productPriceHistory = new ProductPriceHistory
                    {
                        Occurance = now,
                        Price = price,
                        ProductSite = productOnSite
                    };
                    productOnSite.ProductPriceHistories.Add(productPriceHistory);

                    yield return productPriceHistory;

                    /*var bestPrice = this.Database.GetBestPrice(productOnSite.Product.Name);
                    if (bestPrice != null && price >= bestPrice.Price)
                    {
                        continue;
                    }

                    // Notifications!
                    Console.WriteLine(string.Format("Best Current Price!"));

                    var bestHistoricalPrice = this.Database.GetBestHistoricalPrice(productOnSite.Product.Name);
                    if (bestHistoricalPrice != null && price >= bestHistoricalPrice.Price)
                    {
                        continue;
                    }

                    // Notifications!!
                    Console.WriteLine(string.Format("BEST PRICE EVER!"));*/
                }
            }
        }

        public ProductPriceHistory GetCurrentProductPriceHistory(string product, string site)
        {
            throw new NotSupportedException();
        }
    }
}
