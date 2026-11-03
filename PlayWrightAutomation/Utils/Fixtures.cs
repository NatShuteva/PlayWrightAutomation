using Microsoft.Playwright;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace PlayWrightAutomation.Utils
{
    internal static class Fixtures
    {
        private const string LoginUrl = "https://demoqa.com/login";
        private const string UserName = "NatashaQA";
        private const string Password = "Tester1234$";

        public static async Task<(IPlaywright Playwright, IBrowser Browser, IBrowserContext Context, IPage Page)> CreateLoggedInSessionAsync()
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true
            });
            var page = await context.NewPageAsync();
            await page.GotoAsync(LoginUrl);
            await page.FillAsync("#userName", UserName);
            await page.FillAsync("#password", Password);
            await page.ClickAsync("#login", new PageClickOptions { Force = true });
            await page.WaitForURLAsync("**/profile", new PageWaitForURLOptions { Timeout = 30000 });
            await page.WaitForSelectorAsync("#userName-value");
            await page.WaitForTimeoutAsync(2000);

            return (playwright, browser, context, page);
        }

        public static async Task CloseSessionAsync(IPlaywright playwright, IBrowser browser, IBrowserContext context)
        {
            if (context != null)
                await context.CloseAsync();
            if (browser != null)
                await browser.CloseAsync();
            playwright?.Dispose();
        }

     

        public static async Task<string> AddLastBookToProfileAsync(IPage page)
        {
           
            await page.GotoAsync("https://demoqa.com/profile");
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            await page.Locator("#gotoStore").First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            await page.WaitForTimeoutAsync(2000);

            var goToStoreButton = page.Locator("#gotoStore").First;
            await goToStoreButton.ScrollIntoViewIfNeededAsync();
            await goToStoreButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
            await goToStoreButton.ClickAsync();
            await page.WaitForFunctionAsync(
                "() => window.location.pathname.includes('/books')",
                new PageWaitForFunctionOptions { Timeout = 30000 }
            );
            await page.Locator("#searchBox").First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            var bookLinks = page.Locator("a[href*='/books?search=']");
            await page.WaitForFunctionAsync("() => document.querySelectorAll(\"a[href*='/books?search=']\").length > 0", new PageWaitForFunctionOptions
            {
                Timeout = 15000
            });
            await page.WaitForTimeoutAsync(2000);

            var booksCount = await bookLinks.CountAsync();
            var lastBookLink = bookLinks.Nth(booksCount - 1);
            await lastBookLink.ScrollIntoViewIfNeededAsync();
            await lastBookLink.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
            var addedBookTitle = (await lastBookLink.InnerTextAsync()).Trim();
            Console.WriteLine($"Added book: {addedBookTitle}");
            await lastBookLink.ClickAsync();

            var addToCollectionButton = page.GetByRole(AriaRole.Button, new() { Name = "Add To Your Collection" }).First;
            await addToCollectionButton.ScrollIntoViewIfNeededAsync();
            await addToCollectionButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });
            await addToCollectionButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached, Timeout = 15000 });

            var dialogTcs = new TaskCompletionSource<IDialog>();
            void OnDialog(object? _, IDialog dialog)
            {
                dialogTcs.TrySetResult(dialog);
            }

            page.Dialog += OnDialog;
            await addToCollectionButton.ClickAsync();
            var completed = await Task.WhenAny(dialogTcs.Task, Task.Delay(7000));
            page.Dialog -= OnDialog;

            if (completed != dialogTcs.Task)
                throw new PlaywrightException("No popup appeared after clicking Add To Your Collection.");

            var dialog = await dialogTcs.Task;
            var message = dialog.Message;
            await dialog.AcceptAsync();

            if (message != "Book already present in the your collection!" && message != "Book added to your collection.")
                throw new PlaywrightException($"Unexpected popup text: {message}");


            await page.GotoAsync("https://demoqa.com/profile");
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await page.GetByRole(AriaRole.Link, new() { Name = addedBookTitle }).First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            await page.WaitForTimeoutAsync(2000);

            return addedBookTitle;
        }

        public static async Task<string?> DeleteLastBookFromProfileAsync(IPage page)
        {
            await page.GotoAsync("https://demoqa.com/profile");
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await page.Locator("#gotoStore").First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });

            var screenshotsDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Screenshots");
            System.IO.Directory.CreateDirectory(screenshotsDir);
            Console.WriteLine($"Delete method screenshot folder: {screenshotsDir}");

            var deleteIcons = page.Locator(".action-buttons span[title='Delete']");
            var rowsCountBeforeDelete = await deleteIcons.CountAsync();
            for (var i = 0; i < 10 && rowsCountBeforeDelete == 0; i++)
            {
                await page.WaitForTimeoutAsync(500);
                rowsCountBeforeDelete = await deleteIcons.CountAsync();
            }

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = System.IO.Path.Combine(screenshotsDir, "before-delete-book.png"),
                FullPage = true
            });

            if (rowsCountBeforeDelete == 0)
            {
                Console.WriteLine("Delete method executed, but no books found to delete.");
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = System.IO.Path.Combine(screenshotsDir, "no-book-to-delete.png"),
                    FullPage = true
                });
                return null;
            }

            var lastDeleteIcon = deleteIcons.Nth(rowsCountBeforeDelete - 1);
            await lastDeleteIcon.ScrollIntoViewIfNeededAsync();
            await lastDeleteIcon.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            var deletedBookTitle = (await lastDeleteIcon
                .Locator("xpath=ancestor::tr//a")
                .First
                .InnerTextAsync()).Trim();
            var deletedButtonId = await lastDeleteIcon.GetAttributeAsync("id");

            await page.WaitForTimeoutAsync(2000);
            await lastDeleteIcon.ClickAsync(new LocatorClickOptions { Force = true });

            var deletePopupOkButton = page.Locator("#closeSmallModal-ok:visible").First;
            await deletePopupOkButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            var dialogTcs = new TaskCompletionSource<IDialog>(TaskCreationOptions.RunContinuationsAsynchronously);
            void OnDialog(object? _, IDialog dialog)
            {
                dialogTcs.TrySetResult(dialog);
            }

            page.Dialog += OnDialog;
            try
            {
                await deletePopupOkButton.ClickAsync(new LocatorClickOptions { Force = true });

                var completed = await Task.WhenAny(dialogTcs.Task, Task.Delay(10000));
                if (completed != dialogTcs.Task)
                    throw new PlaywrightException("No dialog appeared after confirming delete.");

                var dialog = await dialogTcs.Task;
                if (!dialog.Message.Contains("Book deleted", StringComparison.OrdinalIgnoreCase))
                    throw new PlaywrightException($"Unexpected delete dialog text: {dialog.Message}");

                await dialog.AcceptAsync();
                await page.WaitForTimeoutAsync(2000);
            }
            finally
            {
                page.Dialog -= OnDialog;
            }

            await page.WaitForFunctionAsync(
                "count => document.querySelectorAll('.action-buttons span[title=\"Delete\"]').length < count",
                rowsCountBeforeDelete,
                new PageWaitForFunctionOptions { Timeout = 30000 }
            );

            var stillExists = !string.IsNullOrWhiteSpace(deletedButtonId)
                ? await page.EvaluateAsync<bool>("id => document.getElementById(id) !== null", deletedButtonId)
                : await deleteIcons.CountAsync() >= rowsCountBeforeDelete;

            if (stillExists)
                throw new PlaywrightException("Book still exists after delete.");

            Console.WriteLine($"Deleted book: {deletedBookTitle}");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = System.IO.Path.Combine(screenshotsDir, "after-delete-book.png"),
                FullPage = true
            });

            await page.WaitForTimeoutAsync(2000);
            return deletedBookTitle;
        }
    }
}
