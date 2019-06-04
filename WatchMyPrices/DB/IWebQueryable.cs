namespace WatchMyPrices.DB
{
    using OpenQA.Selenium;

    public interface IWebQueryable
    {
        IWebDriver GetWebDriver();

        IWebElement GetWebElement(IWebDriver driver, string xPath);
    }
}
