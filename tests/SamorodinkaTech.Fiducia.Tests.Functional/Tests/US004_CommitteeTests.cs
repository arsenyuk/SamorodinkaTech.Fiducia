using FluentAssertions;
using System.Net;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Tests;

/// <summary>
/// US-004: Управление комитетами
/// Тестирование через HTTP-запросы
/// </summary>
public class US004_CommitteeTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private const string AdminConsoleUrl = "http://localhost:5001";

    public Task InitializeAsync()
    {
        _client = new HttpClient { BaseAddress = new Uri(AdminConsoleUrl) };
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task US004_Scenario1_CommitteesPage_ReturnsOk()
    {
        // Given: администратор на странице комитетов
        var response = await _client.GetAsync("/committees");

        // Then: страница доступна
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Комитеты");
    }

    [Fact]
    public async Task US004_Scenario1_CommitteesPage_HasCreateButton()
    {
        // Given: администратор на странице комитетов
        var response = await _client.GetAsync("/committees");
        var content = await response.Content.ReadAsStringAsync();

        // Then: есть кнопка создания
        content.Should().Contain("Создать комитет");
    }

    [Fact]
    public async Task US004_Scenario1_CommitteesPage_HasBlazorComponent()
    {
        // Given: администратор на странице комитетов
        var response = await _client.GetAsync("/committees");
        var content = await response.Content.ReadAsStringAsync();

        // Then: страница содержит Blazor компонент
        content.Should().Contain("blazor");
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task US004_Scenario2_SettingsPage_HasAuthMethod()
    {
        // Given: администратор на странице настроек
        var response = await _client.GetAsync("/settings");
        var content = await response.Content.ReadAsStringAsync();

        // Then: есть настройки метода авторизации
        content.Should().Contain("Метод авторизации");
        content.Should().Contain("Basic");
        content.Should().Contain("Active Directory");
    }

    [Fact]
    public async Task US004_Scenario2_SettingsPage_HasGostSettings()
    {
        // Given: администратор на странице настроек
        var response = await _client.GetAsync("/settings");
        var content = await response.Content.ReadAsStringAsync();

        // Then: есть настройки печати по ГОСТу
        content.Should().Contain("ГОСТ");
        content.Should().Contain("Левое поле");
        content.Should().Contain("Шрифт");
    }

    [Fact]
    public async Task US004_Scenario2_SettingsPage_HasIntegrations()
    {
        // Given: администратор на странице настроек
        var response = await _client.GetAsync("/settings");
        var content = await response.Content.ReadAsStringAsync();

        // Then: есть настройки интеграций
        content.Should().Contain("Интеграции");
        content.Should().Contain("КриптоПро");
        content.Should().Contain("SMTP");
        content.Should().Contain("SMS");
    }
}
