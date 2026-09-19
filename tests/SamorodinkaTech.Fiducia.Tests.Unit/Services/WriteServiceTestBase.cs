using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Общая база для unit-тестов write-сервисов.
/// Предоставляет InMemory-контекст, фабрику, seed-данные.
/// </summary>
public static class WriteServiceTestBase
{
    /// <summary>Создаёт FiduciaDbContext с InMemory-провайдером.</summary>
    public static FiduciaDbContext CreateContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<FiduciaDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Edin:Enabled"] = "false" })
            .Build();
        return new FiduciaDbContext(options, config, LoggerFactory.Create(_ => { }));
    }

    /// <summary>
    /// Создаёт мок IDbContextFactory и вычисляет опции, разделяемые всеми контекстами.
    /// Сид-данные записываются в seedContext, а сервисы получают отдельные инстансы
    /// через фабрику (все разделяют одну InMemory-базу).
    /// </summary>
    public static (IDbContextFactory<FiduciaDbContext> factory, DbContextOptions<FiduciaDbContext> sharedOptions)
        CreateMockFactory(out FiduciaDbContext seedContext, string? dbName = null)
    {
        var dbGuid = dbName ?? Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<FiduciaDbContext>()
            .UseInMemoryDatabase(databaseName: dbGuid)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Edin:Enabled"] = "false" })
            .Build();
        seedContext = new FiduciaDbContext(options, config, LoggerFactory.Create(_ => { }));

        var factory = new Mock<IDbContextFactory<FiduciaDbContext>>();
        factory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                var cfg = new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?> { ["Edin:Enabled"] = "false" })
                    .Build();
                var newCtx = new FiduciaDbContext(options, cfg, LoggerFactory.Create(_ => { }));
                return Task.FromResult(newCtx);
            });
        return (factory.Object, options);
    }

    /// <summary>Сидит RefOkopf и возвращает его Id.</summary>
    public static Guid SeedOkopf(FiduciaDbContext ctx, string code = "12300")
    {
        var okopf = new RefOkopf
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = code == "12300" ? "ООО" : "ПАО",
            CreatedBy = Guid.NewGuid()
        };
        ctx.RefOkopf.Add(okopf);
        ctx.SaveChanges();
        return okopf.Id;
    }

    /// <summary>Сидит LegalEntity и возвращает его.</summary>
    public static LegalEntity SeedLegalEntity(FiduciaDbContext ctx, Guid okopfId, string? name = null)
    {
        var le = new LegalEntity
        {
            Id = Guid.NewGuid(),
            Name = name ?? "ООО Тест",
            Inn = "7701234567",
            Ogrn = "1027700123456",
            OkopfId = okopfId
        };
        ctx.LegalEntities.Add(le);
        ctx.SaveChanges();
        return le;
    }

    /// <summary>Сидит User и возвращает его.</summary>
    public static User SeedUser(FiduciaDbContext ctx, string? login = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = login ?? "testuser",
            LastName = "Тестов",
            FirstName = "Иван",
            IsActive = true,
            CreatedBy = Guid.NewGuid()
        };
        ctx.Users.Add(user);
        ctx.SaveChanges();
        return user;
    }

    /// <summary>Сидит EcosystemParticipant и возвращает его.</summary>
    public static EcosystemParticipant SeedEcosystemParticipant(
        FiduciaDbContext ctx, Guid userId, Guid legalEntityId)
    {
        var ecoPerson = new EcosystemPerson
        {
            Id = Guid.NewGuid(),
            LastName = "Тестов",
            FirstName = "Иван",
            CreatedBy = userId
        };
        ctx.EcosystemPersons.Add(ecoPerson);
        ctx.SaveChanges();

        var eco = new EcosystemParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = legalEntityId,
            EcosystemPersonId = ecoPerson.Id,
            UserId = userId,
            IsActive = true
        };
        ctx.EcosystemParticipants.Add(eco);
        ctx.SaveChanges();
        return eco;
    }

    /// <summary>Сидит RefRole с указанным кодом и возвращает его.</summary>
    public static RefRole SeedRole(FiduciaDbContext ctx, string code)
    {
        var role = new RefRole
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = code
        };
        ctx.Roles.Add(role);
        ctx.SaveChanges();
        return role;
    }

    /// <summary>Сидит UserRole.</summary>
    public static void SeedUserRole(FiduciaDbContext ctx, Guid userId, Guid roleId)
    {
        var ur = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId
        };
        ctx.UserRoles.Add(ur);
        ctx.SaveChanges();
    }

    /// <summary>Сидит BoardParticipant и возвращает его.</summary>
    public static BoardParticipant SeedBoardParticipant(
        FiduciaDbContext ctx,
        Guid legalEntityId,
        string participantType = "FL",
        bool isActive = true,
        Guid? ecosystemParticipantId = null,
        bool isGeneralDirector = false)
    {
        var bp = new BoardParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = legalEntityId,
            ParticipantType = participantType,
            IsActive = isActive,
            EcosystemParticipantId = ecosystemParticipantId,
            IsGeneralDirector = isGeneralDirector
        };
        ctx.BoardParticipants.Add(bp);
        ctx.SaveChanges();
        return bp;
    }

    /// <summary>Сидит Person и возвращает его.</summary>
    public static Person SeedPerson(FiduciaDbContext ctx, string lastName = "Тестов", string firstName = "Иван")
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            LastName = lastName,
            FirstName = firstName,
            Inn = "770123456789"
        };
        ctx.Persons.Add(person);
        ctx.SaveChanges();
        return person;
    }

    /// <summary>Сидит BoardParticipantShare и возвращает его.</summary>
    public static BoardParticipantShare SeedShare(
        FiduciaDbContext ctx, Guid participantId, Guid legalEntityId,
        decimal? sharePercent = 50m)
    {
        var share = new BoardParticipantShare
        {
            Id = Guid.NewGuid(),
            ParticipantId = participantId,
            LegalEntityId = legalEntityId,
            SharePercent = sharePercent,
            IsActive = true
        };
        ctx.BoardParticipantShares.Add(share);
        ctx.SaveChanges();
        return share;
    }

    /// <summary>Сидит BoardTreasuryShare и возвращает его.</summary>
    public static BoardTreasuryShare SeedTreasuryShare(
        FiduciaDbContext ctx, Guid legalEntityId, decimal? sharePercent = 5m)
    {
        var ts = new BoardTreasuryShare
        {
            Id = Guid.NewGuid(),
            LegalEntityId = legalEntityId,
            SharePercent = sharePercent,
            SortOrder = 1
        };
        ctx.BoardTreasuryShares.Add(ts);
        ctx.SaveChanges();
        return ts;
    }

    /// <summary>Сидит RefRequestType и возвращает его.</summary>
    public static RefRequestType SeedRequestType(
        FiduciaDbContext ctx, string code, bool isForLlc = true, bool consideredByOsu = false)
    {
        var rt = new RefRequestType
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = code,
            IsForLlc = isForLlc,
            ConsideredByOsu = consideredByOsu
        };
        ctx.RequestTypes.Add(rt);
        ctx.SaveChanges();
        return rt;
    }

    /// <summary>Сидит ShareRequest и возвращает его.</summary>
    public static ShareRequest SeedShareRequest(
        FiduciaDbContext ctx, Guid legalEntityId, Guid participantId,
        Guid requestTypeId, string status = "draft")
    {
        var sr = new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = legalEntityId,
            ParticipantId = participantId,
            RequestTypeId = requestTypeId,
            Status = status,
            CreatedBy = Guid.NewGuid()
        };
        ctx.ShareRequests.Add(sr);
        ctx.SaveChanges();
        return sr;
    }

    /// <summary>Сидит AgendaItem и возвращает его.</summary>
    public static AgendaItem SeedAgendaItem(
        FiduciaDbContext ctx, Guid legalEntityId,
        Guid? shareRequestId = null, string status = "PENDING")
    {
        var item = new AgendaItem
        {
            Id = Guid.NewGuid(),
            BoardOfDirectorsId = Guid.NewGuid(),
            LegalEntityId = legalEntityId,
            ShareRequestId = shareRequestId,
            Title = "Тестовый пункт",
            TargetType = "BOARD_MEETING",
            Reason = "Тест",
            Status = status
        };
        ctx.AgendaItems.Add(item);
        ctx.SaveChanges();
        return item;
    }

    /// <summary>Сидит RefBoardRole и возвращает его.</summary>
    public static RefBoardRole SeedBoardRole(FiduciaDbContext ctx, string code)
    {
        var role = new RefBoardRole
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = code
        };
        ctx.BoardRoles.Add(role);
        ctx.SaveChanges();
        return role;
    }

    /// <summary>Сидит OsaMeeting и LegalEntity для уведомлений.</summary>
    public static (OsaMeeting meeting, LegalEntity le) SeedMeetingForNotification(
        FiduciaDbContext ctx, Guid okopfId)
    {
        var le = SeedLegalEntity(ctx, okopfId, "ООО Уведомления");
        var meeting = new OsaMeeting
        {
            Id = Guid.NewGuid(),
            LegalEntityId = le.Id,
            OsaFormId = Guid.NewGuid(),
            Title = "Тестовое собрание"
        };
        ctx.OsaMeetings.Add(meeting);
        ctx.SaveChanges();
        return (meeting, le);
    }
}
