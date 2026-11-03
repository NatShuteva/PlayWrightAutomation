using Microsoft.Playwright;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PlayWrightAutomation.Tests
{
    public class LoginTests
    {
        [Test]
        public async Task OpenSauceDemoLoginPage()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://www.saucedemo.com/");

            Assert.That(page.Url, Does.StartWith("https://www.saucedemo.com"));
            Assert.That(page.TitleAsync().Result, Is.EqualTo("Swag Labs"));
            Console.WriteLine("Page title is: " + page.TitleAsync().Result);
        }

        [Test]
        public async Task InvalidLogin()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://www.saucedemo.com/");

            Assert.That(page.Url, Does.StartWith("https://www.saucedemo.com"));
            Assert.That(page.TitleAsync().Result, Is.EqualTo("Swag Labs"));
            Console.WriteLine("Page title is: " + page.TitleAsync().Result);

            await page.Locator("#user-name").FillAsync("standard_use");
            await page.Locator("[type='password']").FillAsync("secret_sauce");
            await page.Locator("#login-button").ClickAsync();
            var errorLogin = page.Locator("h3:has-text('Epic sadface')");
            Assert.That(await errorLogin.IsVisibleAsync(), Is.True);
            Console.WriteLine("Error message for unsuccessful login: " + await errorLogin.TextContentAsync());
        }

        [Test]
        public async Task SuccessfulLogin()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://www.saucedemo.com/");

            Assert.That(page.Url, Does.StartWith("https://www.saucedemo.com"));
            Assert.That(page.TitleAsync().Result, Is.EqualTo("Swag Labs"));
            Console.WriteLine("Page title is: " + page.TitleAsync().Result);

            await page.Locator("#user-name").FillAsync("standard_user");
            await page.Locator("[type='password']").FillAsync("secret_sauce");
            await page.Locator("#login-button").ClickAsync();
            Assert.That(page.Url, Does.StartWith("https://www.saucedemo.com/inventory.html"));
            Console.WriteLine("Successfully logged in. Current URL: " + page.Url);
            var productsTitle = page.Locator(".title");
            Assert.That(await productsTitle.InnerTextAsync(), Is.EqualTo("Products"));
            Console.WriteLine("Products page title is: " + await productsTitle.InnerTextAsync());

        }

    }
}
