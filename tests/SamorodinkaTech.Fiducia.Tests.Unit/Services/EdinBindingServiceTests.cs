using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Edin;
using SamorodinkaTech.Fiducia.Infrastructure.Services;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;
using SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Unit-тесты EdinBindingService: привязка MPI MasterId к участнику и поиск УЗ.
/// </summary>
public class EdinBindingServiceTests : IDisposable
{
    private readonly MockEdinApiClient _edinClient = new();
    private readonly Mock<ILogger<EdinBindingService>> _loggerMock = new();
    private readonly FiduciaDbContext _dbContext;
    private readonly EdinBindingService _sut;

    public EdinBindingServiceTests()
    {
        var options = new DbContextOptionsBuilder<FiduciaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new FiduciaDbContext(options);
        _sut = new EdinBindingService(_edinClient, _dbContext, _loggerMock.Object);
    }

    public void Dispose() => _dbContext.Dispose();

    /// <summary>
    /// Успешный resolve: MasterId привязывается к участнику, УЗ найдена в БД → user_id установлен.
    /// </summary>
    [Fact]
    public async Task ResolveAndBindAsync_WhenMasterIdFoundInDb_ShouldLinkUser()
    {
        var masterId = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), MpiMasterId = masterId, Login = "ivanov",
            LastName = "Иванов", FirstName = "Иван", Email = "i@t.ru", Phone = "123",
            CreatedBy = Guid.NewGuid() };
        _dbContext.Users.Add(user);

        var participant = CreateParticipant();
        _dbContext.EcosystemParticipants.Add(participant);
        await _dbContext.SaveChangesAsync();

        _edinClient.ResolveResult = new EdinPersonResult { MasterId = masterId, Status = "Matched" };

        var result = await _sut.ResolveAndBindAsync(
            participant.Id, "Иванов", "Иван", "Иванович",
            "770123456789", null, null, null, null);

        result.Success.Should().BeTrue();
        result.MpiMasterId.Should().Be(masterId);
        result.LinkedUserId.Should().Be(user.Id);
        result.UserSource.Should().Be("db");
    }

    /// <summary>
    /// Resolve возвращает null (сервис недоступен) → ошибка.
    /// </summary>
    [Fact]
    public async Task ResolveAndBindAsync_WhenServiceUnavailable_ShouldReturnError()
    {
        _edinClient.SimulateUnavailable = true;

        var participant = CreateParticipant();
        _dbContext.EcosystemParticipants.Add(participant);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.ResolveAndBindAsync(
            participant.Id, "Иванов", "Иван", null,
            null, null, null, null, null);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("недоступен");
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
        _dbContext.EcosystemParticipants.Add(participant);
        await _dbContext.SaveChangesAsync();

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
        _dbContext.EcosystemParticipants.Add(participant);
        await _dbContext.SaveChangesAsync();

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
        _dbContext.EcosystemParticipants.Add(participant);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.ResolveAndBindAsync(
            participant.Id, "Иванов", "Иван", null,
            null, null, null, null, null);

        result.Success.Should().BeTrue();
        result.MpiMasterId.Should().Be(masterId);
        result.LinkedUserId.Should().BeNull();
    }

    /// <summary>
    /// Вызов ЕДИН содержит корректные данные участника.
    /// </summary>
    [Fact]
    public async Task ResolveAndBindAsync_ShouldPassCorrectDataToEdin()
    {
        var participant = CreateParticipant();
        _dbContext.EcosystemParticipants.Add(participant);
        await _dbContext.SaveChangesAsync();

        _edinClient.ResolveResult = new EdinPersonResult { MasterId = Guid.NewGuid(), Status = "Matched" };

        await _sut.ResolveAndBindAsync(
            participant.Id, "Петров", "Пётр", "Сергеевич",
            "7709901234", "123-456-789 00", "21", "1234", "567890");

        _edinClient.LastResolveLastName.Should().Be("Петров");
        _edinClient.LastResolveInn.Should().Be("7709901234");
        _edinClient.ResolveCallCount.Should().Be(1);
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
