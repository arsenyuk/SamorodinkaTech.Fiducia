using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Models.Ldap;
using SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Unit-тесты сервиса синхронизации СД из LDAP.
/// Проверяют получение кандидатов, маппинг типов директоров,
/// контроль уникальности логинов и обработку сбоев.
/// </summary>
public class BoardMemberLdapServiceTests
{
    private readonly MockBoardMemberLdapService _service = new();

    /// <summary>
    /// GetAllCandidatesAsync возвращает всех добавленных кандидатов.
    /// </summary>
    [Fact]
    public async Task GetCandidatesAsync_ShouldReturnAllAddedCandidates()
    {
        _service.AddCandidate(Candidate("i.ivanov", "Иванов И.И.", "Председатель Совета директоров", "EXECUTIVE"));
        _service.AddCandidate(Candidate("p.petrov", "Петров П.П.", "Независимый директор", "INDEPENDENT"));
        _service.AddCandidate(Candidate("s.sidorov", "Сидоров С.С.", "Неисполнительный директор", "NON_EXECUTIVE"));

        var candidates = await _service.GetCandidatesAsync();

        candidates.Should().HaveCount(3);
        candidates.Should().Contain(c => c.Login == "i.ivanov");
        candidates.Should().Contain(c => c.Login == "p.petrov");
        candidates.Should().Contain(c => c.Login == "s.sidorov");
    }

    /// <summary>
    /// При пустом списке кандидатов возвращается пустая коллекция.
    /// </summary>
    [Fact]
    public async Task GetCandidatesAsync_Empty_ShouldReturnEmptyList()
    {
        var candidates = await _service.GetCandidatesAsync();

        candidates.Should().BeEmpty();
    }

    /// <summary>
    /// Поиск кандидата по логину: существующий логин возвращает кандидата со всеми полями.
    /// </summary>
    [Fact]
    public async Task GetCandidateByLoginAsync_Existing_ShouldReturnCandidate()
    {
        _service.AddCandidate(Candidate("i.ivanov", "Иванов И.И.", "Председатель", "EXECUTIVE"));

        var candidate = await _service.GetCandidateByLoginAsync("i.ivanov");

        candidate.Should().NotBeNull();
        candidate!.FullName.Should().Be("Иванов И.И.");
        candidate.Login.Should().Be("i.ivanov");
        candidate.Title.Should().Be("Председатель");
        candidate.SuggestedMemberTypeCode.Should().Be("EXECUTIVE");
    }

    /// <summary>
    /// Поиск кандидата по несуществующему логину возвращает null.
    /// </summary>
    [Fact]
    public async Task GetCandidateByLoginAsync_NonExisting_ShouldReturnNull()
    {
        var candidate = await _service.GetCandidateByLoginAsync("ghost");

        candidate.Should().BeNull();
    }

    /// <summary>
    /// Проверка уникальности: новый логин не считается дубликатом.
    /// </summary>
    [Fact]
    public void IsDuplicate_NewLogin_ShouldReturnFalse()
    {
        var existing = new HashSet<string> { "i.ivanov", "p.petrov" };

        var result = _service.IsDuplicate(existing, "s.sidorov");

        result.Should().BeFalse();
    }

    /// <summary>
    /// Проверка уникальности: уже присутствующий логин считается дубликатом.
    /// </summary>
    [Fact]
    public void IsDuplicate_ExistingLogin_ShouldReturnTrue()
    {
        var existing = new HashSet<string> { "i.ivanov", "p.petrov" };

        var result = _service.IsDuplicate(existing, "i.ivanov");

        result.Should().BeTrue();
    }

    /// <summary>
    /// Попытка назначить одного кандидата дважды блокируется проверкой IsDuplicate.
    /// </summary>
    [Fact]
    public async Task IsDuplicate_PreventsDoubleAssignment()
    {
        _service.AddCandidate(Candidate("i.ivanov", "Иванов И.И.", "Председатель", "EXECUTIVE"));

        var candidate = await _service.GetCandidateByLoginAsync("i.ivanov");

        // Симулируем: Иванов уже назначен председателем
        var assignedMembers = new HashSet<string> { candidate!.Login };

        // Попытка назначить Иванова ещё и как независимого директора
        var isDuplicate = _service.IsDuplicate(assignedMembers, "i.ivanov");

        isDuplicate.Should().BeTrue();
    }

