using Microsoft.Playwright;
using NUnit.Framework;
using PlayWrightAutomation.Utils;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;

namespace PlayWrightAutomation.Tests
{
    internal class FixtureDemo
    {
        [Test]
        public async Task FixtureDemoLoginTest()
        {
            var session = await Fixtures.CreateLoggedInSessionAsync();
            try
            {
                var resultText = await session.Page.InnerTextAsync("#userName-value");
                Assert.That(resultText.Trim(), Is.EqualTo("NatashaQA"));
            }
            finally
            {
                await Fixtures.CloseSessionAsync(session.Playwright, session.Browser, session.Context);
            }
        }

        [Test]
        public async Task FixtureDemoAddBook()
        {
            var session = await Fixtures.CreateLoggedInSessionAsync();
            try
            {
                var resultText = await session.Page.InnerTextAsync("#userName-value");
                Assert.That(resultText.Trim(), Is.EqualTo("NatashaQA"));

                var deletedTitle = await Fixtures.DeleteLastBookFromProfileAsync(session.Page);
                Assert.That(string.IsNullOrWhiteSpace(deletedTitle), Is.False, "Expected a book to be deleted after seeding one.");
     
                var addedTitle = await Fixtures.AddLastBookToProfileAsync(session.Page);
                Assert.That(string.IsNullOrWhiteSpace(addedTitle), Is.False);
            }
            finally
            {
                await Fixtures.CloseSessionAsync(session.Playwright, session.Browser, session.Context);
            }
        }

    }
}
