using Microsoft.Playwright;
using NUnit.Framework;
using PlayWrightAutomation.PageObject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PlayWrightAutomation.Tests
{
    internal class MainTestPageObjectDataDriven
    {
        public class LoginTestData
        {
            public string? Username { get; set; }   //get read the value from the json file
            public string? Password { get; set; }   //set assigan a value to the property
        }

        //this code read the data drom the LoginTestData.json file and return the data in a dictionary with the username as the key and the LoginTestData object as the value
        private static Dictionary<string, LoginTestData> GetLoginDatasetsByUser()
        {
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "Data", "LoginTestData.json");
            var json = File.ReadAllText(jsonPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var rootElement = JsonDocument.Parse(json).RootElement;
            if (rootElement.ValueKind == JsonValueKind.Array)
            {
                var datasets = JsonSerializer.Deserialize<List<LoginTestData>>(json, options);
                if (datasets == null || datasets.Count == 0)
                    throw new InvalidOperationException("LoginTestData.json contains an empty dataset array.");

                return datasets
                    .Where(d => !string.IsNullOrWhiteSpace(d.Username) && !string.IsNullOrWhiteSpace(d.Password))
                    .ToDictionary(d => d.Username!, d => d, StringComparer.OrdinalIgnoreCase);
            }

            var single = JsonSerializer.Deserialize<LoginTestData>(json, options);
            if (single == null || string.IsNullOrWhiteSpace(single.Username) || string.IsNullOrWhiteSpace(single.Password))
                throw new InvalidOperationException("LoginTestData.json object is missing username or password.");

            return new Dictionary<string, LoginTestData>(StringComparer.OrdinalIgnoreCase)
            {
                [single.Username!] = single
            };
        }

        [Test]
        public async Task LoginWithJsonData_NatashaQA()
        {
            await LoginWithNamedDataset("NatashaQA");
        }

        [Test]
        public async Task LoginWithJsonData_Natasha1()
        {
            await LoginWithNamedDataset("Natasha1");
        }

        private async Task LoginWithNamedDataset(string datasetName)
        {
            var datasets = GetLoginDatasetsByUser();
            Assert.That(datasets.ContainsKey(datasetName), Is.True, $"Dataset '{datasetName}' was not found in LoginTestData.json.");

            var username = datasets[datasetName].Username!;
            var password = datasets[datasetName].Password!;


            // Create Playwright
            var playwright = await Playwright.CreateAsync();

            // Launch browser
            var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = false
                });

            // Create context
            var context = await browser.NewContextAsync();

            // Create page
            var page = await context.NewPageAsync();
            try
            {
                // Create object instance
                var loginPage = new LoginPage(page);
                await loginPage.GoToAction();
                await loginPage.validLogin(username, password);
                await page.WaitForURLAsync("**/profile", new PageWaitForURLOptions { Timeout = 30000 });
                Assert.That(page.Url, Does.Contain("/profile"));

                var profilePage = new ProfilePage(page);
                var allTitles = await profilePage.GetAllBookTitlesAsync();
                Assert.That(allTitles.Count, Is.GreaterThan(0), "No books found in profile to use for search validation.");

                var someBook = allTitles[0];
                await profilePage.SearchAsync(someBook);
                await profilePage.GoToBookStoreAsync();

                var bookStorePage = new BookStorePage(page);
                await bookStorePage.SearchAsync(someBook);
            }
            finally
            {
                await context.CloseAsync();
                await browser.CloseAsync();
                playwright.Dispose();
            }
        }
    }
}
