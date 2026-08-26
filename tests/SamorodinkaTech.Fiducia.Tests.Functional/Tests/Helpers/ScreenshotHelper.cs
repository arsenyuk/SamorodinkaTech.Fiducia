using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Временный хелпер для отладки — делает скриншот страницы.
/// </summary>
public static class ScreenshotHelper
{
    public static async Task TakeScreenshotAsync(IPage page, string filename)
    {
        var path = Path.Combine(Path.GetTempPath(), filename);
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
        Console.WriteLine($"[Screenshot] Сохранён: {path}");
    }
}
