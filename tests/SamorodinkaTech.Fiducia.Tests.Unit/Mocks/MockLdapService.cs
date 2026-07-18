using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Ldap;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

/// <summary>
/// Mock-реализация ILdapService для unit-тестирования.
/// Хранит пользователей и группы в памяти, имитирует поведение LDAP-каталога.
/// </summary>
public class MockLdapService : ILdapService
{
    private readonly Dictionary<string, LdapUser> _users = new();
    private readonly Dictionary<string, string> _passwords = new();
    private readonly Dictionary<string, List<LdapUser>> _groups = new();

    /// <summary>Если true, все методы выбрасывают исключение (имитация отказа LDAP).</summary>
    public bool SimulateFailure { get; set; }

    /// <inheritdoc />
    public Task<bool> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        if (!_passwords.TryGetValue(login, out var storedPassword))
            return Task.FromResult(false);

        return Task.FromResult(storedPassword == password);
    }

    /// <inheritdoc />
    public Task<LdapUser?> FindUserByLoginAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        _users.TryGetValue(login, out var user);
        return Task.FromResult(user);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LdapUser>> SearchUsersAsync(
        string filter,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        var results = new List<LdapUser>();

        if (filter.Contains("department="))
        {
            var dept = ExtractValue(filter, "department");
            results.AddRange(_users.Values.Where(u =>
                string.Equals(u.Department, dept, StringComparison.OrdinalIgnoreCase)));
        }
        else if (filter.Contains("memberOf="))
        {
            var groupDn = ExtractValue(filter, "memberOf");
            if (_groups.TryGetValue(groupDn, out var members))
                results.AddRange(members);
        }
        else
        {
            results.AddRange(_users.Values);
        }

        return Task.FromResult<IReadOnlyList<LdapUser>>(results);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LdapUser>> GetGroupMembersAsync(
        string groupDn,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        if (_groups.TryGetValue(groupDn, out var members))
            return Task.FromResult<IReadOnlyList<LdapUser>>(members.ToList());

        return Task.FromResult<IReadOnlyList<LdapUser>>(Array.Empty<LdapUser>());
    }

    /// <summary>
    /// Добавляет пользователя в mock-каталог с паролем.
    /// </summary>
    public void AddUser(LdapUser user, string password)
    {
        _users[user.LoginName] = user;
        _passwords[user.LoginName] = password;
    }

    /// <summary>
    /// Добавляет пользователя в группу mock-каталога.
    /// </summary>
    public void AddToGroup(string groupDn, LdapUser user)
    {
        if (!_groups.ContainsKey(groupDn))
            _groups[groupDn] = new List<LdapUser>();

        _groups[groupDn].Add(user);
    }

    private static string? ExtractValue(string filter, string attr)
    {
        var start = filter.IndexOf($"{attr}=", StringComparison.Ordinal);
        if (start < 0) return null;

        start += attr.Length + 1;
        var end = filter.IndexOf(')', start);
        if (end < 0) return null;

        return filter[start..end];
    }

    private void ThrowIfFailure()
    {
        if (SimulateFailure)
            throw new InvalidOperationException("Simulated LDAP server failure");
    }
}
