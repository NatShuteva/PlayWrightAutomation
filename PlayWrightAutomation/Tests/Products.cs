using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;

namespace PlayWrightAutomation.Tests
{
    internal class Products
    {

        [Test]
        public async Task GetTheTiteleFormTheProductCard()
        {

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://www.saucedemo.com/");

            Assert.That(page.Url, Does.StartWith("https://www.saucedemo.com"));
            Assert.That(await page.TitleAsync(), Is.EqualTo("Swag Labs"));
            Console.WriteLine("Page title is: " + await page.TitleAsync());

            var userName = page.Locator("#user-name");
            await userName.FillAsync("standard_user");
            var password = page.Locator("[type='password']");
            await password.FillAsync("secret_sauce");
            var loginButton = page.Locator("#login-button");
            await loginButton.ClickAsync();
            Assert.That(page.Url, Does.StartWith("https://www.saucedemo.com/inventory.html"));
            Console.WriteLine("Successfully logged in. Current URL: " + page.Url);
            var productsTitle = page.Locator(".title");
            Assert.That(await productsTitle.InnerTextAsync(), Is.EqualTo("Products"));
            Console.WriteLine("Products page title is: " + await productsTitle.InnerTextAsync());

            //grab the title of the first product card
            var poductCardTitle = page.Locator(".inventory_item_name");
            Assert.That(await poductCardTitle.First.InnerTextAsync(), Is.EqualTo("Sauce Labs Backpack"));
            Console.WriteLine("First product card title is: " + await poductCardTitle.First.InnerTextAsync());

            //grab the title of the second product card
            Assert.That(await poductCardTitle.Nth(1).InnerTextAsync(), Is.EqualTo("Sauce Labs Bike Light"));
            Console.WriteLine("Second product card title is: " + await poductCardTitle.Nth(1).InnerTextAsync());

            //grab all the titles of the product cards
            var allProductCardTitles = await poductCardTitle.AllTextContentsAsync();
            Console.WriteLine(allProductCardTitles.Count);
            foreach (var title in allProductCardTitles)
            {
                Console.WriteLine("Product card title: " + title);
            }
        }

        [Test]
        public async Task OrderProduct()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://www.saucedemo.com/");

            await page.Locator("#user-name").FillAsync("standard_user");
            await page.Locator("#password").FillAsync("secret_sauce");
            await page.Locator("#login-button").ClickAsync();

            await page.WaitForURLAsync("**/inventory.html");
            await page.Locator(".inventory_item").First.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            var productName = "Sauce Labs Bike Light";
            var targetProduct = page.Locator(".inventory_item").Filter(new()
            {
                HasTextString = productName
            });
            await targetProduct.First.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await targetProduct.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();
            

            var shoppingCartBadge = page.Locator(".shopping_cart_badge");
            await shoppingCartBadge.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            Assert.That((await shoppingCartBadge.InnerTextAsync()).Trim(), Is.EqualTo("1"));
            //Assert.That(async () => (await shoppingCartBadge.InnerTextAsync()).Trim(),Is.EqualTo("1").After(10000, 500));
            await page.Locator(".shopping_cart_link").ClickAsync();
            await page.WaitForURLAsync("**/cart.html");
            Assert.That(page.Url, Does.Contain("/cart.html"));
            await page.Locator("#continue-shopping").WaitForAsync(new() { State = WaitForSelectorState.Visible });
            var headerTitle = page.Locator(".header_secondary_container .title");
            await headerTitle.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            Assert.That(((await headerTitle.TextContentAsync()) ?? string.Empty).Trim(), Is.EqualTo("Your Cart"));


            var cartItems = page.Locator(".cart_item");
            var cartCount = await cartItems.CountAsync();

            var productFound = false;

            for (int i = 0; i < cartCount; i++)
            {
                var name = (await cartItems.Nth(i).Locator(".inventory_item_name").InnerTextAsync()).Trim();
                if (name.Equals(productName, StringComparison.OrdinalIgnoreCase))
                {
                    productFound = true;
                    Console.WriteLine("Found product in cart: " + name);
                    break;
                }
            }

            Assert.That(productFound, Is.True, $"Product '{productName}' was not found in cart.");
            await page.WaitForTimeoutAsync(2000);

            await page.Locator("#checkout").ClickAsync();
            await page.WaitForURLAsync("**/checkout-step-one.html");
            // optional assert
            Assert.That(page.Url, Does.Contain("checkout-step-one.html"));

            await page.Locator("#first-name").FillAsync("Natasha");
            await page.Locator("#last-name").FillAsync("Shuteva");
            await page.Locator("#postal-code").FillAsync("2000");

            await page.Locator("#continue").ClickAsync();
            await page.WaitForURLAsync("**/checkout-step-two.html");
            await page.WaitForTimeoutAsync(2000);

            var overviewTitle = page.Locator(".title");
            Assert.That((await overviewTitle.InnerTextAsync()).Trim(), Is.EqualTo("Checkout: Overview"));
           

            var itemInList = page.Locator(".inventory_item_name");
            Assert.That((await itemInList.First.InnerTextAsync()).Trim(), Is.EqualTo(productName));

            await page.Locator("#finish").ClickAsync();
            await page.WaitForURLAsync("**/checkout-complete.html");
            await page.WaitForTimeoutAsync(3000);

            var completeTitle = page.Locator(".title");
            Assert.That((await completeTitle.InnerTextAsync()).Trim(), Is.EqualTo("Checkout: Complete!"));

            var successMessage = page.Locator(".complete-header");
            await successMessage.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            Assert.That((await successMessage.InnerTextAsync()).Trim(), Is.EqualTo("Thank you for your order!"));

