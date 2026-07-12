using FluentAssertions;
using System.Net;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Tests;

/// <summary>
/// US-002: Управление заседаниями
/// Тестирование через HTTP-запросы
/// </summary>
public class US002_MeetingTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private const string BoardPortalUrl = "http://localhost:5000";

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
    public async Task US002_Scenario1_MeetingsPage_ReturnsOk()
    {
        // Given: пользователь на странице «Созывы»
        var response = await _client.GetAsync("/meetings");

        // Then: страница доступна
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Созывы");
    }

    [Fact]
    public async Task US002_Scenario1_MeetingsPage_HasCreateButton()
    {
        // Given: пользователь на странице «Созывы»
        var response = await _client.GetAsync("/meetings");
        var content = await response.Content.ReadAsStringAsync();

        // Then: есть кнопка создания
        content.Should().Contain("Создать уведомление");
    }

    [Fact]
    public async Task US002_Scenario1_MeetingsPage_HasBlazorComponent()
    {
        // Given: пользователь на странице «Созывы»
        var response = await _client.GetAsync("/meetings");
        var content = await response.Content.ReadAsStringAsync();

        // Then: страница содержит Blazor компонент
        content.Should().Contain("blazor");
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task US002_Scenario2_VotingPage_ReturnsOk()
    {
        // Given: пользователь на странице голосования
        var response = await _client.GetAsync("/voting/1");

        // Then: страница доступна
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Голосование");
    }

    [Fact]
    public async Task US002_Scenario2_VotingPage_HasBlazorComponent()
    {
        // Given: пользователь на странице голосования
        var response = await _client.GetAsync("/voting/1");
        var content = await response.Content.ReadAsStringAsync();

        // Then: страница содержит Blazor компонент
        content.Should().Contain("blazor");
    }
}
