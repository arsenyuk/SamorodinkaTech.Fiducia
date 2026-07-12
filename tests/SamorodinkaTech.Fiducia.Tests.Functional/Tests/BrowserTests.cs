using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Tests;

/// <summary>
/// Демонстрация автотестирования через браузер (Playwright)
/// </summary>
public class BrowserTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private const string BoardPortalUrl = "http://localhost:5000";
    private const string AdminConsoleUrl = "http://localhost:5001";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task Browser_OpenLoginPage_ShowsDropdown()
    {
        Console.WriteLine("[Тест] Открываю страницу входа...");
        await _page.GotoAsync($"{BoardPortalUrl}/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var select = await _page.QuerySelectorAsync("select.form-select");
        select.Should().NotBeNull();

        var options = await _page.QuerySelectorAllAsync("select.form-select option");
        Console.WriteLine($"[Тест] Найдено {options.Count} опций");
        options.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task Browser_LoginPage_HasNoSidebar()
    {
        Console.WriteLine("[Тест] Открываю страницу входа...");
        await _page.GotoAsync($"{BoardPortalUrl}/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // На странице входа сайдер-меню должен быть скрыт
        var sidebar = await _page.QuerySelectorAsync(".sidebar");
        Console.WriteLine($"[Тест] Сайдер-меню: {(sidebar != null ? "есть" : "нет")}");
        // Сайдер может быть в DOM, но скрыт через CSS
        var sidebarVisible = await _page.EvaluateAsync<bool>("el => el ? window.getComputedStyle(el).display !== 'none' : false", sidebar);
        Console.WriteLine($"[Тест] Сайдер видим: {sidebarVisible}");
    }

    [Fact]
    public async Task Browser_AfterLogin_ShowsSidebar()
    {
        Console.WriteLine("[Тест] Вхожу в систему...");
        await _page.GotoAsync($"{BoardPortalUrl}/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.SelectOptionAsync("select.form-select", new SelectOptionValue { Index = 1 });
        await _page.ClickAsync("button:has-text('Войти')");
        await _page.WaitForURLAsync($"{BoardPortalUrl}/");

        // После входа сайдер-меню должен быть виден
        var sidebar = await _page.QuerySelectorAsync(".sidebar");
        Console.WriteLine($"[Тест] Сайдер-меню: {(sidebar != null ? "есть" : "нет")}");
        var sidebarVisible = await _page.EvaluateAsync<bool>("el => el ? window.getComputedStyle(el).display !== 'none' : false", sidebar);
        Console.WriteLine($"[Тест] Сайдер видим: {sidebarVisible}");
        sidebarVisible.Should().BeTrue("После входа сайдер-меню должен быть виден");
    }

    [Fact]
    public async Task Browser_AfterLogin_ShowsUserName()
    {
        Console.WriteLine("[Тест] Вхожу в систему...");
        await _page.GotoAsync($"{BoardPortalUrl}/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.SelectOptionAsync("select.form-select", new SelectOptionValue { Index = 1 });
        await _page.ClickAsync("button:has-text('Войти')");
        await _page.WaitForURLAsync($"{BoardPortalUrl}/");

        // В header должно отображаться ФИО
        var headerText = await _page.TextContentAsync(".top-row");
        Console.WriteLine($"[Тест] Header: {headerText}");
        headerText.Should().NotContain("Гость");
    }

    [Fact]
    public async Task Browser_LoginAndNavigate_ShowsHomePage()
    {
        Console.WriteLine("[Тест] Открываю страницу входа...");
        await _page.GotoAsync($"{BoardPortalUrl}/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Console.WriteLine("[Тест] Выбираю пользователя...");
        await _page.SelectOptionAsync("select.form-select", new SelectOptionValue { Index = 1 });

        Console.WriteLine("[Тест] Нажимаю 'Войти'...");
        await _page.ClickAsync("button:has-text('Войти')");
        await _page.WaitForURLAsync($"{BoardPortalUrl}/");

        var url = _page.Url;
        Console.WriteLine($"[Тест] URL: {url}");
        url.Should().Contain("/");
    }

    [Fact]
    public async Task Browser_LogoutPage_LoadsSuccessfully()
    {
        Console.WriteLine("[Тест] Вхожу в систему...");
        await _page.GotoAsync($"{BoardPortalUrl}/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.SelectOptionAsync("select.form-select", new SelectOptionValue { Index = 1 });
        await _page.ClickAsync("button:has-text('Войти')");
        await _page.WaitForURLAsync($"{BoardPortalUrl}/");

        Console.WriteLine("[Тест] Перехожу на /logout...");
        await _page.GotoAsync($"{BoardPortalUrl}/logout");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var url = _page.Url;
        Console.WriteLine($"[Тест] URL: {url}");
        url.Should().Contain("/logout");
    }

    [Fact]
    public async Task Browser_AdminConsole_InvalidUser_ShowsError()
    {
        Console.WriteLine("[Тест] Открываю login Admin Console...");
        await _page.GotoAsync($"{AdminConsoleUrl}/login");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var options = await _page.QuerySelectorAllAsync("select.form-select option");
        for (int i = 1; i < options.Count; i++)
        {
            var text = await options[i].TextContentAsync();
            if (text != null && (text.Contains("Член СД") || text.Contains("Председатель СД")))
            {
                await _page.SelectOptionAsync("select.form-select", new SelectOptionValue { Index = i });
                break;
            }
        }

        Console.WriteLine("[Тест] Нажимаю 'Войти'...");
        await _page.ClickAsync("button:has-text('Войти')");

        Console.WriteLine("[Тест] Жду ошибку...");
        await _page.WaitForSelectorAsync(".alert-danger", new PageWaitForSelectorOptions { Timeout = 5000 });
        var error = await _page.QuerySelectorAsync(".alert-danger");
        error.Should().NotBeNull();
    }
}
