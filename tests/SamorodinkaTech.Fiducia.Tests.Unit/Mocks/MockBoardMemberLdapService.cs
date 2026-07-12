using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Ldap;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

/// <summary>
/// Mock-реализация IBoardMemberLdapService для unit-тестирования.
/// </summary>
public class MockBoardMemberLdapService : IBoardMemberLdapService
{
    private readonly List<BoardMemberCandidate> _candidates = new();
    private bool _simulateFailure;

    public bool SimulateFailure
    {
        get => _simulateFailure;
        set
        {
            _simulateFailure = value;
            if (value)
                _candidates.Clear();
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<BoardMemberCandidate>> GetCandidatesAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();
        return Task.FromResult<IReadOnlyList<BoardMemberCandidate>>(_candidates.ToList());
    }

    /// <inheritdoc />
    public Task<BoardMemberCandidate?> GetCandidateByLoginAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();
        return Task.FromResult(
            _candidates.FirstOrDefault(c =>
                string.Equals(c.Login, login, StringComparison.OrdinalIgnoreCase)));
    }

    /// <inheritdoc />
    public bool IsDuplicate(IReadOnlySet<string> existingLogins, string newLogin)
    {
        return existingLogins.Contains(newLogin);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> FindDuplicates(IReadOnlyList<string> logins)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var duplicates = new List<string>();

        foreach (var login in logins)
        {
            if (!seen.Add(login))
                duplicates.Add(login);
        }

        return duplicates;
    }

    /// <summary>
    /// Добавляет кандидата в mock-хранилище.
    /// </summary>
    public void AddCandidate(BoardMemberCandidate candidate)
    {
        _candidates.Add(candidate);
    }

    private void ThrowIfFailure()
    {
        if (_simulateFailure)
            throw new InvalidOperationException("Simulated LDAP sync failure");
    }
}
