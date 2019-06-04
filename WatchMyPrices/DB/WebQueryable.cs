namespace WatchMyPrices.DB
{
    using System;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    public class WebQueryable : IWebQueryable
    {
        public IWebDriver GetWebDriver()
        {
            var options = new ChromeOptions()
            {
                BinaryLocation = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            };

            options.AddArguments("--headless", "--disable-gpu", "--no-sandbox");

            var driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            return driver;
        }

        public IWebElement GetWebElement(IWebDriver driver, string xPath)
        {
            return driver.FindElement(By.XPath(xPath));
        }
    }
}
