using Microsoft.Playwright;
using Reqnroll;
using System.Threading.Tasks;

namespace PlayWrightAutomation.Features.Support
{
    public class PlaywrightDriver
    {
        public IPlaywright Playwright { get; set; } = null!;
        public IBrowser Browser { get; set; } = null!;
        public IBrowserContext Context { get; set; } = null!;
        public IPage Page { get; set; } = null!;
    }

    [Binding]
    public class Hooks
    {
        private readonly PlaywrightDriver driver;

        public Hooks(PlaywrightDriver driver)
        {
            this.driver = driver;
        }

        [BeforeScenario]  //runs before every test scenario
        public async Task Setup()
        {
            driver.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            driver.Browser = await driver.Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
            driver.Context = await driver.Browser.NewContextAsync();
            driver.Page = await driver.Context.NewPageAsync();
        }

        [AfterScenario]   //runs after every test scenario
        public async Task Cleanup()
        {
            if (driver.Context != null)
                await driver.Context.CloseAsync();
            if (driver.Browser != null)
                await driver.Browser.CloseAsync();
            driver.Playwright?.Dispose();
        }
    }
}
