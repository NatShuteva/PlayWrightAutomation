using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace PlayWrightAutomation.Tests
{
    internal class TraceDebug
    {

        [Test]
        public async Task DeleteProductFromTheCartTraceDebug()
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = false });

            await using var context = await browser.NewContextAsync();
            await context.Tracing.StartAsync(new()
            {
                Title = "DeleteProductFromTheCartTraceDebug",
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });

            var page = await context.NewPageAsync();

            try
            {
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
            finally
            {
                await context.Tracing.StopAsync(new()
                {
                    Path = Path.Combine("traces", $"DeleteProductFromTheCartTraceDebug-{DateTime.Now:yyyyMMdd-HHmmss}.zip")
                });
                Console.WriteLine(Path.GetFullPath(Path.Combine("traces",$"DeleteProductFromTheCartTraceDebug-{DateTime.Now:yyyyMMdd-HHmmss}.zip")));
            }
        }
    }
}
