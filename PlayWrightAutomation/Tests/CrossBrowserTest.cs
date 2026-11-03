using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace PlayWrightAutomation.Tests
{
    public class CrossBrowserTest
    {
        [TestCase("chromium", TestName = "SuccessfulLogin_chromium")]
        [TestCase("firefox", TestName = "SuccessfulLogin_firefox")]
        [TestCase("webkit", TestName = "SuccessfulLogin_webkit")]
        [TestCase("iphone11", TestName = "SuccessfulLogin_iphone11")]
        public async Task SuccessfulLogin(string browserName)
        {
            using var playwright = await Playwright.CreateAsync();

            IBrowser browser = browserName switch
            {
                "chromium" => await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false,
                    Channel = "chrome"
                }),

                "firefox" => await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                }),

                "webkit" => await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                }),

                "iphone11" => await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = false
                }),

                _ => throw new Exception($"Browser '{browserName}' not supported.")
            };

            IBrowserContext context;
            if (browserName == "iphone11")
            {
                context = await browser.NewContextAsync(playwright.Devices["iPhone 11"]);
            }
            else
            {
                context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    ViewportSize = browserName switch
                    {
                        "firefox" => new ViewportSize { Width = 720, Height = 720 },
                        "chromium" => new ViewportSize { Width = 1280, Height = 720 },
                        "webkit" => new ViewportSize { Width = 1280, Height = 720 },
                        _ => new ViewportSize { Width = 1280, Height = 720 }
                    }
                });
            }

            var page = await context.NewPageAsync();

            Console.WriteLine($"Running on: {browserName}");

            await page.GotoAsync("https://www.saucedemo.com/");

            Assert.That(page.Url, Does.StartWith("https://www.saucedemo.com"));

            var title = await page.TitleAsync();
            Assert.That(title, Is.EqualTo("Swag Labs"));
            Console.WriteLine($"Page title is: {title}");

            await page.Locator("#user-name").FillAsync("standard_user");
            await page.Locator("#password").FillAsync("secret_sauce");
            await page.Locator("#login-button").ClickAsync();

            await page.WaitForURLAsync("**/inventory.html", new PageWaitForURLOptions { Timeout = 30000 });
            Assert.That(page.Url, Does.StartWith("https://www.saucedemo.com/inventory.html"));

            var productsTitle = page.Locator(".title");
            Assert.That(await productsTitle.InnerTextAsync(), Is.EqualTo("Products"));

            await context.CloseAsync();

            await browser.CloseAsync();
        }
    }
}
