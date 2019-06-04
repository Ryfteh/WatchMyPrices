namespace WatchMyPrices.Mode
{
    using System;
    using System.Linq;
    using WatchMyPrices.DB;

    public class InteractiveMode : IInteractionMode
    {
        public InteractiveMode(IDatabase database)
        {
            this.Database = database;
        }

        public IDatabase Database { get; }

        public void Run()
        {
            string valueRead;

            var productNames = this.Database.GetProductNames();
            var leftPadProducts = productNames.Max(p => p.Length) + 1;

            var siteNames = this.Database.GetSiteNames();
            var leftPadSites = siteNames.Max(s => s.Length) + 1;

            Console.WriteLine(string.Format("        | {0," + -leftPadProducts + "}| {1,-7} @ {2," + -leftPadSites + "} | {3,-12} @ {4," + -leftPadSites + "} ({5})", "Product", "Price", "Site", "Best Price", "Site", "Date"));
            Console.WriteLine(string.Format("--------+-{0}+-{1}--{2}--+{3}--{4}--{5}", new string('-', leftPadProducts), new string('-', 7), new string('-', leftPadSites), new string('-', 12), new string('-', leftPadSites), new string('-', 22)));

            foreach (var productName in productNames)
            {
                var bestPriceNow = this.Database.GetBestPrice(productName);
                var bestPriceEver = this.Database.GetBestHistoricalPrice(productName);

                var bestPriceNowPrice = bestPriceNow == null ? string.Empty : string.Format("${0}", bestPriceNow.Price);
                var bestPriceNowName = bestPriceNow == null ? string.Empty : bestPriceNow.ProductSite.Site.Name;

                var bestPriceEverPrice = bestPriceEver == null ? string.Empty : string.Format("${0}", bestPriceEver.Price);
                var bestPriceEverName = bestPriceEver == null ? string.Empty : bestPriceEver.ProductSite.Site.Name;
                var bestPriceEverDate = bestPriceEver == null ? string.Empty : string.Format("({0})", bestPriceEver.Occurance.ToString("MM/dd/yyyy hh:mm tt"));

                var buyPercentFormatted = string.Empty;

                if (bestPriceNow != null && bestPriceEver != null && bestPriceNow.Occurance != bestPriceEver.Occurance)
                {
                    var buyPercent = ((bestPriceNow.Price / bestPriceEver.Price) - 1) * 100;
                    buyPercentFormatted = buyPercent == 0 ? "BUY!!!" : string.Format("+{0:0.0}%", buyPercent);
                }

                Console.WriteLine(string.Format("{0,7} | {1," + -leftPadProducts + "}| {2,-7} @ {3," + -leftPadSites + "} | {4,-12} @ {5," + -leftPadSites + "} {6,-21}", buyPercentFormatted, productName, bestPriceNowPrice, bestPriceNowName, bestPriceEverPrice, bestPriceEverName, bestPriceEverDate));
            }

            do
            {
                valueRead = Console.ReadLine().Trim();
            }
            while (!valueRead.Equals("exit", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
