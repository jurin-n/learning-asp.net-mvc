using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;

namespace WebApp.Tests.Controllers
{
    [TestClass]
    public class LoginControllerTest
    {
        [TestMethod]
        public void TestMethod()
        {
            var execPath = AppDomain.CurrentDomain.BaseDirectory;
            var appRoot = execPath.Substring(0, execPath.IndexOf("bin"));
            using (IWebDriver driver = new EdgeDriver(appRoot + "selenium_WebDrivers"))
            //using (IWebDriver driver = new ChromeDriver(appRoot + "selenium_WebDrivers"))
            {
                driver.Navigate().GoToUrl(@"https://selenium.dev");
            }
        }
    }
}
