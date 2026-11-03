using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

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
            var apiUtils = new Utils.APIUtils(apiContext);
            var authData = await apiUtils.GetLoginAuthData();
            var isbns = new[] { "9781449325862", "9781449331818" };

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

            var response = await apiUtils.AddBooks(authData.UserId, isbns, authData.Token);
            Assert.That(response.Status, Is.EqualTo(201));
        }
    }
}
