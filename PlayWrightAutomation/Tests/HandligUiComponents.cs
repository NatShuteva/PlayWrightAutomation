using Allure.NUnit;
using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using UglyToad.PdfPig.Content;


namespace PlayWrightAutomation.Tests
{
    [AllureNUnit]
    internal class HandligUiComponents
    {
        [Test]
        public async Task SelectItemFromTheDropdown()
        {

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            await using var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await page.GotoAsync("https://demoqa.com/automation-practice-form");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/automation-practice-form"));
            var formHeader = page.Locator("h1.text-center");
            Assert.That((await formHeader.InnerTextAsync()).Trim(), Is.EqualTo("Practice Form"));
            Console.WriteLine("Page title is: " + await formHeader.InnerTextAsync());

            await page.Locator("#state").ScrollIntoViewIfNeededAsync();
            await page.Locator("#state").ClickAsync();
            await page.Locator("#react-select-3-input").FillAsync("Rajasthan");
            await page.Keyboard.PressAsync("Enter");
            var selectedState = await page.Locator("#state").TextContentAsync();
            Assert.That(selectedState, Does.Contain("Rajasthan"));

            await page.Locator("#city").ClickAsync();
            await page.Locator("#react-select-4-input").FillAsync("Jaipur");
            await page.Keyboard.PressAsync("Enter");
            var selectedCity = await page.Locator("#city").TextContentAsync();
            Assert.That(selectedCity, Does.Contain("Jaipur"));
            await page.WaitForTimeoutAsync(3000);
        }

        [Test]
        public async Task SelectItemFromTheRadioButton()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://demoqa.com/automation-practice-form");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/automation-practice-form"));
            var formHeader = page.Locator("h1.text-center");
            Assert.That((await formHeader.InnerTextAsync()).Trim(), Is.EqualTo("Practice Form"));
            Console.WriteLine("Page title is: " + await formHeader.InnerTextAsync());
            var femaleRadio = page.Locator("#gender-radio-2");
            await femaleRadio.ClickAsync();
            Assert.That(await femaleRadio.IsCheckedAsync(), Is.True);
            await page.WaitForTimeoutAsync(3000);
        }