    /// <summary>
    /// FindDuplicates: при отсутствии дубликатов возвращается пустой список.
    /// </summary>
    [Fact]
    public void FindDuplicates_NoDuplicates_ShouldReturnEmpty()
    {
        var logins = new List<string> { "i.ivanov", "p.petrov", "s.sidorov" };

        var duplicates = _service.FindDuplicates(logins);

        duplicates.Should().BeEmpty();
    }

    /// <summary>
    /// FindDuplicates: при наличии одного дубликата — он обнаруживается.
    /// </summary>
    [Fact]
    public void FindDuplicates_WithDuplicates_ShouldDetectThem()
    {
        var logins = new List<string> { "i.ivanov", "p.petrov", "i.ivanov", "s.sidorov" };

        var duplicates = _service.FindDuplicates(logins);

        duplicates.Should().HaveCount(1);
        duplicates[0].Should().Be("i.ivanov");
    }

    /// <summary>
    /// FindDuplicates: при нескольких дубликатах — все обнаруживаются.
    /// </summary>
    [Fact]
    public void FindDuplicates_MultipleDuplicates_ShouldDetectAll()
    {
        var logins = new List<string> { "i.ivanov", "p.petrov", "i.ivanov", "p.petrov", "s.sidorov" };

        var duplicates = _service.FindDuplicates(logins);

        duplicates.Should().HaveCount(2);
        duplicates.Should().Contain("i.ivanov");
        duplicates.Should().Contain("p.petrov");
    }

    /// <summary>
    /// Кандидат должен нести все поля: логин, ФИО, email, должность, телефон, DN, тип.
    /// </summary>
    [Fact]
    public async Task Candidates_ShouldCarryAllFields()
    {
        _service.AddCandidate(new BoardMemberCandidate
        {
            Login = "a.smirnova",
            FullName = "Смирнова А.В.",
            Email = "a.smirnova@bryansk-arsenal.ru",
            Title = "Член Совета директоров",
            Phone = "+7-999-500-50-05",
            DistinguishedName = "cn=Смирнова Анна Владимировна,ou=Users,dc=bryansk-arsenal,dc=local",
            SuggestedMemberTypeCode = "STAFF"
        });

        var candidates = await _service.GetCandidatesAsync();
        var c = candidates[0];

        c.Login.Should().Be("a.smirnova");
        c.FullName.Should().Be("Смирнова А.В.");
        c.Email.Should().Be("a.smirnova@bryansk-arsenal.ru");
        c.Title.Should().Be("Член Совета директоров");
        c.Phone.Should().Be("+7-999-500-50-05");
        c.DistinguishedName.Should().Contain("Смирнова");
        c.SuggestedMemberTypeCode.Should().Be("STAFF");
    }

    /// <summary>
    /// При включённой симуляции сбоя все асинхронные операции выбрасывают исключение.
    /// </summary>
    [Fact]
    public async Task SimulateFailure_ShouldThrowOnAsyncOperations()
    {
        _service.SimulateFailure = true;

        var getCandidatesAct = () => _service.GetCandidatesAsync();
        await getCandidatesAct.Should().ThrowAsync<InvalidOperationException>();

        var getByLoginAct = () => _service.GetCandidateByLoginAsync("x");
        await getByLoginAct.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Создаёт тестового кандидата с заданными параметрами.
    /// </summary>
    private static BoardMemberCandidate Candidate(
        string login,
        string fullName,
        string title,
        string typeCode)
    {
        return new BoardMemberCandidate
        {
            Login = login,
            FullName = fullName,
            Email = $"{login}@company.ru",
            Title = title,
            DistinguishedName = $"cn={fullName},ou=Users,dc=company,dc=local",
            SuggestedMemberTypeCode = typeCode
        };
    }
}
