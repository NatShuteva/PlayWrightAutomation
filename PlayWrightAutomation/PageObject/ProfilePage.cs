using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayWrightAutomation.PageObject
{
    internal class ProfilePage
    {
        private readonly IPage page; 
        private readonly ILocator search;
        private readonly ILocator bookTitle;
        private readonly ILocator bookTitles;
        private readonly ILocator goToBookStore;

        //constructor 
        public ProfilePage(IPage page)  //Ipage page is constructor parameter
        {
            this.page = page;
            this.bookTitles = page.Locator("a[href*='/books?search=']");
            this.bookTitle = this.bookTitles.First;
            this.search = page.Locator("#searchBox");
            this.goToBookStore = page.Locator("button#gotoStore");
        }

        //main custom methods for ProfilePage
        public async Task<string> GetBookTitleAsync()
        {
            await this.search.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });


            Console.WriteLine($"Profile URL: {this.page.Url}");
            var count = await this.bookTitles.CountAsync();


            var hasBooks = count > 0;
            if (!hasBooks)
            {
                for (var i = 0; i < 30 && !hasBooks; i++)
                {
                    await this.page.WaitForTimeoutAsync(500);
                    count = await this.bookTitles.CountAsync();
                    hasBooks = count > 0;
                }

                Console.WriteLine($"Book rows count (after wait): {count}");

                if (!hasBooks)
                {
                    Console.WriteLine("Book title from profile page: <none>");
                    return string.Empty;
                }
            }

            var title = (await this.bookTitle.InnerTextAsync()).Trim();
            Console.WriteLine($"Book title from profile page: {title}");
            return title;
        }



        public async Task<List<string>> GetAllBookTitlesAsync()
        {
            var titles = new List<string>();
            await this.search.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });

            var count = await this.bookTitles.CountAsync();
            for (var i = 0; i < 10 && count == 0; i++)
            {
                await this.page.WaitForTimeoutAsync(500);
                count = await this.bookTitles.CountAsync();
            }

            for (int i = 0; i < count; i++)
            {
                var title = (await this.bookTitles.Nth(i).InnerTextAsync()).Trim();
                if (!string.IsNullOrWhiteSpace(title))
                    titles.Add(title);
            }

            Console.WriteLine($"Getting all book titles from the profile page: {string.Join(", ", titles)}");
            return titles;
        }

        public async Task SearchAsync(string query)
        {
            await this.search.FillAsync(query);
            await this.search.PressAsync("Enter");

            await this.page.WaitForTimeoutAsync(1000);

            var screenshotsDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Screenshots");
            System.IO.Directory.CreateDirectory(screenshotsDir);
            await this.page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = System.IO.Path.Combine(screenshotsDir, "profile-search-result.png"),
                FullPage = true
            });
        }

        public async Task GoToBookStoreAsync()
        {
            await this.goToBookStore.ClickAsync();
            await page.WaitForTimeoutAsync(2000);
        }

        internal async Task ClickLogoutButtonAsync()
        {
            await this.page.ClickAsync("button#submit");
            await this.page.WaitForTimeoutAsync(2000);
        }
    }
}
