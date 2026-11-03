using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayWrightAutomation.Utils
{
    internal class ImprovedAPIUtils
    {
        private readonly IAPIRequestContext apiContext;

        private const string BaseUrl = "https://demoqa.com";
        private const string UserName = "NatashaQA";
        private const string Password = "Tester1234$";

        public ImprovedAPIUtils(IAPIRequestContext apiContext)
        {
            this.apiContext = apiContext;
        }

        private object LoginPayload => new
        {
            userName = UserName,
            password = Password

        };


        public async Task<(string Token, string UserId)> GetLoginAuthData()
        {
            var payload = LoginPayload;
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(payload));

            var response = await apiContext.PostAsync(
                $"{BaseUrl}/Account/v1/Login",
                new APIRequestContextOptions
                {
                    DataObject = LoginPayload
                });

            if (!response.Ok)
                throw new Exception(
                    $"Login failed. Status: {response.Status}");

            var json = await response.JsonAsync();

            if (!json.HasValue)
                throw new Exception(
                    "Login response body is empty.");

            var token = json.Value.GetProperty("token").GetString();

            var userId =json.Value.GetProperty("userId").GetString();

            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Token is empty.");

            if (string.IsNullOrWhiteSpace(userId))
                throw new Exception("UserId is empty.");

            return (token, userId);
        }

        public async Task<IAPIResponse> DeleteAllBooks(
            string userId,
            string token)
        {
            return await apiContext.DeleteAsync(
                $"{BaseUrl}/BookStore/v1/Books?UserId={userId}",
                new APIRequestContextOptions
                {
                    Headers = CreateAuthHeader(token)
                });
        }

        public async Task<IAPIResponse> AddBooks(
            string userId,
            IEnumerable<string> isbns,
            string token)
        {
            var payload = new
            {
                userId,
                collectionOfIsbns =
                    isbns.Select(isbn => new { isbn }).ToArray()
            };

            return await apiContext.PostAsync(
                $"{BaseUrl}/BookStore/v1/Books",
                new APIRequestContextOptions
                {
                    Headers = CreateAuthHeader(token),
                    DataObject = payload
                });
        }

        private Dictionary<string, string> CreateAuthHeader(
            string token)
        {
            return new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}",
                ["Content-Type"] = "application/json"
            };
        }
    }
}