            var download = await page.RunAndWaitForDownloadAsync(async () =>
            {
                await page.Locator("#generate-pdf-order").ClickAsync();
            });

            var downloadsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            Directory.CreateDirectory(downloadsFolder);

            var downloadedPdfPath = Path.Combine(downloadsFolder, download.SuggestedFilename);
            await download.SaveAsAsync(downloadedPdfPath);

            Assert.That(File.Exists(downloadedPdfPath), Is.True, "PDF order document was not downloaded.");

            var pdfText = PlayWrightAutomation.Helpers.Helpers.ExtractPdfText(downloadedPdfPath); 

            // normalize spaces/newlines
            var normalized = Regex.Replace(pdfText, @"\s+", " ").Trim();

            // SHIP TO section: FirstName, LastName, PostalCode
            var shipToMatch = Regex.Match(
                normalized,
                @"SHIP TO\s*(?<first>[A-Za-z]+)\s+(?<last>[A-Za-z]+)\s*(?<postal>\d{3,10})\s*ITEMS",
                RegexOptions.IgnoreCase);

            Assert.That(shipToMatch.Success, Is.True, "Could not parse SHIP TO section from PDF.");

            var firstName = shipToMatch.Groups["first"].Value;
            var lastName = shipToMatch.Groups["last"].Value;
            var postalCode = shipToMatch.Groups["postal"].Value;

            // DESCRIPTION/PRICE section: Product Name
            var productMatch = Regex.Match(
                normalized,
                @"DESCRIPTION\s*PRICE\s*(?<product>.*?)\s*\$\d+\.\d{2}\s*Item total",
                RegexOptions.IgnoreCase);

            Assert.That(productMatch.Success, Is.True, "Could not parse product name from PDF.");

            var productNameFromPdf = productMatch.Groups["product"].Value.Trim();

            // print filtered values
            Console.WriteLine($"First Name: {firstName}");
            Console.WriteLine($"Last Name: {lastName}");
            Console.WriteLine($"Postal Code: {postalCode}");
            Console.WriteLine($"Product Name: {productNameFromPdf}");

            // verify expected
            Assert.That(firstName, Is.EqualTo("Natasha"));
            Assert.That(lastName, Is.EqualTo("Shuteva"));
            Assert.That(postalCode, Is.EqualTo("2000"));
            Assert.That(productNameFromPdf, Is.EqualTo(productName));
        }


        [Test]
        public async Task DeleteProductFromTheCart()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false
            });

            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://www.saucedemo.com/");

            await page.Locator("#user-name").FillAsync("standard_user");
            await page.Locator("#password").FillAsync("secret_sauce");
            await page.Locator("#login-button").ClickAsync();

            await page.WaitForURLAsync("**/inventory.html");
            await page.Locator(".inventory_item").First.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            var productName = "Sauce Labs Bike Light";
            var targetProduct = page.Locator(".inventory_item").Filter(new()
            {
                HasTextString = productName
            });
            await targetProduct.First.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            await targetProduct.GetByRole(AriaRole.Button, new() { Name = "Add to cart" }).ClickAsync();


            var shoppingCartBadge = page.Locator(".shopping_cart_badge");
            await shoppingCartBadge.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            Assert.That((await shoppingCartBadge.InnerTextAsync()).Trim(), Is.EqualTo("1"));
            await page.Locator(".shopping_cart_link").ClickAsync();
            await page.WaitForURLAsync("**/cart.html");
            Assert.That(page.Url, Does.Contain("/cart.html"));
            await page.Locator("#continue-shopping").WaitForAsync(new() { State = WaitForSelectorState.Visible });
            var headerTitle = page.Locator(".header_secondary_container .title");
            await headerTitle.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            Assert.That(((await headerTitle.TextContentAsync()) ?? string.Empty).Trim(), Is.EqualTo("Your Cart"));

            await page.Locator("#remove-sauce-labs-bike-light").ClickAsync();
            var cartItems = page.Locator(".cart_item");
            var cartCount = await cartItems.CountAsync();

            var productFound = false;
            for (int i = 0; i < cartCount; i++)
            {
                var name = (await cartItems.Nth(i).Locator(".inventory_item_name").InnerTextAsync()).Trim();
                if (name.Equals(productName, StringComparison.OrdinalIgnoreCase))
                {
                    productFound = true;
                    Console.WriteLine("Found product in cart: " + name);
                    break;
                }
            }
            Assert.That(productFound, Is.False, $"Product '{productName}' was found in cart.");

            await page.Locator("#continue-shopping").ClickAsync();
            await page.Locator(".inventory_item").First.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            var addToCartButtonText = string.Empty;
            var bikeLightFoundOnInventory = false;
            var inventoryCards = page.Locator(".inventory_item");
            var inventoryCount = await inventoryCards.CountAsync();

            for (int i = 0; i < inventoryCount; i++)
            {
                var card = inventoryCards.Nth(i);
                var name = (await card.Locator(".inventory_item_name").InnerTextAsync()).Trim();

                if (name.Equals(productName, StringComparison.OrdinalIgnoreCase))
                {
                    bikeLightFoundOnInventory = true;
                    addToCartButtonText = (await card.GetByRole(AriaRole.Button, new() { Name = "Add to cart", Exact = true }).InnerTextAsync()).Trim();
                    break;
                }
            }

            Assert.That(bikeLightFoundOnInventory, Is.True, $"Product '{productName}' was not found after continue shopping.");
            Console.WriteLine($"Button name for '{productName}': {addToCartButtonText}");
            Assert.That(addToCartButtonText, Is.EqualTo("Add to cart"));
            await page.WaitForTimeoutAsync(2000);

        }
    }
}
