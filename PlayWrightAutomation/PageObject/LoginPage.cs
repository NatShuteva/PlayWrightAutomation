using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using UglyToad.PdfPig.Actions;

namespace PlayWrightAutomation.PageObject
{
    internal class LoginPage
    {
        private readonly IPage page;
        private readonly ILocator signInButton;
        private readonly ILocator userName;
        private readonly ILocator password;
        private readonly ILocator invalidLoginMessage;

        //constructor 
        public LoginPage(IPage page)
        {
            this.page = page;
            this.userName = page.Locator("#userName");
            this.password = page.Locator("#password");
            this.signInButton = page.Locator("#login");
            this.invalidLoginMessage = page.Locator("#name");
        }


        //main custom methods for LoginPage 
        public async Task GoToAction()
        {
            await page.GotoAsync("https://demoqa.com/login");
        }
        public async Task validLogin(string userName, string password)
        {
            await this.userName.FillAsync(userName);
            await this.password.FillAsync(password);
            await page.WaitForTimeoutAsync(2000);
            await signInButton.ClickAsync();
            await page.WaitForTimeoutAsync(2000);
        }


        public async Task invalidLogin(string userName, string password)
        {
            await this.userName.FillAsync(userName);
            await this.password.FillAsync(password);
            await signInButton.ClickAsync();
        }

        public async Task<string> GetInvalidLoginMessageAsync()
        {
            await invalidLoginMessage.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });

            return (await invalidLoginMessage.InnerTextAsync()).Trim();
        }

    }
}
