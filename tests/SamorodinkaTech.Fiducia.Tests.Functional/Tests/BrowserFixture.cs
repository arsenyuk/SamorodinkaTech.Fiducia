using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Базовый класс — предоставляет доступ к браузеру из GlobalFixture.
/// НЕ создаёт свой экземпляр браузера — использует общий из GlobalFixture.
/// </summary>
public class BrowserFixture
{
    private readonly GlobalFixture _globalFixture;

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
}
