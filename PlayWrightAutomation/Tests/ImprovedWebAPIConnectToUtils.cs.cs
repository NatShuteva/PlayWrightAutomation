using Microsoft.Playwright;
using PlayWrightAutomation.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayWrightAutomation.Tests
{
    internal class ImprovedWebAPIConnectToUtils
    {
        [NonParallelizable]
        internal class ImpWebAPIConnectToUtils
        {
            private IPlaywright playwright = null!;
            private IAPIRequestContext apiContext = null!;
            private ImprovedAPIUtils apiUtils = null!;

            [SetUp]
            public async Task Setup()
            {
                playwright = await Playwright.CreateAsync();

                apiContext =
                    await playwright.APIRequest.NewContextAsync(
                        new APIRequestNewContextOptions
                        {
                            IgnoreHTTPSErrors = true
                        });

                apiUtils = new ImprovedAPIUtils(apiContext);
            }

            [TearDown]
            public async Task TearDown()
            {
                await apiContext.DisposeAsync();
                playwright.Dispose();
            }

            [Test]
            public async Task TestAddBooks()
            {
                // Login
                var authData = await apiUtils.GetLoginAuthData();

                // Test Data
                var isbns = new[]
                {
                "9781449325862",
                "9781449331818"
            };

                // Cleanup existing books
                var deleteResponse = await apiUtils.DeleteAllBooks(authData.UserId,authData.Token);

                Assert.That(deleteResponse.Status,Is.EqualTo(204).Or.EqualTo(200));

                // Add books
                var addBooksResponse = await apiUtils.AddBooks(authData.UserId,isbns,authData.Token);

                Assert.That(addBooksResponse.Status, Is.EqualTo(201));
                Console.WriteLine(await addBooksResponse.TextAsync());
            }
        }
    }
}