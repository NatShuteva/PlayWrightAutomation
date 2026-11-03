using Microsoft.Playwright;

namespace PlayWrightAutomation.Tests
{
    public class UnitTest1
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }

        [Test]
        public async Task OpenSauceDemoPage()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
                SlowMo = 500
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://www.saucedemo.com/");

            Assert.That(page.Url, Does.StartWith("https://www.saucedemo.com"));
        }
    }
}
