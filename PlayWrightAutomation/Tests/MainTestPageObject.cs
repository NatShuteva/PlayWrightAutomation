using Microsoft.Playwright;
using NUnit.Framework;
using PlayWrightAutomation.PageObject;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PlayWrightAutomation.Tests
{
    internal class MainTestPageObject
    {
        [Test]
        public async Task PageObjectLogin()
        {
            var userName = "NatashaQA";
            var password = "Tester1234$";
            var someBook = "Git Pocket Guide";
            // Create Playwright
            var playwright = await Playwright.CreateAsync(); //Initialize Playwright

            // Launch a browser
            var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = false
                });

            // Create context
            var context = await browser.NewContextAsync(); //Create a new isolated browser session.

            // Create page
            var page = await context.NewPageAsync(); //This creates a Playwright browser page(tab).

            // Create object instance
            var loginPage = new LoginPage(page);
            await loginPage.GoToAction();
            await loginPage.validLogin(userName, password);
            await page.WaitForURLAsync("**/profile", new PageWaitForURLOptions { Timeout = 30000 });
            Assert.That(page.Url, Does.Contain("/profile"));

            var profilePage = new ProfilePage(page);
            var firstBookTitle = await profilePage.GetBookTitleAsync();
            await profilePage.GetAllBookTitlesAsync();
            await profilePage.SearchAsync(someBook);
            Assert.That(firstBookTitle, Does.Contain(someBook));
            await profilePage.GoToBookStoreAsync();

            var bookStorePage = new BookStorePage(page);
            await bookStorePage.SearchAsync(someBook);

            await browser.CloseAsync();
        }
    }
}
