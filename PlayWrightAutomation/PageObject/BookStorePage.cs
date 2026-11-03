using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayWrightAutomation.PageObject
{
    internal class BookStorePage
    {
        private readonly IPage page;
        private readonly ILocator search;

    //constructor 
        public BookStorePage(IPage page)
        {
            this.page = page;
            this.search = page.Locator("#searchBox");
        }

        //main custom methods for BookStorePage
        public async Task SearchAsync(string query)
        {
            await this.search.FillAsync(query);
            await this.search.PressAsync("Enter");

            await this.page.WaitForTimeoutAsync(1000);

            var screenshotsDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Screenshots");
            System.IO.Directory.CreateDirectory(screenshotsDir);
            await this.page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = System.IO.Path.Combine(screenshotsDir, "bookstore-search-result.png"),
                FullPage = true
            });
        }
    }
}
