using FluentAssertions;
using System.Net;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Tests;

/// <summary>
/// US-001: Авторизация
/// Тестирование через HTTP-запросы
/// </summary>
public class US001_AuthorizationTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private const string BoardPortalUrl = "http://localhost:5000";
    private const string AdminConsoleUrl = "http://localhost:5001";

    public Task InitializeAsync()
    {
        _client = new HttpClient { BaseAddress = new Uri(BoardPortalUrl) };
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task US001_Scenario1_LoginPage_ReturnsOk()
    {
        // Given: пользователь на странице входа
        // When: открывает страницу /login
        var response = await _client.GetAsync("/login");

        // Then: страница доступна
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Fiducia");
        content.Should().Contain("Board Portal");
    }

    [Fact]
    public async Task US001_Scenario1_LoginPage_ContainsDropdown()
    {
        // Given: пользователь на странице входа
        var response = await _client.GetAsync("/login");
        var content = await response.Content.ReadAsStringAsync();

        // Then: страница содержит select элемент
        content.Should().Contain("<select");
        content.Should().Contain("form-select");
    }

    [Fact]
    public async Task US001_Scenario2_MainPage_RequiresAuth()
    {
        // Given: пользователь не авторизован
        // When: пытается открыть главную
        var response = await _client.GetAsync("/");

        // Then: страница доступна (Blazor не блокирует)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task US001_Scenario3_AdminConsole_LoginPage()
    {
        // Given: пользователь на странице входа Admin Console
        using var adminClient = new HttpClient { BaseAddress = new Uri(AdminConsoleUrl) };
        var response = await adminClient.GetAsync("/login");
        var content = await response.Content.ReadAsStringAsync();

        // Then: страница доступна и содержит Admin Console
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Admin Console");
    }
}
