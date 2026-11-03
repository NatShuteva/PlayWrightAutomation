using Microsoft.Playwright;
using PlayWrightAutomation.PageObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayWrightAutomation.Tests
{
    internal class MainTestPOManager
    {

        [Test]
        public async Task PageObjectLogin()
        {
            {
                 
                var userName = "NatashaQA";
                var password = "Tester1234$";
                var someBook = "Git Pocket Guide";
                // Create Playwright
                var playwright = await Playwright.CreateAsync();

                // Launch browser
                var browser = await playwright.Chromium.LaunchAsync(
                    new BrowserTypeLaunchOptions
                    {
                        Headless = false
                    });

                // Create context
                var context = await browser.NewContextAsync();

                // Create page
                var page = await context.NewPageAsync();

                // Create object instance
                var poManager = new PageObject.POManager(page);
                var loginPage = poManager.getLoginPage();
                await loginPage.GoToAction();
                await loginPage.validLogin(userName, password);


                var profilePage = poManager.getProfilePage();
                await profilePage.GetBookTitleAsync();
                await profilePage.GetAllBookTitlesAsync();
                await profilePage.SearchAsync(someBook);
                await profilePage.GoToBookStoreAsync();

                var bookStorePage = poManager.getBookStorePage();
                await bookStorePage.SearchAsync(someBook);


                await browser.CloseAsync();

            }
        }
    }
}
