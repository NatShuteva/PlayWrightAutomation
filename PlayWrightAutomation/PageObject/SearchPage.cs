using Microsoft.Playwright;
using NUnit.Framework;

namespace PlayWrightAutomation.PageObject
{
    internal class SearchPage
    {
        private readonly IPage page;
        private readonly ILocator searchBox;
        private readonly ILocator firstResult;
       

        public string Isbn { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string SubTitle { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string TotalPages { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public SearchPage(IPage page)
        {
            this.page = page;
            this.searchBox = page.Locator("#searchBox");
            this.firstResult = page.Locator("a[href*='/books?search=']").First;
        }

        public async Task OpenBookStorePageAsync()
        {
            await page.GotoAsync("https://demoqa.com/books");
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            Assert.That(page.Url, Does.Contain("/books"));
        }

        public async Task SearchByTitleAsync(string title)
        {
            await searchBox.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            await searchBox.FillAsync(title);
        }

        public async Task<string> GetFirstResultTitleAsync()
        {
            await firstResult.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            await page.WaitForTimeoutAsync(2000); // 2 seconds
            return (await firstResult.InnerTextAsync()).Trim();
        }

        public async Task ClickBookTitleAsync(string title)
        {
            await firstResult.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            await firstResult.ClickAsync();
        }

        public async Task<SearchPage> GetBookDetailsAsync()
        {
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            var details = new SearchPage(page)
            {
                Isbn = (await page.Locator("#ISBN-wrapper #userName-value").InnerTextAsync()).Trim(),
                Title = (await page.Locator("#title-wrapper #userName-value").InnerTextAsync()).Trim(),
                SubTitle = (await page.Locator("#subtitle-wrapper #userName-value").InnerTextAsync()).Trim(),
                Author = (await page.Locator("#author-wrapper #userName-value").InnerTextAsync()).Trim(),
                Publisher = (await page.Locator("#publisher-wrapper #userName-value").InnerTextAsync()).Trim(),
                TotalPages = (await page.Locator("#pages-wrapper #userName-value").InnerTextAsync()).Trim(),
                Description = (await page.Locator("#description-wrapper #userName-value").InnerTextAsync()).Trim()
            };

            Console.WriteLine($"ISBN: {details.Isbn}");
            Console.WriteLine($"Title: {details.Title}");
            Console.WriteLine($"SubTitle: {details.SubTitle}");
            Console.WriteLine($"Author: {details.Author}");
            Console.WriteLine($"Publisher: {details.Publisher}");
            Console.WriteLine($"TotalPages: {details.TotalPages}");
            Console.WriteLine($"Description: {details.Description}");

            return details;
        }

        public async Task ClickBackToBookStoreAsync()
        {
            var backToBookStoreButton = page.Locator("#addNewRecordButton");
            await backToBookStoreButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            await backToBookStoreButton.ScrollIntoViewIfNeededAsync();
            await backToBookStoreButton.ClickAsync();

        }

        public async Task WaitForBookStorePageAsync()
        {
            await page.WaitForURLAsync("**/books", new PageWaitForURLOptions
            {
                Timeout = 15000
            });
        }

        public string GetCurrentUrl()
        {
            return page.Url;
        }
    }
}
