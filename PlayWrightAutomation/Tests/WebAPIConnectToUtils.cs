using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace PlayWrightAutomation.Tests
{
    [NonParallelizable]
    internal class WebAPIConnectToUtils
    {
        [Test]
        public async Task TestAddBooks()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var apiContext = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
            {
                IgnoreHTTPSErrors = true
            });
            //Create APIUtils object
            var apiUtils = new Utils.APIUtils(apiContext);
            var authData = await apiUtils.GetLoginAuthData();
            //define books to add (creates an array containing two ISBN numbers)
            var isbns = new[] { "9781449325862", "9781449331818" };

            //delete all existing books
            var deleteResponse = await apiContext.DeleteAsync(
                $"https://demoqa.com/BookStore/v1/Books?UserId={authData.UserId}",
                new APIRequestContextOptions
                {
                    Headers = new Dictionary<string, string>
                    {
                        { "Authorization", $"Bearer {authData.Token}" }
                    }
                });
            Assert.That(deleteResponse.Status, Is.EqualTo(204).Or.EqualTo(200));

            //add books using the AddBooks method
            var response = await apiUtils.AddBooks(authData.UserId, isbns, authData.Token);
            Assert.That(response.Status, Is.EqualTo(201));
        }
    }
}
