namespace WatchMyPrices.Mode
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Timers;
    using OpenQA.Selenium.Chrome;
    using WatchMyPrices.DB;

    public class WatchMode : IInteractionMode
    {
        public WatchMode(IPriceQueryable priceQueryable, IDatabase database, TimeSpan checkInterval)
        {
            this.PriceQueryable = priceQueryable;
            this.Database = database;
            this.CheckInterval = checkInterval;
        }

        public IPriceQueryable PriceQueryable { get; }

        public IDatabase Database { get; }

        public TimeSpan CheckInterval { get; }

        public void Run()
        {
            this.TimerElapsed(null, null);

            using (var timer = new System.Timers.Timer(this.CheckInterval.TotalMilliseconds))
            {
                timer.AutoReset = true;
                timer.Elapsed += this.TimerElapsed;
                timer.Start();

                Console.ReadLine();

                timer.Stop();
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var currentPrices = this.PriceQueryable.GetCurrentProductPriceHistories();

            foreach (var currentPrice in currentPrices)
            {
                Console.WriteLine(string.Format("{0} @ {1}: {2}", currentPrice.ProductSite.Product.Name, currentPrice.ProductSite.Site.Name, currentPrice.Price));
                this.Database.AddPriceForProductOnSite(currentPrice.ProductSite.Product.Name, currentPrice.ProductSite.Site.Name, currentPrice.Price, currentPrice.Occurance);
            }

            Console.WriteLine(string.Format("DONE!"));
        }
    }
}