using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Базовый класс — поднимает/останавливает Playwright браузер.
/// </summary>
public class BrowserFixture : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    /// <summary>Создать страницу по базе URL (для редких случаев).</summary>
    protected async Task<IPage> CreatePageAsync(string? urlBase = null)
    {
        var page = await _browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });
        if (!string.IsNullOrEmpty(urlBase))
            await page.GotoAsync(urlBase);
        return page;
    }

    /// <summary>Создать страницу портала с указанием пути.</summary>
    protected async Task<IPage> CreatePageAsync(Portal portal, string path = "/") =>
        await CreatePageAsync(PortalUrls.GetUrl(portal, path));

    /// <summary>Короткая алиас-перегрузка: создать страницу Board Portal.</summary>
    protected Task<IPage> CreateBoardPortalPageAsync(string path = "/") =>
        CreatePageAsync(Portal.BoardPortal, path);

    /// <summary>Короткая алиас-перегрузка: создать страницу Admin Console.</summary>
    protected Task<IPage> CreateAdminConsolePageAsync(string path = "/") =>
        CreatePageAsync(Portal.AdminConsole, path);
}
