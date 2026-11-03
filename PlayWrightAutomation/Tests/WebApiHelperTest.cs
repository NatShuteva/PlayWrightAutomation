using Allure.NUnit;
using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.Text.RegularExpressions;

namespace PlayWrightAutomation.Tests
{
    [AllureNUnit]
    internal class WebApiHelperTest
    {
        private IPlaywright playwright = null!;
        private IBrowser browser = null!;
        private IBrowserContext context = null!;
        private IPage page = null!;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
            context = await browser.NewContextAsync();
            page = await context.NewPageAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await context.CloseAsync();
            await browser.CloseAsync();
            playwright.Dispose();
        }

        [Test]
        [Order(1)]
        public async Task LoginApi()
        {
            await page.GotoAsync("https://demoqa.com/login");

            var generateTokenResponseTask = page.WaitForResponseAsync(r =>
                r.Url.Contains("/Account/v1/GenerateToken") && r.Request.Method == "POST");

            await page.Locator("#userName").FillAsync("NatashaQA");
            await page.Locator("#password").FillAsync("Tester1234$");
            await page.Locator("#login").ClickAsync(new LocatorClickOptions { Force = true });

            var generateTokenResponse = await generateTokenResponseTask;
            Assert.That(generateTokenResponse.Status, Is.EqualTo(200));
            Assert.That(generateTokenResponse.Ok, Is.True);

            var json = await generateTokenResponse.JsonAsync();
            Assert.That(json.HasValue, Is.True);

            var root = json.Value;
            var token = root.GetProperty("token").GetString();
            var expires = root.GetProperty("expires").GetString();
            var status = root.GetProperty("status").GetString();
            var result = root.GetProperty("result").GetString();

            Assert.That(token, Is.Not.Null.And.Not.Empty);
            Assert.That(expires, Is.Not.Null.And.Not.Empty);
            Assert.That(status, Is.EqualTo("Success"));
            Assert.That(result, Is.EqualTo("User authorized successfully."));

            await page.WaitForURLAsync("**/profile", new PageWaitForURLOptions { Timeout = 30000 });
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/profile"));
            Console.WriteLine("Opened page URL: " + page.Url);

            var usernameLabel = page.Locator("#userName-value").First;
            await usernameLabel.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            Assert.That((await usernameLabel.InnerTextAsync()).Trim(), Is.EqualTo("NatashaQA"));

            var goToBookStoreButton = page.Locator("#gotoStore");
            await goToBookStoreButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            Assert.That(await goToBookStoreButton.IsEnabledAsync(), Is.True);
            Assert.That((await goToBookStoreButton.InnerTextAsync()).Trim(), Is.EqualTo("Go To Book Store"));

            await page.WaitForTimeoutAsync(3000);
        }

        [Test]
        [Order(2)]
        public async Task LoginFromUIAndVerifyProfilePage()
        {
            await page.GotoAsync("https://demoqa.com/profile");
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/profile"));
            Console.WriteLine("Opened page URL: " + page.Url);

            var userNameValue = page.Locator("#userName-value").First;
            await userNameValue.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            Assert.That((await userNameValue.InnerTextAsync()).Trim(), Is.EqualTo("NatashaQA"));

            await page.WaitForTimeoutAsync(10000);
        }
        
    }
}
