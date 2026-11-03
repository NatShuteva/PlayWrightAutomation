using Microsoft.Playwright;
using NUnit.Framework;
using PlayWrightAutomation.Features.Support;
using PlayWrightAutomation.PageObject;
using Reqnroll;
using System.Threading.Tasks;

namespace PlayWrightAutomation.Features.StepDefinitions
{
    [Binding]
    public class LoginStepDefinition
    {
        private readonly IPage page;
        private LoginPage loginPage = null!;
        private ProfilePage profilePage = null!;

        public LoginStepDefinition(PlaywrightDriver driver)
        {
            page = driver.Page;
        }

        [Given("the login page is open")]
        public async Task GivenTheLoginPageIsOpen()
        {
            loginPage = new LoginPage(page);
            await loginPage.GoToAction();
        }

        [When(@"valid credentials are entered for ""(.*)"" and ""(.*)""")]
        public async Task WhenValidCredentialsAreEnteredForAnd(string username, string password)
        {
            await loginPage.validLogin(username, password);
        }

        [Then("the profile page is displayed")]
        public async Task ThenTheProfilePageIsDisplayed()
        {
            profilePage = new ProfilePage(page);
            await profilePage.GetBookTitleAsync();

        }

        [Then("the profile page is not displayed")]
        public async Task ThenTheProfilePageIsNotDisplayed()
        {
            var message = await loginPage.GetInvalidLoginMessageAsync();
            Assert.That(message, Is.EqualTo("Invalid username or password!"));
        }

    }
}
