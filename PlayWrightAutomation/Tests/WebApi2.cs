using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PlayWrightAutomation.Tests
{
    [NonParallelizable]
    internal class WebApi2
    {
        private const string UserName = "NatashaQA";
        private const string Password = "Tester1234$";
        private const string TargetIsbn = "9781449325862";

        [Test]
        public async Task Login_InterceptZeroBooks_Wait2Sec_AddOneBook()
        {
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

            await page.RouteAsync("**/Account/v1/User/*", async route =>
            {
                var originalResponse = await route.FetchAsync();
                var body = await originalResponse.TextAsync();
                var node = JsonNode.Parse(body) as JsonObject;

                if (node != null)
                {
                    node["books"] = new JsonArray();

                    await route.FulfillAsync(new RouteFulfillOptions
                    {
                        Status = originalResponse.Status,
                        ContentType = "application/json; charset=utf-8",
                        Body = node.ToJsonString()
                    });
                    return;
                }

                await route.ContinueAsync();
            });

            await page.GotoAsync("https://demoqa.com/login");
            await page.FillAsync("#userName", UserName);
            await page.FillAsync("#password", Password);
            await page.ClickAsync("#login", new PageClickOptions { Force = true });

            await page.WaitForURLAsync("**/profile", new PageWaitForURLOptions { Timeout = 30000 });
            await page.WaitForSelectorAsync("#userName-value");

            var resultText = await page.InnerTextAsync("#userName-value");
            Assert.That(resultText.Trim(), Is.EqualTo(UserName));

            var booksRows = page.Locator("[id^='delete-record-']");
            Assert.That(await booksRows.CountAsync(), Is.EqualTo(0));
            await page.WaitForTimeoutAsync(2000);

            await using var apiContext = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
            {
                IgnoreHTTPSErrors = true
            });

            var loginResponse = await apiContext.PostAsync("https://demoqa.com/Account/v1/Login", new APIRequestContextOptions
            {
                DataObject = new { userName = UserName, password = Password }
            });
            Assert.That(loginResponse.Status, Is.EqualTo(200));

            var loginJson = await loginResponse.JsonAsync();
            Assert.That(loginJson.HasValue, Is.True);
            var token = loginJson.Value.GetProperty("token").GetString() ?? string.Empty;
            var userId = loginJson.Value.GetProperty("userId").GetString() ?? string.Empty;
            Assert.That(token, Is.Not.Empty);
            Assert.That(userId, Is.Not.Empty);

            var deleteResponse = await apiContext.DeleteAsync(
                $"https://demoqa.com/BookStore/v1/Books?UserId={userId}",
                new APIRequestContextOptions
                {
                    Headers = new Dictionary<string, string>
                    {
                        { "Authorization", $"Bearer {token}" }
                    }
                });
            Assert.That(deleteResponse.Status, Is.EqualTo(204).Or.EqualTo(200));

            var addResponse = await apiContext.PostAsync("https://demoqa.com/BookStore/v1/Books", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {token}" },
                    { "Content-Type", "application/json" }
                },
                DataObject = new
                {
                    userId,
                    collectionOfIsbns = new[] { new { isbn = TargetIsbn } }
                }
            });
            Assert.That(addResponse.Status, Is.EqualTo(201));

            await page.UnrouteAsync("**/Account/v1/User/*");
            await page.ReloadAsync();
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var bookRow = page.Locator($"#delete-record-{TargetIsbn}");
            await bookRow.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
            Assert.That(await bookRow.CountAsync(), Is.EqualTo(1));
            await page.WaitForTimeoutAsync(2000);
        }

    }
}
