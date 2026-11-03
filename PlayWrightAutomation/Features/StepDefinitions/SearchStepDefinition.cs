using Microsoft.Playwright;
using NUnit.Framework;
using PlayWrightAutomation.Features.Support;
using PlayWrightAutomation.PageObject;
using Reqnroll;
using System.Threading.Tasks;

namespace PlayWrightAutomation.Features.StepDefinitions
{
    [Binding]
    public class SearchStepDefinition
    {
        private readonly SearchPage searchPage;
        private string searchedTitle = string.Empty;
        private SearchPage details = null!;

        public SearchStepDefinition(PlaywrightDriver driver)
        {
            searchPage = new SearchPage(driver.Page);
        }

        [Given("the Book Store page is open")]
        public async Task GivenTheBookStorePageIsOpen()
        {
            await searchPage.OpenBookStorePageAsync();
        }

        [When(@"enter text book in the search field ""(.*)""")]
        public async Task WhenEnterTextBookInTheSearchField(string title)
        {
            searchedTitle = title;
            await searchPage.SearchByTitleAsync(title);
        }

        [Then("the search results are displayed")]
        public async Task ThenTheSearchResultsAreDisplayed()
        {
            var text = await searchPage.GetFirstResultTitleAsync();
            Assert.That(text, Does.Contain(searchedTitle).IgnoreCase);
        }

        [When("click on book title from the filtered book")]
        public async Task WhenClickOnBookTitleAsync()
        {
            searchedTitle = await searchPage.GetFirstResultTitleAsync();
            await searchPage.ClickBookTitleAsync(searchedTitle);
        }

        [Then("book details are displayed correctly")]
        public async Task ThenBookDetailsAreDisplayedCorrectly()
        {
            details = await searchPage.GetBookDetailsAsync();

            Assert.That(details.Title, Is.EqualTo(searchedTitle));
            Assert.That(details.Isbn, Does.Match(@"^\d{10}(\d{3})?$"));
            Assert.That(details.Author, Is.Not.Null.And.Not.Empty);
            Assert.That(details.Publisher, Is.Not.Null.And.Not.Empty);
            Assert.That(details.SubTitle, Is.Not.Null);
            Assert.That(int.TryParse(details.TotalPages, out var pages) && pages > 0, Is.True,
                "Total pages should be a positive number");
            Assert.That(details.Description, Is.Not.Null.And.Not.Empty);
        }

        [When("click on the Back To Book Store button")]
        public async Task WhenClickOnTheBackToBookStoreButton()
        {
            await searchPage.ClickBackToBookStoreAsync();
        }

        [Then("the Book Store page is opened")]
        public async Task ThenTheBookStorePageIsOpened()
        {
            await searchPage.WaitForBookStorePageAsync();
            Assert.That(searchPage.GetCurrentUrl(), Does.Contain("/books"));
        }

    }
}
