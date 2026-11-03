using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayWrightAutomation.PageObject
{
    internal class POManager
    {
        private readonly IPage page;
        private readonly LoginPage loginPage;
        private readonly ProfilePage profilePage;
        private readonly BookStorePage bookStorePage;

        //constructor         
        public POManager(IPage page)
        {
            this.page = page;
            this.loginPage = new LoginPage(page);
            this.profilePage = new ProfilePage(page);
            this.bookStorePage = new BookStorePage(page);
        }

        public LoginPage getLoginPage()
        {            
            return this.loginPage;
        }

        public ProfilePage getProfilePage()
        {
            return this.profilePage;
        }

        public BookStorePage getBookStorePage()
        {
            return this.bookStorePage;
        }
       
    }

        
}
