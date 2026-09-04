using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Edin;
using SamorodinkaTech.Fiducia.Infrastructure.Services;
using SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Unit-тесты EdinBindingService: привязка MPI MasterId к участнику и поиск УЗ.
/// </summary>
public class EdinBindingServiceTests
{
    private readonly MockEdinApiClient _edinClient = new();
    private readonly Mock<IApplicationDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<EdinBindingService>> _loggerMock = new();
    private readonly EdinBindingService _sut;

    private readonly List<User> _users = new();
    private readonly List<EcosystemParticipant> _participants = new();

    public EdinBindingServiceTests()
    {
        SetupDbSets();
        _sut = new EdinBindingService(_edinClient, _dbContextMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Успешный resolve: MasterId привязывается к участнику, УЗ найдена в БД → user_id установлен.
    /// </summary>
    [Fact]
    public async Task ResolveAndBindAsync_WhenMasterIdFoundInDb_ShouldLinkUser()
    {
        var masterId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), MpiMasterId = masterId, Login = "ivanov" };
        _users.Add(user);

        var participant = CreateParticipant();
        _participants.Add(participant);

        _edinClient.ResolveResult = new EdinPersonResult
        {
            MasterId = masterId,
            Status = "Matched"
        };

        var result = await _sut.ResolveAndBindAsync(
            participant.Id, "Иванов", "Иван", "Иванович",
            "770123456789", null, null, null, null);

        result.Success.Should().BeTrue();
        result.MpiMasterId.Should().Be(masterId);
        result.LinkedUserId.Should().Be(user.Id);
        result.UserSource.Should().Be("db");

        participant.MpiMasterId.Should().Be(masterId);
        participant.UserId.Should().Be(user.Id);
    }

    /// <summary>
    /// Resolve возвращает null (сервис недоступен) → ошибка.
    /// </summary>
    [Fact]
    public async Task ResolveAndBindAsync_WhenServiceUnavailable_ShouldReturnError()
    {
        _edinClient.SimulateUnavailable = true;

        var participant = CreateParticipant();
        _participants.Add(participant);

        var result = await _sut.ResolveAndBindAsync(
            participant.Id, "Иванов", "Иван", null,
            null, null, null, null, null);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("недоступен");
        result.MpiMasterId.Should().BeNull();
    }

    /// <summary>
    /// Resolve возвращает Ambiguous (MasterId null) → ошибка с описанием.
    /// </summary>
    [Fact]
    public async Task ResolveAndBindAsync_WhenAmbiguous_ShouldReturnErrorWithStatus()
    {
        _edinClient.ResolveResult = new EdinPersonResult
        {
            MasterId = null,
            Status = "Ambiguous",
            HasDefects = true,
            Defects = ["Конфликт ИНН"]
        };

        var participant = CreateParticipant();
        _participants.Add(participant);

        var result = await _sut.ResolveAndBindAsync(
            participant.Id, "Иванов", "Иван", null,
            "770123456789", null, null, null, null);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Ambiguous");
        result.Error.Should().Contain("Конфликт ИНН");
    }

    /// <summary>
    /// Участник не найден в БД → ошибка.
    /// </summary>
    [Fact]
    public async Task ResolveAndBindAsync_WhenParticipantNotFound_ShouldReturnError()
    {
        var masterId = Guid.NewGuid();
        _edinClient.ResolveResult = new EdinPersonResult { MasterId = masterId, Status = "Matched" };

        var result = await _sut.ResolveAndBindAsync(
            Guid.NewGuid(), "Иванов", "Иван", null,
            null, null, null, null, null);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("не найден");
    }

