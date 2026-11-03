using Microsoft.Playwright;
using NUnit.Framework;
using PlayWrightAutomation.Utils;
using System;
using System.Threading.Tasks;

namespace PlayWrightAutomation.Tests
{
    internal class FixturesDemoTest2
    {
        [Test]
        public async Task LoginAndAddBook_UsingFixturesTest2Data()
        {
            var data = new FixturesTest2.TestDataFixture();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });

            await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true
            });

            var page = await context.NewPageAsync();

            await page.GotoAsync("https://demoqa.com/login");
            await page.FillAsync("#userName", data.UserName);
            await page.FillAsync("#password", data.Password);
            await page.ClickAsync("#login", new PageClickOptions { Force = true });

            await page.WaitForURLAsync("**/profile", new PageWaitForURLOptions { Timeout = 30000 });

            var userNameValue = page.Locator("#userName-value");
            await userNameValue.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            Assert.That((await userNameValue.InnerTextAsync()).Trim(), Is.EqualTo(data.UserName));

            await page.ClickAsync("#gotoStore", new PageClickOptions { Force = true });
            await page.WaitForURLAsync("**/books", new PageWaitForURLOptions { Timeout = 30000 });

            var searchBox = page.Locator("#searchBox");
            await searchBox.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            await searchBox.FillAsync(data.BookTitle);

            var bookLink = page.GetByRole(AriaRole.Link, new() { Name = data.BookTitle }).First;
            await bookLink.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            await bookLink.ScrollIntoViewIfNeededAsync();
            await bookLink.ClickAsync();

            var dialogTask = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            page.Dialog += async (_, dialog) =>
            {
                dialogTask.TrySetResult(dialog.Message);
                await dialog.AcceptAsync();
            };

            var addToCollection = page.GetByRole(AriaRole.Button, new() { Name = "Add To Your Collection" }).First;
            await addToCollection.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            await addToCollection.ClickAsync(new LocatorClickOptions { Force = true });

            var completed = await Task.WhenAny(dialogTask.Task, Task.Delay(10000));
            Assert.That(completed, Is.EqualTo(dialogTask.Task), "Add-to-collection dialog did not appear.");

            var dialogMessage = await dialogTask.Task;
            Assert.That(
                dialogMessage,
                Is.EqualTo("Book added to your collection.").Or.EqualTo("Book already present in the your collection!"));

            await page.GotoAsync("https://demoqa.com/profile");
            var addedBookTitle = page.GetByRole(AriaRole.Link, new() { Name = data.BookTitle }).First;
            await addedBookTitle.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            Assert.That(await addedBookTitle.IsVisibleAsync(), Is.True);
        }
    }
}
