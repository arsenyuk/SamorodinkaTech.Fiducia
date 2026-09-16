using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class ShareRequestReadServiceTests
{
    private static readonly Guid LeId = new("30000000-0000-0000-0000-000000000001");

    private static FiduciaDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FiduciaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new FiduciaDbContext(
            options,
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Edin:Enabled"] = "false" })
                .Build(),
            LoggerFactory.Create(_ => { }));
    }

    private static IDbContextFactory<FiduciaDbContext> WrapFactory(FiduciaDbContext ctx) =>
        Mock.Of<IDbContextFactory<FiduciaDbContext>>(
            f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()) == Task.FromResult(ctx));

    // ── Seed helpers ───────────────────────────────────────────────

    private static User SeedUser(FiduciaDbContext ctx, Guid? id = null, string? login = null)
    {
        var user = new User
        {
            Id = id ?? Guid.NewGuid(),
            Login = login ?? Guid.NewGuid().ToString("N")[..10],
            LastName = "Тестов",
            FirstName = "Тест",
            Email = $"{Guid.NewGuid():N}@test.com",
            Phone = "1234567890",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        ctx.Users.Add(user);
        return user;
    }

    private static BoardParticipant SeedBoardParticipant(
        FiduciaDbContext ctx,
        Guid legalEntityId,
        Guid? ecosystemParticipantId = null,
        bool isActive = true)
    {
        var bp = new BoardParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = legalEntityId,
            EcosystemParticipantId = ecosystemParticipantId,
            ParticipantType = "FL",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.BoardParticipants.Add(bp);
        return bp;
    }

    private static EcosystemParticipant SeedEcosystemParticipant(
        FiduciaDbContext ctx,
        Guid legalEntityId,
        Guid userId)
    {
        var ecoPerson = new EcosystemPerson
        {
            Id = Guid.NewGuid(),
            LastName = "Тестов",
            FirstName = "Тест",
            CreatedAt = DateTime.UtcNow
        };
        ctx.EcosystemPersons.Add(ecoPerson);

        var eco = new EcosystemParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = legalEntityId,
            EcosystemPersonId = ecoPerson.Id,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        ctx.EcosystemParticipants.Add(eco);
        return eco;
    }

    private static RefRequestType SeedRequestType(
        FiduciaDbContext ctx,
        string code,
        string name,
        bool isForLlc = true)
    {
        var rt = new RefRequestType
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            IsForLlc = isForLlc,
            IsForNjsc = false,
            IsForPjsc = false,
            RequiresFile = false,
            ConsideredByOsu = false,
            CreatedAt = DateTime.UtcNow
        };
        ctx.RequestTypes.Add(rt);
        return rt;
    }

    // ── GetListAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetListAsync_ReturnsShareRequestsViaEcosystemParticipantChain()
    {
        using var ctx = CreateContext();

        var user = SeedUser(ctx);
        var eco = SeedEcosystemParticipant(ctx, LeId, user.Id);
        var bp = SeedBoardParticipant(ctx, LeId, eco.Id);

        var requestType = SeedRequestType(ctx, "PREEMPTIVE_LIST", "Список участников");

        ctx.ShareRequests.Add(new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantId = bp.Id,
            RequestTypeId = requestType.Id,
            Status = "draft",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        });
        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetListAsync(user.Id);

        result.Should().HaveCount(1);
        result[0].RequestType!.Code.Should().Be("PREEMPTIVE_LIST");
    }

    [Fact]
    public async Task GetListAsync_EmptyForUserWithNoBoardParticipants()
    {
        using var ctx = CreateContext();
        var user = SeedUser(ctx);
        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetListAsync(user.Id);

        result.Should().BeEmpty();
    }

    // ── GetTypesAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetTypesAsync_ReturnsActiveRequestTypes()
    {
        using var ctx = CreateContext();

        ctx.RequestTypes.AddRange(
            new RefRequestType
            {
                Id = Guid.NewGuid(), Code = "ZAOCHN", Name = "Заочное",
                IsForLlc = true, IsForNjsc = false, IsForPjsc = false,
                RequiresFile = false, ConsideredByOsu = false, CreatedAt = DateTime.UtcNow
            },
            new RefRequestType
            {
                Id = Guid.NewGuid(), Code = "OCHN", Name = "Очное",
                IsForLlc = true, IsForNjsc = false, IsForPjsc = false,
                RequiresFile = false, ConsideredByOsu = false, CreatedAt = DateTime.UtcNow
            });
        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetTypesAsync();

        result.Should().HaveCount(2);
        // Ordered by Name: "Заочное" < "Очное" in Russian alphabetical order
        result[0].Name.Should().Be("Заочное");
        result[1].Name.Should().Be("Очное");
    }

    // ── GetRefDataAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetRefDataAsync_ReturnsDocumentGroupsAndAccessMethods()
    {
        using var ctx = CreateContext();

        ctx.DocumentTypes.AddRange(
            new RefDocumentType
            {
                Id = Guid.NewGuid(), Code = "CHARTER", Name = "Устав",
                GroupCode = "FOUNDING", GroupName = "Учредительные документы",
                SortOrder = 1, CreatedAt = DateTime.UtcNow
            },
            new RefDocumentType
            {
                Id = Guid.NewGuid(), Code = "ACCOUNTING", Name = "Бухгалтерская отчётность",
                GroupCode = "FINANCE", GroupName = "Финансовые документы",
                SortOrder = 2, CreatedAt = DateTime.UtcNow
            });

        ctx.DocumentAccessMethods.AddRange(
            new RefDocumentAccessMethod
            {
                Id = Guid.NewGuid(), Code = "IN_PERSON", Name = "Ознакомление в офисе",
                SortOrder = 1, CreatedAt = DateTime.UtcNow
            },
            new RefDocumentAccessMethod
            {
                Id = Guid.NewGuid(), Code = "COPIES_ISSUE", Name = "Выдача копий",
                DeadlineDays = 5, SortOrder = 2, CreatedAt = DateTime.UtcNow
            });

        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetRefDataAsync();

        result.DocumentGroups.Should().HaveCount(2);
        // Ordered by SortOrder: CHARTER=1 (FOUNDING) first, ACCOUNTING=2 (FINANCE) second
        result.DocumentGroups[0].Code.Should().Be("FOUNDING");
        result.DocumentGroups[0].Documents.Should().HaveCount(1);
        result.DocumentGroups[1].Code.Should().Be("FINANCE");
        result.DocumentGroups[1].Documents.Should().HaveCount(1);

        result.AccessMethods.Should().HaveCount(2);
        result.AccessMethods[0].Code.Should().Be("IN_PERSON");
        result.AccessMethods[1].Code.Should().Be("COPIES_ISSUE");
    }

    // ── GetDetailAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetDetailAsync_ReturnsRequestWithType()
    {
        using var ctx = CreateContext();

        var rt = SeedRequestType(ctx, "NOTARIAL_OFFER", "Нотариальная оферта");

        var request = new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantId = Guid.NewGuid(),
            RequestTypeId = rt.Id,
            Status = "draft",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        ctx.ShareRequests.Add(request);
        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetDetailAsync(request.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(request.Id);
        result.RequestType!.Code.Should().Be("NOTARIAL_OFFER");
    }

    [Fact]
    public async Task GetDetailAsync_ReturnsNullForNonExistentId()
    {
        using var ctx = CreateContext();
        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetDetailAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── GetSupportsAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetSupportsAsync_ReturnsWithParticipantIncludes()
    {
        using var ctx = CreateContext();

        var rt = SeedRequestType(ctx, "PREEMPTIVE_LIST", "Список");

        var person = new Person
        {
            Id = Guid.NewGuid(), LastName = "Поддержкин", FirstName = "Поддержка",
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        ctx.Persons.Add(person);

        var bp = new BoardParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantType = "FL",
            PersonId = person.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.BoardParticipants.Add(bp);

        var request = new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantId = bp.Id,
            RequestTypeId = rt.Id,
            Status = "submitted",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        ctx.ShareRequests.Add(request);

        ctx.ShareRequestSupports.Add(new ShareRequestSupport
        {
            Id = Guid.NewGuid(),
            ShareRequestId = request.Id,
            ParticipantId = bp.Id,
            SharePercentAtSupport = 15m,
            SupportedAt = new DateTime(2025, 6, 1)
        });

        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetSupportsAsync(request.Id);

        result.Should().HaveCount(1);
        result[0].Participant!.Person!.LastName.Should().Be("Поддержкин");
    }

    // ── GetFilesAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetFilesAsync_ReturnsWithFileIncludes()
    {
        using var ctx = CreateContext();

        var rt = SeedRequestType(ctx, "PREEMPTIVE_LIST", "Список");

        var bp = SeedBoardParticipant(ctx, LeId);

        var request = new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantId = bp.Id,
            RequestTypeId = rt.Id,
            Status = "submitted",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        ctx.ShareRequests.Add(request);

        var file = new FileEntry
        {
            Id = Guid.NewGuid(),
            OriginalName = "doc.pdf",
            ContentType = "application/pdf",
            SizeBytes = 1024,
            StorageProvider = "LOCAL",
            StorageKeyOrPath = "/tmp/doc.pdf",
            CreatedAt = new DateTime(2025, 6, 1)
        };
        ctx.Files.Add(file);

        ctx.ShareRequestFiles.Add(new ShareRequestFile
        {
            Id = Guid.NewGuid(),
            ShareRequestId = request.Id,
            FileId = file.Id
        });

        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetFilesAsync(request.Id);

        result.Should().HaveCount(1);
        result[0].File!.OriginalName.Should().Be("doc.pdf");
    }

    // ── GetItemsAsync ──────────────────────────────────────────────

    [Fact]
    public async Task GetItemsAsync_ReturnsSortedBySequenceNumber()
    {
        using var ctx = CreateContext();

        var rt = SeedRequestType(ctx, "REQUEST_INFORMATION", "Запрос информации");

        var bp = SeedBoardParticipant(ctx, LeId);

        var request = new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantId = bp.Id,
            RequestTypeId = rt.Id,
            Status = "submitted",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        ctx.ShareRequests.Add(request);

        ctx.ShareRequestItems.AddRange(
            new ShareRequestItem
            {
                Id = Guid.NewGuid(),
                ShareRequestId = request.Id,
                SequenceNumber = 2,
                Title = "Бухгалтерская отчётность",
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            },
            new ShareRequestItem
            {
                Id = Guid.NewGuid(),
                ShareRequestId = request.Id,
                SequenceNumber = 1,
                Title = "Устав",
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            });

        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetItemsAsync(request.Id);

        result.Should().HaveCount(2);
        result[0].SequenceNumber.Should().Be(1);
        result[1].SequenceNumber.Should().Be(2);
    }

    // ── GetResultAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetResultAsync_ReturnsEmptyForNonPreemptiveListRequest()
    {
        using var ctx = CreateContext();

        var rt = SeedRequestType(ctx, "NOTARIAL_OFFER", "Нотариальная оферта");
        var bp = SeedBoardParticipant(ctx, LeId);

        var request = new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantId = bp.Id,
            RequestTypeId = rt.Id,
            Status = "completed",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        ctx.ShareRequests.Add(request);
        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetResultAsync(request.Id);

        result.Should().BeEmpty();
    }

    // ── GetNotificationsAsync ──────────────────────────────────────

    [Fact]
    public async Task GetNotificationsAsync_ReturnsMatchingRequestId()
    {
        using var ctx = CreateContext();

        var requestId = Guid.NewGuid();

        ctx.Notifications.AddRange(
            new Notification
            {
                Id = Guid.NewGuid(),
                NotificationType = "TEST",
                Title = "Уведомление по заявке",
                Body = "Текст",
                Url = $"/share-requests/{requestId}",
                IsRead = false,
                CreatedAt = new DateTime(2025, 6, 1)
            },
            new Notification
            {
                Id = Guid.NewGuid(),
                NotificationType = "TEST",
                Title = "Другое уведомление",
                Body = "Текст",
                Url = "/other-page",
                IsRead = false,
                CreatedAt = new DateTime(2025, 6, 2)
            });

        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetNotificationsAsync(requestId);

        result.Should().HaveCount(1);
        result[0].Url.Should().Contain(requestId.ToString());
    }

    // ── GetVosuNotificationsAsync ──────────────────────────────────

    [Fact]
    public async Task GetVosuNotificationsAsync_ReturnsEmptyWhenNoOrgIntentId()
    {
        using var ctx = CreateContext();

        var rt = SeedRequestType(ctx, "PREEMPTIVE_LIST", "Список");
        var bp = SeedBoardParticipant(ctx, LeId);

        var request = new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantId = bp.Id,
            RequestTypeId = rt.Id,
            Status = "submitted",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        ctx.ShareRequests.Add(request);
        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetVosuNotificationsAsync(request.Id);

        result.Should().BeEmpty();
    }

    // ── GetDocumentCatalogAsync ────────────────────────────────────

    [Fact]
    public async Task GetDocumentCatalogAsync_GroupsFilesByItemTitle()
    {
        using var ctx = CreateContext();

        var rt = SeedRequestType(ctx, "REQUEST_INFORMATION", "Запрос информации");

        var bp = SeedBoardParticipant(ctx, LeId);

        var request = new ShareRequest
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantId = bp.Id,
            RequestTypeId = rt.Id,
            Status = "accepted",
            CreatedAt = new DateTime(2025, 6, 1),
            CreatedBy = Guid.NewGuid()
        };
        ctx.ShareRequests.Add(request);

        var item1 = new ShareRequestItem
        {
            Id = Guid.NewGuid(),
            ShareRequestId = request.Id,
            SequenceNumber = 1,
            Title = "Устав",
            Status = "approved",
            CreatedAt = DateTime.UtcNow
        };
        var item2 = new ShareRequestItem
        {
            Id = Guid.NewGuid(),
            ShareRequestId = request.Id,
            SequenceNumber = 2,
            Title = "Устав",
            Status = "approved",
            CreatedAt = DateTime.UtcNow
        };
        ctx.ShareRequestItems.AddRange(item1, item2);

        var file1 = new FileEntry
        {
            Id = Guid.NewGuid(), OriginalName = "charter.pdf", SizeBytes = 100,
            StorageProvider = "LOCAL", StorageKeyOrPath = "/tmp/charter.pdf",
            CreatedAt = new DateTime(2025, 6, 2)
        };
        var file2 = new FileEntry
        {
            Id = Guid.NewGuid(), OriginalName = "charter2.pdf", SizeBytes = 200,
            StorageProvider = "LOCAL", StorageKeyOrPath = "/tmp/charter2.pdf",
            CreatedAt = new DateTime(2025, 6, 3)
        };
        ctx.Files.AddRange(file1, file2);

        ctx.ShareRequestItemFiles.AddRange(
            new ShareRequestItemFile
            {
                Id = Guid.NewGuid(), ShareRequestItemId = item1.Id, FileId = file1.Id,
                CreatedAt = new DateTime(2025, 6, 2)
            },
            new ShareRequestItemFile
            {
                Id = Guid.NewGuid(), ShareRequestItemId = item2.Id, FileId = file2.Id,
                CreatedAt = new DateTime(2025, 6, 3)
            });

        await ctx.SaveChangesAsync();

        var sut = new ShareRequestReadService(WrapFactory(ctx), Mock.Of<ILogger<ShareRequestReadService>>());

        var result = await sut.GetDocumentCatalogAsync(LeId);

        result.Should().HaveCount(1);
        result[0].GroupName.Should().Be("Устав");
        result[0].TotalFiles.Should().Be(2);
        result[0].LatestProvisionDate.Should().Be(new DateTime(2025, 6, 3));
    }
}
