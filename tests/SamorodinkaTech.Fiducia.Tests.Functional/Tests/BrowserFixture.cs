using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Базовый класс — предоставляет доступ к браузеру из GlobalFixture.
/// НЕ создаёт свой экземпляр браузера — использует общий из GlobalFixture.
/// </summary>
public class BrowserFixture
{
    private readonly GlobalFixture _globalFixture;

    /// <summary>Единый таймаут ожидания элементов (мс) из конфигурации.</summary>
    protected static int DefaultTimeout => GlobalFixture.TestOptions.TimeoutMs;

    public BrowserFixture(GlobalFixture globalFixture)
    {
        _globalFixture = globalFixture;
    }

    /// <summary>Создать страницу по базе URL (для редких случаев).</summary>
    public async Task<IPage> CreatePageAsync(string? urlBase = null)
    {
        var page = await _globalFixture.Browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });
        if (!string.IsNullOrEmpty(urlBase))
            await page.GotoAsync(urlBase);
        return page;
    }

    /// <summary>Создать страницу портала с указанием пути.</summary>
    public async Task<IPage> CreatePageAsync(Portal portal, string path = "/") =>
        await CreatePageAsync(PortalUrls.GetUrl(portal, path));

    /// <summary>Короткая алиас-перегрузка: создать страницу Board Portal.</summary>
    public Task<IPage> CreateBoardPortalPageAsync(string path = "/") =>
        CreatePageAsync(Portal.BoardPortal, path);

    /// <summary>Короткая алиас-перегрузка: создать страницу Admin Console.</summary>
    public Task<IPage> CreateAdminConsolePageAsync(string path = "/") =>
        CreatePageAsync(Portal.AdminConsole, path);

    /// <summary>Проверить, не было ли ошибки в предыдущих тестах. Если да — пропустить текущий.</summary>
    protected static void SkipIfPreviousFailed()
    {
        if (GlobalFixture.HasFailed)
            Assert.Skip("Предыдущий тест завершился с ошибкой — пропуск");
    }

    /// <summary>
    /// Убедиться, что элемент найден на странице. Если не найден — выбросить исключение с описанием.
    /// </summary>
    protected static async Task<ILocator> RequireLocatorAsync(IPage page, string selector, string fieldName)
    {
        var locator = page.Locator(selector);
        var count = await locator.CountAsync();
        if (count == 0)
            throw new InvalidOperationException($"Не найден элемент «{fieldName}» (селектор: {selector})");
        return locator;
    }

    /// <summary>
    /// Убедиться, что элемент с data-testid найден. Если не найден — исключение.
    /// </summary>
    protected static async Task<ILocator> RequireByTestIdAsync(IPage page, string testId, string fieldName)
    {
        var locator = page.GetByTestId(testId);
        var count = await locator.CountAsync();
        if (count == 0)
            throw new InvalidOperationException($"Не найден элемент «{fieldName}» (data-testid={testId})");
        return locator;
    }
}
