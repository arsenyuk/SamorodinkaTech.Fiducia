using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Временный тест для проверки работоспособности входа.
/// </summary>
public class LoginTest
{
    [Fact]
    public async Task TestLoginAdminConsole()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });

        try
        {
            await AuthHelper.LoginAsAdminAsync(page, "v.vasilyeva", "1");
            var url = page.Url;
            Console.WriteLine($"[Test] URL после входа: {url}");
            url.Should().Contain("/main");
        }
        catch (Exception ex)
        {
            // Сохраняем скриншот при ошибке
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(Path.GetTempPath(), "login_test_error.png"),
                FullPage = true
            });
            Console.WriteLine($"[Test] Ошибка: {ex.Message}");
            throw;
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
