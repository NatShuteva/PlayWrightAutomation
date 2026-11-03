using Microsoft.Playwright;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using System.Threading.Tasks;
using NUnit.Framework.Legacy;

namespace PlayWrightAutomation.Tests
{
    internal class WebUiAPI
    {
        private IPlaywright playwright = null!;
        private IBrowser browser = null!;
        private IBrowserContext context = null!;
        private IPage page = null!;

        private static readonly object LoginPayload = new
        {
            userName = "NatashaQA",
            password = "Tester1234$"
        };

        private string token;

        [SetUp]
        public async Task SetUp()
        {
            Console.WriteLine("Running before each test- LoginAPI");

            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
            context = await browser.NewContextAsync();
            page = await context.NewPageAsync();

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
        }

        [TearDown]
        public async Task TearDown()
        {
            await browser.CloseAsync();
            playwright.Dispose();
        }

        [Test]
        public async Task LoginWithAPI()
        {
            Console.WriteLine("Test 1");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/profile"));
            Console.WriteLine("Opened page URL: " + page.Url);

            var userNameValue = page.Locator("#userName-value").First;
            await userNameValue.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            Assert.That((await userNameValue.InnerTextAsync()).Trim(), Is.EqualTo("NatashaQA"));

            await page.WaitForTimeoutAsync(3000);
        }

        [Test]
        public async Task LoginWithAPI2()
        {
            Console.WriteLine("Test 2");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/profile"));
            Console.WriteLine("Opened page URL: " + page.Url);

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
        public async Task LoginWithAPI3()
        {
            Console.WriteLine("Test 3");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/profile"));
            Console.WriteLine("Opened page URL: " + page.Url);

            var goToBookStoreButton = page.Locator("#gotoStore");
            await goToBookStoreButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            Assert.That(await goToBookStoreButton.IsEnabledAsync(), Is.True);
            Assert.That((await goToBookStoreButton.InnerTextAsync()).Trim(), Is.EqualTo("Go To Book Store"));
            await goToBookStoreButton.ClickAsync();

            await page.WaitForURLAsync("**/books", new PageWaitForURLOptions { Timeout = 30000 });
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/books"));

            var expectedHeaders = new[] { "Image", "Title", "Author", "Publisher" };

            foreach (var header in expectedHeaders)
            {
                var headerLocator = page.GetByRole(AriaRole.Columnheader, new() { Name = header }).First;
                await headerLocator.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 1000
                  
                });

                Assert.That((await headerLocator.InnerTextAsync()).Trim(), Is.EqualTo(header));
            }

            await page.WaitForTimeoutAsync(3000);
        }
    }
}