        [Test]
        public async Task SelectTheCheckbox()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://demoqa.com/automation-practice-form");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/automation-practice-form"));
            var formHeader = page.Locator("h1.text-center");
            Assert.That((await formHeader.InnerTextAsync()).Trim(), Is.EqualTo("Practice Form"));
            Console.WriteLine("Page title is: " + await formHeader.InnerTextAsync());
            var hobbiesCheckbox = page.GetByLabel("Sports");
            await hobbiesCheckbox.ClickAsync();
            Assert.That(await hobbiesCheckbox.IsCheckedAsync(), Is.True);
            await page.WaitForTimeoutAsync(3000);
            await hobbiesCheckbox.ClickAsync();
            Assert.That(await hobbiesCheckbox.IsCheckedAsync(), Is.False);
            await page.WaitForTimeoutAsync(3000);
        }

        [Test]
        public async Task GrabTheInputValue()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://demoqa.com/automation-practice-form");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/automation-practice-form"));
            var formHeader = page.Locator("h1.text-center");
            Assert.That((await formHeader.InnerTextAsync()).Trim(), Is.EqualTo("Practice Form"));
            Console.WriteLine("Page title is: " + await formHeader.InnerTextAsync());

            var firstNameInput = page.Locator("#firstName");
            await firstNameInput.FillAsync("Natasha");

            var enteredFirstName = await firstNameInput.InputValueAsync();
            Assert.That(enteredFirstName, Is.EqualTo("Natasha"));
            Console.WriteLine("Entered First Name is: " + enteredFirstName);
        }


        [Test]
        public async Task ChildWindowHendling()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://demoqa.com/browser-windows");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/browser-windows"));
            var formHeader = page.Locator("h1.text-center");
            Assert.That((await formHeader.InnerTextAsync()).Trim(), Is.EqualTo("Browser Windows"));
            Console.WriteLine("Page title is: " + await formHeader.InnerTextAsync());

            var childWindow = page.WaitForPopupAsync();
            await page.Locator("#messageWindowButton").ClickAsync();
            var window = await childWindow;
            var messageText = (await window.Locator("body").InnerTextAsync()).Trim();
            Assert.That(messageText, Does.Contain("Please share this website"));
            Console.WriteLine("Message text is: " + messageText);
            await page.WaitForTimeoutAsync(3000);
        }

        [Test]
        public async Task SecondTabHendling()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://demoqa.com/browser-windows");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/browser-windows"));
            var formHeader = page.Locator("h1.text-center");
            Assert.That((await formHeader.InnerTextAsync()).Trim(), Is.EqualTo("Browser Windows"));
            Console.WriteLine("Page title is: " + await formHeader.InnerTextAsync());

            var newTabTask = page.WaitForPopupAsync();
            await page.Locator("#tabButton").ClickAsync();
            var newTab = await newTabTask;

            var newTabHeading = (await newTab.Locator("#sampleHeading").InnerTextAsync()).Trim();
            Assert.That(newTabHeading, Is.EqualTo("This is a sample page"));
            Console.WriteLine("New tab heading is: " + newTabHeading);
            await page.WaitForTimeoutAsync(3000);
        }

        [Test]
        public async Task DynamicProperties()
        {

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://demoqa.com/dynamic-properties");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/dynamic-properties"));
            var formHeader = page.Locator("h1.text-center");
            Assert.That((await formHeader.InnerTextAsync()).Trim(), Is.EqualTo("Dynamic Properties"));
            Console.WriteLine("Page title is: " + await formHeader.InnerTextAsync());

            var colorChangeButton = page.GetByRole(AriaRole.Button, new() { Name = "Color Change" });

            var colorTask = page.WaitForFunctionAsync(@"element => getComputedStyle(element).color === 'rgb(220, 53, 69)'",
            await colorChangeButton.ElementHandleAsync());

            var visibleAfter5SecButton = page.GetByRole(AriaRole.Button, new() { Name = "Visible After 5 Seconds" });

            var visibleTask = visibleAfter5SecButton.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 7000
            });

            await Task.WhenAll(colorTask, visibleTask);

            string color = await colorChangeButton.EvaluateAsync<string>("e => getComputedStyle(e).color");
            Assert.That(color, Is.EqualTo("rgb(220, 53, 69)"));
            Console.WriteLine($"Button text color: {color}");
            Assert.That(await visibleAfter5SecButton.IsVisibleAsync(), Is.True);
            Console.WriteLine($"The button '{await visibleAfter5SecButton.InnerTextAsync()}' is displayed.");

        }

        [Test]
        public async Task JavaPopups()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            await using var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await page.GotoAsync("https://demoqa.com/alerts");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/alerts"));
            var formHeader = page.Locator("h1.text-center");
            Assert.That((await formHeader.InnerTextAsync()).Trim(), Is.EqualTo("Alerts"));
            Console.WriteLine("Page title is: " + await formHeader.InnerTextAsync());
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var alertButton = page.Locator("#alertButton").First;
            await alertButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });
            await alertButton.ScrollIntoViewIfNeededAsync();

            var dialogTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            async void OnDialog(object? _, IDialog dialog)
            {
                try
                {
                    var message = dialog.Message;
                    await dialog.AcceptAsync();
                    dialogTcs.TrySetResult(message);
                }
                catch (Exception ex)
                {
                    dialogTcs.TrySetException(ex);
                }
            }

            context.Dialog += OnDialog;
            try
            {
                try
                {
                    await alertButton.ClickAsync(new LocatorClickOptions { Force = true, Timeout = 5000 });
                }
                catch (TimeoutException)
                {
                    await page.EvaluateAsync("() => document.querySelector('#alertButton')?.click()");
                }

                var alertMessage = await dialogTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.That(alertMessage, Is.EqualTo("You clicked a button"));
            }
            finally
            {
                context.Dialog -= OnDialog;
            }

        }

        [Test]
        public async Task Hoverover()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://demoqa.com/tool-tips");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/tool-tips"));
            var formHeader = page.Locator("h1.text-center");
            Assert.That((await formHeader.InnerTextAsync()).Trim(), Is.EqualTo("Tool Tips"));
            Console.WriteLine("Page title is: " + await formHeader.InnerTextAsync());

            var hoverButton = page.Locator("#toolTipButton");
            await hoverButton.ScrollIntoViewIfNeededAsync();
            await hoverButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 10000
            });

            var tooltip = page.Locator("#buttonToolTip .tooltip-inner");
            var tooltipVisible = false;
            for (var i = 0; i < 3; i++)
            {
                await hoverButton.HoverAsync(new LocatorHoverOptions
                {
                    Force = true
                });

                try
                {
                    await tooltip.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 2500
                    });
                    tooltipVisible = true;
                    break;
                }
                catch (TimeoutException)
                {
                    await page.Mouse.MoveAsync(0, 0);
                }
            }

            Assert.That(tooltipVisible, Is.True, "Tooltip did not become visible after hover retries.");

            var tooltipText = (await tooltip.InnerTextAsync()).Trim();
            Assert.That(tooltipText, Is.EqualTo("You hovered over the Button"));
            Console.WriteLine("Tooltip text is: " + tooltipText);
        }


        [Test]
        public async Task NestedFrames()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://demoqa.com/nestedframes");
            Assert.That(page.Url, Does.StartWith("https://demoqa.com/nestedframes"));
            var formHeader = page.Locator("h1.text-center");
            Assert.That((await formHeader.InnerTextAsync()).Trim(), Is.EqualTo("Nested Frames"));
            Console.WriteLine("Page title is: " + await formHeader.InnerTextAsync());

            var parentFrame = page.FrameLocator("#frame1");
            var parentText = (await parentFrame.Locator("body").InnerTextAsync()).Trim();
            Assert.That(parentText, Does.Contain("Parent frame"));
            Console.WriteLine("Parent frame text is: " + parentText);

            var childFrame = parentFrame.FrameLocator("iframe[srcdoc*='Child Iframe']");
            var childText = (await childFrame.Locator("body").InnerTextAsync()).Trim();
            Assert.That(childText, Is.EqualTo("Child Iframe"));
            Console.WriteLine("Child frame text is: " + childText);
        }

    }

}