    /// <summary>
    /// MasterId уже привязан к участнику и user_id есть → идемпотентный возврат.
    /// </summary>
    [Fact]
    public async Task ResolveAndBindAsync_WhenAlreadyBound_ShouldReturnIdempotent()
    {
        var masterId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var participant = CreateParticipant();
        participant.MpiMasterId = masterId;
        participant.UserId = userId;
        _participants.Add(participant);

        _edinClient.ResolveResult = new EdinPersonResult { MasterId = masterId, Status = "Matched" };

        var result = await _sut.ResolveAndBindAsync(
            participant.Id, "Иванов", "Иван", null,
            null, null, null, null, null);

        result.Success.Should().BeTrue();
        result.MpiMasterId.Should().Be(masterId);
        result.LinkedUserId.Should().Be(userId);
    }

    /// <summary>
    /// УЗ не найдена в БД → MasterId привязывается, но user_id остаётся null.
    /// </summary>
    [Fact]
    public async Task ResolveAndBindAsync_WhenUserNotInDb_ShouldBindWithoutUser()
    {
        var masterId = Guid.NewGuid();
        _edinClient.ResolveResult = new EdinPersonResult { MasterId = masterId, Status = "Matched" };

        var participant = CreateParticipant();
        _participants.Add(participant);

        var result = await _sut.ResolveAndBindAsync(
            participant.Id, "Иванов", "Иван", null,
            null, null, null, null, null);

        result.Success.Should().BeTrue();
        result.MpiMasterId.Should().Be(masterId);
        result.LinkedUserId.Should().BeNull();
        result.UserSource.Should().BeNull();

        participant.MpiMasterId.Should().Be(masterId);
        participant.UserId.Should().BeNull();
    }

    /// <summary>
    /// Вызов ЕДИН содержит корректные данные участника.
    /// </summary>
    [Fact]
    public async Task ResolveAndBindAsync_ShouldPassCorrectDataToEdin()
    {
        var participant = CreateParticipant();
        _participants.Add(participant);

        _edinClient.ResolveResult = new EdinPersonResult { MasterId = Guid.NewGuid(), Status = "Matched" };

        await _sut.ResolveAndBindAsync(
            participant.Id, "Петров", "Пётр", "Сергеевич",
            "7709901234", "123-456-789 00", "21", "1234", "567890");

        _edinClient.LastResolveLastName.Should().Be("Петров");
        _edinClient.LastResolveInn.Should().Be("7709901234");
        _edinClient.ResolveCallCount.Should().Be(1);
    }

    private void SetupDbSets()
    {
        var usersQueryable = _users.AsQueryable();
        var usersMockSet = new Mock<DbSet<User>>();
        usersMockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(usersQueryable.Provider);
        usersMockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(usersQueryable.Expression);
        usersMockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(usersQueryable.ElementType);
        usersMockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(usersQueryable.GetEnumerator());
        usersMockSet.Setup(s => s.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(key => Task.FromResult(_users.FirstOrDefault(u => u.Id == (Guid)key[0])));

        _dbContextMock.Setup(db => db.Users).Returns(usersMockSet.Object);

        var participantsQueryable = _participants.AsQueryable();
        var participantsMockSet = new Mock<DbSet<EcosystemParticipant>>();
        participantsMockSet.As<IQueryable<EcosystemParticipant>>().Setup(m => m.Provider).Returns(participantsQueryable.Provider);
        participantsMockSet.As<IQueryable<EcosystemParticipant>>().Setup(m => m.Expression).Returns(participantsQueryable.Expression);
        participantsMockSet.As<IQueryable<EcosystemParticipant>>().Setup(m => m.ElementType).Returns(participantsQueryable.ElementType);
        participantsMockSet.As<IQueryable<EcosystemParticipant>>().Setup(m => m.GetEnumerator()).Returns(participantsQueryable.GetEnumerator());
        participantsMockSet.Setup(s => s.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(key => Task.FromResult(_participants.FirstOrDefault(p => p.Id == (Guid)key[0])));

        _dbContextMock.Setup(db => db.EcosystemParticipants).Returns(participantsMockSet.Object);
        _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private static EcosystemParticipant CreateParticipant() => new()
    {
        Id = Guid.NewGuid(),
        LegalEntityId = Guid.NewGuid(),
        LastName = "Иванов",
        FirstName = "Иван",
        Login = "ivanov"
    };
}
