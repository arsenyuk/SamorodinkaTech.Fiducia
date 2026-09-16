using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class ParticipantReadServiceTests
{
    private static readonly Guid LeId = new("20000000-0000-0000-0000-000000000001");
    private static readonly Guid LeId2 = new("20000000-0000-0000-0000-000000000002");

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

    // ── GetByLegalEntityAsync ──────────────────────────────────────

    [Fact]
    public async Task GetByLegalEntityAsync_ReturnsParticipantsSortedByShareDesc()
    {
        using var ctx = CreateContext();

        var person1 = new Person { Id = Guid.NewGuid(), LastName = "Иванов", FirstName = "Иван", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var person2 = new Person { Id = Guid.NewGuid(), LastName = "Петров", FirstName = "Пётр", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        ctx.Persons.AddRange(person1, person2);

        var bp1 = new BoardParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantType = "FL",
            PersonId = person1.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var bp2 = new BoardParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantType = "FL",
            PersonId = person2.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.BoardParticipants.AddRange(bp1, bp2);

        ctx.BoardParticipantShares.AddRange(
            new BoardParticipantShare
            {
                Id = Guid.NewGuid(),
                ParticipantId = bp1.Id,
                LegalEntityId = LeId,
                SharePercent = 10m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new BoardParticipantShare
            {
                Id = Guid.NewGuid(),
                ParticipantId = bp2.Id,
                LegalEntityId = LeId,
                SharePercent = 30m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        await ctx.SaveChangesAsync();

        var sut = new ParticipantReadService(WrapFactory(ctx));

        var result = await sut.GetByLegalEntityAsync(LeId);

        result.Should().HaveCount(2);
        result[0].Person!.LastName.Should().Be("Петров");
        result[1].Person!.LastName.Should().Be("Иванов");
    }

    [Fact]
    public async Task GetByLegalEntityAsync_EmptyForNonExistentLegalEntity()
    {
        using var ctx = CreateContext();

        ctx.BoardParticipants.Add(new BoardParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            ParticipantType = "FL",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await ctx.SaveChangesAsync();

        var sut = new ParticipantReadService(WrapFactory(ctx));

        var result = await sut.GetByLegalEntityAsync(LeId2);

        result.Should().BeEmpty();
    }

    // ── GetDetailAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetDetailAsync_ReturnsParticipantWithIncludes()
    {
        using var ctx = CreateContext();

        var ecoPerson = new EcosystemPerson
        {
            Id = Guid.NewGuid(),
            LastName = "Сидоров",
            FirstName = "Сидор",
            CreatedAt = DateTime.UtcNow
        };
        ctx.EcosystemPersons.Add(ecoPerson);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "sidorov",
            LastName = "Сидоров",
            FirstName = "Сидор",
            Email = "s@test.com",
            Phone = "123",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        ctx.Users.Add(user);

        var ecoParticipant = new EcosystemParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            EcosystemPersonId = ecoPerson.Id,
            UserId = user.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        ctx.EcosystemParticipants.Add(ecoParticipant);

        var person = new Person
        {
            Id = Guid.NewGuid(),
            LastName = "Сидоров",
            FirstName = "Сидор",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.Persons.Add(person);

        var bp = new BoardParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            EcosystemParticipantId = ecoParticipant.Id,
            PersonId = person.Id,
            ParticipantType = "FL",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.BoardParticipants.Add(bp);

        ctx.BoardParticipantCompanies.Add(new BoardParticipantCompany
        {
            Id = Guid.NewGuid(),
            ParticipantId = bp.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        ctx.BoardParticipantShares.Add(new BoardParticipantShare
        {
            Id = Guid.NewGuid(),
            ParticipantId = bp.Id,
            LegalEntityId = LeId,
            SharePercent = 25m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var sut = new ParticipantReadService(WrapFactory(ctx));

        var result = await sut.GetDetailAsync(bp.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(bp.Id);
        result.EcosystemParticipant.Should().NotBeNull();
        result.EcosystemParticipant!.User.Should().NotBeNull();
        result.Person.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDetailAsync_ReturnsNullForNonExistentId()
    {
        using var ctx = CreateContext();
        var sut = new ParticipantReadService(WrapFactory(ctx));

        var result = await sut.GetDetailAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── GetCurrentAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentAsync_ReturnsParticipantAndShareForUser()
    {
        using var ctx = CreateContext();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "testuser",
            LastName = "Тестов",
            FirstName = "Тест",
            Email = "t@test.com",
            Phone = "999",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
        ctx.Users.Add(user);

        var ecoPerson = new EcosystemPerson
        {
            Id = Guid.NewGuid(),
            LastName = "Тестов",
            FirstName = "Тест",
            CreatedAt = DateTime.UtcNow
        };
        ctx.EcosystemPersons.Add(ecoPerson);

        var ecoParticipant = new EcosystemParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            EcosystemPersonId = ecoPerson.Id,
            UserId = user.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        ctx.EcosystemParticipants.Add(ecoParticipant);

        var bp = new BoardParticipant
        {
            Id = Guid.NewGuid(),
            LegalEntityId = LeId,
            EcosystemParticipantId = ecoParticipant.Id,
            ParticipantType = "FL",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        ctx.BoardParticipants.Add(bp);

        ctx.BoardParticipantShares.Add(new BoardParticipantShare
        {
            Id = Guid.NewGuid(),
            ParticipantId = bp.Id,
            LegalEntityId = LeId,
            SharePercent = 40m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();

        var sut = new ParticipantReadService(WrapFactory(ctx));

        var (participant, sharePercent) = await sut.GetCurrentAsync(user.Id, LeId);

        participant.Should().NotBeNull();
        participant!.Id.Should().Be(bp.Id);
        sharePercent.Should().Be(40m);
    }

    [Fact]
    public async Task GetCurrentAsync_ReturnsNullTupleForNonExistentUser()
    {
        using var ctx = CreateContext();
        var sut = new ParticipantReadService(WrapFactory(ctx));

        var (participant, sharePercent) = await sut.GetCurrentAsync(Guid.NewGuid(), LeId);

        participant.Should().BeNull();
        sharePercent.Should().BeNull();
    }

    // ── GetTreasuryAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetTreasuryAsync_ReturnsSortedBySortOrder()
    {
        using var ctx = CreateContext();

        ctx.BoardTreasuryShares.AddRange(
            new BoardTreasuryShare
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId,
                SharePercent = 5m,
                SortOrder = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new BoardTreasuryShare
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId,
                SharePercent = 2m,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new BoardTreasuryShare
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId,
                SharePercent = 3m,
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        await ctx.SaveChangesAsync();

        var sut = new ParticipantReadService(WrapFactory(ctx));

        var result = await sut.GetTreasuryAsync(LeId);

        result.Should().HaveCount(3);
        result[0].SortOrder.Should().Be(1);
        result[1].SortOrder.Should().Be(2);
        result[2].SortOrder.Should().Be(3);
    }

    // ── GetRegistryUploadsAsync ────────────────────────────────────

    [Fact]
    public async Task GetRegistryUploadsAsync_ReturnsSortedByDateDesc()
    {
        using var ctx = CreateContext();

        ctx.BoardRegistryUploads.AddRange(
            new BoardRegistryUpload
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId,
                Status = "uploaded",
                UploadedAt = new DateTime(2025, 1, 1),
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 1, 1)
            },
            new BoardRegistryUpload
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId,
                Status = "processed",
                UploadedAt = new DateTime(2025, 6, 15),
                CreatedAt = new DateTime(2025, 6, 15),
                UpdatedAt = new DateTime(2025, 6, 15)
            },
            new BoardRegistryUpload
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId,
                Status = "uploaded",
                UploadedAt = new DateTime(2025, 3, 10),
                CreatedAt = new DateTime(2025, 3, 10),
                UpdatedAt = new DateTime(2025, 3, 10)
            });

        await ctx.SaveChangesAsync();

        var sut = new ParticipantReadService(WrapFactory(ctx));

        var result = await sut.GetRegistryUploadsAsync(LeId);

        result.Should().HaveCount(3);
        result[0].UploadedAt.Should().Be(new DateTime(2025, 6, 15));
        result[1].UploadedAt.Should().Be(new DateTime(2025, 3, 10));
        result[2].UploadedAt.Should().Be(new DateTime(2025, 1, 1));
    }

    // ── GetChangesAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetChangesAsync_ReturnsFilteredByLegalEntity()
    {
        using var ctx = CreateContext();

        var participantId = Guid.NewGuid();

        ctx.BoardParticipantChanges.AddRange(
            new BoardParticipantChange
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId,
                ParticipantId = participantId,
                ParticipantType = "FL",
                Status = "pending",
                SubmittedAt = new DateTime(2025, 1, 1),
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 1, 1)
            },
            new BoardParticipantChange
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId2,
                ParticipantId = participantId,
                ParticipantType = "FL",
                Status = "pending",
                SubmittedAt = new DateTime(2025, 2, 1),
                CreatedAt = new DateTime(2025, 2, 1),
                UpdatedAt = new DateTime(2025, 2, 1)
            });

        await ctx.SaveChangesAsync();

        var sut = new ParticipantReadService(WrapFactory(ctx));

        var result = await sut.GetChangesAsync(LeId);

        result.Should().HaveCount(1);
        result[0].LegalEntityId.Should().Be(LeId);
    }

    [Fact]
    public async Task GetChangesAsync_FiltersByParticipantId()
    {
        using var ctx = CreateContext();

        var p1 = Guid.NewGuid();
        var p2 = Guid.NewGuid();

        ctx.BoardParticipantChanges.AddRange(
            new BoardParticipantChange
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId,
                ParticipantId = p1,
                ParticipantType = "FL",
                Status = "pending",
                SubmittedAt = new DateTime(2025, 3, 1),
                CreatedAt = new DateTime(2025, 3, 1),
                UpdatedAt = new DateTime(2025, 3, 1)
            },
            new BoardParticipantChange
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId,
                ParticipantId = p2,
                ParticipantType = "FL",
                Status = "approved",
                SubmittedAt = new DateTime(2025, 2, 1),
                CreatedAt = new DateTime(2025, 2, 1),
                UpdatedAt = new DateTime(2025, 2, 1)
            },
            new BoardParticipantChange
            {
                Id = Guid.NewGuid(),
                LegalEntityId = LeId,
                ParticipantId = p1,
                ParticipantType = "FL",
                Status = "rejected",
                SubmittedAt = new DateTime(2025, 1, 1),
                CreatedAt = new DateTime(2025, 1, 1),
                UpdatedAt = new DateTime(2025, 1, 1)
            });

        await ctx.SaveChangesAsync();

        var sut = new ParticipantReadService(WrapFactory(ctx));

        var result = await sut.GetChangesAsync(LeId, participantId: p1);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(c => c.ParticipantId.Should().Be(p1));
        result[0].SubmittedAt.Should().Be(new DateTime(2025, 3, 1));
        result[1].SubmittedAt.Should().Be(new DateTime(2025, 1, 1));
    }
}
