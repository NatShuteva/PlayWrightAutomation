using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayWrightAutomation.Tests
{
    internal class AddBookViaAPI_Call
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

        private static readonly string[] AddBookIsbns =
        {
            "9781449325862",
            "9781449331818"
        };

        private static object BuildAddBookPayload(string userId, IEnumerable<string> isbns)
        {
            return new
            {
                userId,
                collectionOfIsbns = isbns.Select(isbn => new { isbn }).ToArray()
            };
        }

        private string token = string.Empty;
        private string userId = string.Empty;
        private string deletedBookTitle = string.Empty;
        private string deletedBookIsbn = string.Empty;

        [SetUp]
        public async Task SetUp()
        {
            Console.WriteLine("Running before each test- LoginAPI");

            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
            context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true
            });
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
            token = root.GetProperty("token").GetString() ?? string.Empty;
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

            var loginApiResponse = await context.APIRequest.PostAsync("https://demoqa.com/Account/v1/Login", new APIRequestContextOptions
            {
                DataObject = LoginPayload
            });
            Assert.That(loginApiResponse.Status, Is.EqualTo(200));

            var loginApiJson = await loginApiResponse.JsonAsync();
            Assert.That(loginApiJson.HasValue, Is.True);
            token = loginApiJson.Value.GetProperty("token").GetString() ?? token;
            userId = loginApiJson.Value.GetProperty("userId").GetString() ?? string.Empty;
            Assert.That(userId, Is.Not.Empty);

            var deleteAllResponse = await context.APIRequest.DeleteAsync($"https://demoqa.com/BookStore/v1/Books?UserId={userId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {token}" }
                }
            });
            Assert.That(deleteAllResponse.Status, Is.EqualTo(204).Or.EqualTo(200));

            var userAfterDeleteResponse = await context.APIRequest.GetAsync($"https://demoqa.com/Account/v1/User/{userId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {token}" }
                }
            });
            Assert.That(userAfterDeleteResponse.Status, Is.EqualTo(200));
            var userAfterDeleteJson = await userAfterDeleteResponse.JsonAsync();
            Assert.That(userAfterDeleteJson.HasValue, Is.True);
            var booksAfterDelete = userAfterDeleteJson.Value.GetProperty("books");
            Assert.That(booksAfterDelete.GetArrayLength(), Is.EqualTo(0));

            var booksListResponse = await context.APIRequest.GetAsync("https://demoqa.com/BookStore/v1/Books");
            Assert.That(booksListResponse.Status, Is.EqualTo(200));
            var booksListJson = await booksListResponse.JsonAsync();
            Assert.That(booksListJson.HasValue, Is.True);

            var selectedBooks = booksListJson.Value.GetProperty("books")
                .EnumerateArray()
                .Where(b => AddBookIsbns.Contains(b.GetProperty("isbn").GetString() ?? string.Empty))
                .Select(b => new
                {
                    isbn = b.GetProperty("isbn").GetString() ?? string.Empty,
                    title = b.GetProperty("title").GetString() ?? string.Empty
                })
                .ToList();

            Assert.That(selectedBooks.Count, Is.EqualTo(2), "Configured AddBookPayload ISBNs were not found in catalog.");
            Assert.That(selectedBooks.All(b => !string.IsNullOrWhiteSpace(b.isbn)), Is.True);


            deletedBookTitle = selectedBooks[0].title;
            deletedBookIsbn = selectedBooks[0].isbn;

            var addBookPayload = BuildAddBookPayload(userId, AddBookIsbns);

            var addBookResponse = await context.APIRequest.PostAsync("https://demoqa.com/BookStore/v1/Books", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {token}" },
                    { "Content-Type", "application/json" }
                },
                DataObject = addBookPayload
            });

            Assert.That(addBookResponse.Status, Is.EqualTo(201));

            var userAfterAddResponse = await context.APIRequest.GetAsync($"https://demoqa.com/Account/v1/User/{userId}", new APIRequestContextOptions
            {
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {token}" }
                }
            });
            Assert.That(userAfterAddResponse.Status, Is.EqualTo(200));
            var userAfterAddJson = await userAfterAddResponse.JsonAsync();
            Assert.That(userAfterAddJson.HasValue, Is.True);
            var booksAfterAdd = userAfterAddJson.Value.GetProperty("books");
            Assert.That(booksAfterAdd.GetArrayLength(), Is.EqualTo(2));

            await page.GotoAsync("https://demoqa.com/profile");
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/profile"));

            foreach (var book in selectedBooks)
            {
                var titleLocator = page.GetByRole(AriaRole.Link, new() { Name = book.title }).First;
                await titleLocator.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 15000
                });
                Assert.That((await titleLocator.InnerTextAsync()).Trim(), Is.EqualTo(book.title));
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            await browser.CloseAsync();
            playwright.Dispose();
        }

        [Test]
        public async Task DeleteOneBook()
        {
            await page.GotoAsync("https://demoqa.com/profile");
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var userNameValue = page.Locator("#userName-value").First;
            await userNameValue.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            Assert.That((await userNameValue.InnerTextAsync()).Trim(), Is.EqualTo("NatashaQA"));

            var deletedTitleLocator = page.GetByRole(AriaRole.Link, new() { Name = deletedBookTitle });

            var firstDeleteIcon = page.Locator($"#delete-record-{deletedBookIsbn}");
            await firstDeleteIcon.ClickAsync();

            var modalOkButton = page.Locator("#closeSmallModal-ok");
            await modalOkButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            var alertTask = new TaskCompletionSource<string>();
            page.Dialog += async (_, dialog) =>
            {
                alertTask.TrySetResult(dialog.Message);
                await dialog.AcceptAsync();
            };

            await modalOkButton.ClickAsync();

            var completed = await Task.WhenAny(alertTask.Task, Task.Delay(5000));
            Assert.That(completed, Is.EqualTo(alertTask.Task), "Delete alert did not appear in 5 seconds.");

            var alertMessage = await alertTask.Task;
            Assert.That(alertMessage, Is.EqualTo("Book deleted."));
            await deletedTitleLocator.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Detached,
                Timeout = 15000
            });

            Assert.That(await deletedTitleLocator.CountAsync(), Is.EqualTo(0));

            await page.ReloadAsync();
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await page.WaitForFunctionAsync("""
                () => document.querySelectorAll("a[href^='/books?search=']").length === 1
                """);

            var booksInGrid = page.Locator("a[href^='/books?search=']");
            Assert.That(await booksInGrid.CountAsync(), Is.EqualTo(1));
            await page.WaitForTimeoutAsync(3000);
        }
    }
}
