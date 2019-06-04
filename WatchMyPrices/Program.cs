namespace WatchMyPrices
{
    using System;
    using System.Net;
    using System.Net.Mail;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Support.UI;
    using WatchMyPrices.DB;
    using WatchMyPrices.Mode;

    public class Program
    {
        public static void Main(string[] args)
        {
            var passedArguments = new Arguments(args);
            IDatabase database = new SQLite(@"C:\git\personal\WatchMyPrices\myQuickDb.sqlite");
            IWebQueryable webQueryable = new WebQueryable();
            IPriceQueryable webPriceQueryable = new WebPriceQueryable(webQueryable, database);
            IInteractionMode mode = null;

            if (passedArguments.IsWatchMode)
            {
                mode = new WatchMode(webPriceQueryable, database, passedArguments.TimeInterval);
            }
            else
            {
                mode = new InteractiveMode(database);
            }

            mode.Run();
        }
    }
}
