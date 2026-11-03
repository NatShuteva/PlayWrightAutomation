using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlayWrightAutomation.Utils
{
    public class APIUtils
    {
        private readonly IAPIRequestContext apiContext;

        public APIUtils(IAPIRequestContext apiContext)
        {
            this.apiContext = apiContext;
        }

        public async Task<string> GetToken()
        {
            var loginPayload = new
            {
                userName = "NatashaQA",
                password = "Tester1234$"
            };

            var loginResponse = await this.apiContext.PostAsync(
                "https://demoqa.com/Account/v1/GenerateToken", new APIRequestContextOptions
                {
                    DataObject = loginPayload
                });

            if (!loginResponse.Ok)
                throw new Exception($"GenerateToken failed. Status: {loginResponse.Status}");

            var json = await loginResponse.JsonAsync();
            if (!json.HasValue)
                throw new Exception("GenerateToken response body is empty.");

            var token = json.Value.GetProperty("token").GetString();
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token is null or empty.");

            return token;
        }

        public async Task<(string Token, string UserId)> GetLoginAuthData()
        {
            var loginPayload = new
            {
                userName = "NatashaQA",
                password = "Tester1234$"
            };

            var loginResponse = await this.apiContext.PostAsync(
                "https://demoqa.com/Account/v1/Login",
                new APIRequestContextOptions
                {
                    DataObject = loginPayload
                });

            if (!loginResponse.Ok)
                throw new Exception($"Login failed. Status: {loginResponse.Status}");

            var json = await loginResponse.JsonAsync();
            if (!json.HasValue)
                throw new Exception("Login response body is empty.");

            var token = json.Value.GetProperty("token").GetString();
            var userId = json.Value.GetProperty("userId").GetString();

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
                throw new Exception("Token or userId is null/empty from Login response.");

            return (token, userId);
        }

        public async Task<IAPIResponse> AddBooks(string userId, IEnumerable<string> isbns, string token)
        {
            var addBookPayload = new
            {
                userId,
                collectionOfIsbns = isbns.Select(isbn => new { isbn }).ToArray()
            };

            var addBookResponse = await this.apiContext.PostAsync(
                "https://demoqa.com/BookStore/v1/Books",
                new APIRequestContextOptions
                {
                    Headers = new Dictionary<string, string>
                    {
                        { "Authorization", $"Bearer {token}" },
                        { "Content-Type", "application/json" }
                    },
                    DataObject = addBookPayload
                });

            return addBookResponse;
        }
    }

}