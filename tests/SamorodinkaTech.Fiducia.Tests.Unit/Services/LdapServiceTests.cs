using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Models.Ldap;
using SamorodinkaTech.Fiducia.Tests.Unit.Mocks;
using static SamorodinkaTech.Fiducia.Tests.Unit.Services.TestHelper;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Unit-тесты LDAP-сервиса с использованием MockLdapService.
/// Проверяют контракт ILdapService: аутентификацию, поиск пользователей,
/// фильтрацию по отделу, членство в группах и обработку сбоев.
/// </summary>
public class LdapServiceTests
{
    private readonly MockLdapService _ldap = new();

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ShouldReturnTrue()
    {
        _ldap.AddUser(Director("ivanov", "Иванов И.И."), "pass123");

        var result = await _ldap.AuthenticateAsync("ivanov", "pass123");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AuthenticateAsync_WrongPassword_ShouldReturnFalse()
    {
        _ldap.AddUser(Director("ivanov", "Иванов И.И."), "pass123");

        var result = await _ldap.AuthenticateAsync("ivanov", "wrong");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AuthenticateAsync_UnknownUser_ShouldReturnFalse()
    {
        var result = await _ldap.AuthenticateAsync("ghost", "password");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task FindUserByLoginAsync_Existing_ShouldReturnUser()
    {
        _ldap.AddUser(Director("ivanov", "Иванов И.И.", "ivanov@company.ru", "Член СД"), "pass");

        var user = await _ldap.FindUserByLoginAsync("ivanov");

        user.Should().NotBeNull();
        user!.LoginName.Should().Be("ivanov");
        user.DisplayName.Should().Be("Иванов И.И.");
        user.Email.Should().Be("ivanov@company.ru");
        user.Title.Should().Be("Член СД");
    }

    [Fact]
    public async Task FindUserByLoginAsync_NonExisting_ShouldReturnNull()
    {
        var user = await _ldap.FindUserByLoginAsync("nonexistent");

        user.Should().BeNull();
    }

    [Fact]
    public async Task SearchUsersAsync_ByDepartment_ShouldFilter()
    {
        _ldap.AddUser(Director("ivanov", "Иванов И.И.", dept: "Совет директоров"), "p1");
        _ldap.AddUser(Director("petrov", "Петров П.П.", dept: "Совет директоров"), "p2");
        _ldap.AddUser(Director("sidorov", "Сидоров С.С.", dept: "Бухгалтерия"), "p3");

        var results = await _ldap.SearchUsersAsync("(department=Совет директоров)");

        results.Should().HaveCount(2);
        results.Should().Contain(u => u.LoginName == "ivanov");
        results.Should().Contain(u => u.LoginName == "petrov");
        results.Should().NotContain(u => u.LoginName == "sidorov");
    }

    [Fact]
    public async Task GetGroupMembersAsync_ShouldReturnMembers()
    {
        var ivanov = Director("ivanov", "Иванов И.И.");
        var petrov = Director("petrov", "Петров П.П.");
        var sidorov = Director("sidorov", "Сидоров С.С.");

        _ldap.AddUser(ivanov, "p1");
        _ldap.AddUser(petrov, "p2");
        _ldap.AddUser(sidorov, "p3");

        var boardDn = "CN=BoardOfDirectors,OU=Groups,DC=company,DC=local";
        _ldap.AddToGroup(boardDn, ivanov);
        _ldap.AddToGroup(boardDn, petrov);

        var members = await _ldap.GetGroupMembersAsync(boardDn);

        members.Should().HaveCount(2);
        members.Should().Contain(u => u.LoginName == "ivanov");
        members.Should().Contain(u => u.LoginName == "petrov");
        members.Should().NotContain(u => u.LoginName == "sidorov");
    }

    [Fact]
    public async Task GetGroupMembersAsync_EmptyGroup_ShouldReturnEmpty()
    {
        var members = await _ldap.GetGroupMembersAsync(
            "CN=Empty,OU=Groups,DC=company,DC=local");

        members.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchUsersAsync_MemberOfFilter_ShouldReturnGroupMembers()
    {
        var ivanov = Director("ivanov", "Иванов И.И.");
        var petrov = Director("petrov", "Петров П.П.");

        _ldap.AddUser(ivanov, "p1");
        _ldap.AddUser(petrov, "p2");

        var boardDn = "CN=BoardOfDirectors,DC=company,DC=local";
        _ldap.AddToGroup(boardDn, ivanov);
        _ldap.AddToGroup(boardDn, petrov);

        var results = await _ldap.SearchUsersAsync(
            $"(&(objectClass=user)(memberOf={boardDn}))");

        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task LdapUser_ShouldHaveAllFields()
    {
        _ldap.AddUser(new LdapUser
        {
            DistinguishedName = "CN=Иванов,OU=Users,DC=company,DC=local",
            LoginName = "ivanov",
            DisplayName = "Иванов Иван Иванович",
            Email = "ivanov@company.ru",
            Title = "Председатель СД",
            Department = "Совет директоров",
            Phone = "+7-999-123-45-67",
            MemberOf = new[] { "CN=BoardOfDirectors,DC=company,DC=local" }
        }, "pass");

        var user = await _ldap.FindUserByLoginAsync("ivanov");

        user.Should().NotBeNull();
        user!.DistinguishedName.Should().Contain("CN=Иванов");
        user.LoginName.Should().Be("ivanov");
        user.DisplayName.Should().Be("Иванов Иван Иванович");
        user.Email.Should().Be("ivanov@company.ru");
        user.Title.Should().Be("Председатель СД");
        user.Department.Should().Be("Совет директоров");
        user.Phone.Should().Be("+7-999-123-45-67");
        user.MemberOf.Should().HaveCount(1);
        user.MemberOf[0].Should().Contain("BoardOfDirectors");
    }

    [Fact]
    public async Task SimulateFailure_ShouldThrowOnAllOperations()
    {
        _ldap.SimulateFailure = true;

        var authAct = () => _ldap.AuthenticateAsync("u", "p");
        await authAct.Should().ThrowAsync<InvalidOperationException>();

        var findAct = () => _ldap.FindUserByLoginAsync("u");
        await findAct.Should().ThrowAsync<InvalidOperationException>();

        var searchAct = () => _ldap.SearchUsersAsync("(objectClass=*)");
        await searchAct.Should().ThrowAsync<InvalidOperationException>();

        var groupAct = () => _ldap.GetGroupMembersAsync("CN=X");
        await groupAct.Should().ThrowAsync<InvalidOperationException>();
    }
}

/// <summary>
/// Helper для создания тестовых пользователей.
/// </summary>
internal static class TestHelper
{
    public static LdapUser Director(
        string login,
        string displayName,
        string? email = null,
        string? title = null,
        string? dept = null)
    {
        return new LdapUser
        {
            DistinguishedName = $"CN={displayName},OU=Users,DC=company,DC=local",
            LoginName = login,
            DisplayName = displayName,
            Email = email ?? $"{login}@company.ru",
            Title = title,
            Department = dept
        };
    }
}
