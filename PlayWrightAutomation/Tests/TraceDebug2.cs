using Allure.NUnit;
using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PlayWrightAutomation.Tests
{
    internal class TraceDebug2
    {
        [Test]
        public async Task LoginTraceDebug()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });

            var traceDir = "traces";
            var videoDir = "videos";
            Directory.CreateDirectory(traceDir);
            Directory.CreateDirectory(videoDir);

            await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                RecordVideoDir = videoDir,
                RecordVideoSize = new RecordVideoSize
                {
                    Width = 1280,
                    Height = 720
                }
            });
            await context.Tracing.StartAsync(new()
            {
                Title = "LoginTraceDebug",
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });

            var page = await context.NewPageAsync();
            var traceFile = Path.Combine(traceDir, $"LoginTraceDebug-{DateTime.Now:yyyyMMdd-HHmmss}.zip");

            try
            {
                await page.GotoAsync("https://demoqa.com/login");
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

                var userNameInput = page.Locator("#userName").First;
                var passwordInput = page.Locator("#password").First;
                var loginButton = page.Locator("#login").First;

                await userNameInput.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });
                await passwordInput.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });
                await loginButton.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });

                var generateTokenResponseTask = page.WaitForResponseAsync(r =>
                    r.Request.Method == "POST"
                    && r.Url.Contains("/Account/v1/")
                    && (r.Url.Contains("GenerateToken") || r.Url.Contains("Login")));

                await userNameInput.FillAsync("NatashaQA");
                await passwordInput.FillAsync("Tester1234$");
                await loginButton.ClickAsync();

                var generateTokenResponse = await generateTokenResponseTask;
                Assert.That(generateTokenResponse.Status, Is.LessThan(400));

                await page.WaitForURLAsync("**/profile", new PageWaitForURLOptions { Timeout = 30000 });
                Assert.That(page.Url, Does.StartWith("https://demoqa.com/profile"));

                var userNameValue = page.Locator("#userName-value").First;
                await userNameValue.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });
                Assert.That((await userNameValue.InnerTextAsync()).Trim(), Is.EqualTo("NatashaQA"));

                var goToBookStoreButton = page.Locator("#gotoStore").First;
                await goToBookStoreButton.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });
                await goToBookStoreButton.ScrollIntoViewIfNeededAsync();
                await goToBookStoreButton.ClickAsync();
                await page.WaitForURLAsync("**/books", new PageWaitForURLOptions { Timeout = 30000 });
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await page.Locator("#searchBox").First.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });

                var expectedHeaders = new[] { "Image", "Title", "Author", "Publisher" };
                await page.WaitForFunctionAsync(
                    "() => document.querySelectorAll('.rt-thead .rt-th, thead th').length >= 4",
                    new PageWaitForFunctionOptions { Timeout = 30000 }
                );

                var headersText = await page.EvaluateAsync<string[]>(
                    "() => Array.from(document.querySelectorAll('.rt-thead .rt-th, thead th')).map(h => (h.textContent || '').trim()).filter(Boolean)"
                );

                foreach (var header in expectedHeaders)
                    Assert.That(headersText, Has.Member(header));
            }
            finally
            {
                await context.Tracing.StopAsync(new()
                {
                    Path = traceFile
                });
                Console.WriteLine($"Trace saved: {Path.GetFullPath(traceFile)}");
            }
        }
    }
}