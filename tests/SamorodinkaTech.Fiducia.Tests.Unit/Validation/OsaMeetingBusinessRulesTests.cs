using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Tests.Unit.Fakes;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Validation;

/// <summary>
/// Тесты бизнес-правил ОСА с использованием FakeDbContext (in-memory).
/// Проверяют сценарии, требующие выборки из БД:
/// уникальность года ГОСА, создание и редактирование.
/// </summary>
public class OsaMeetingBusinessRulesTests
{
    [Fact]
    public async Task CreateGosa_NoExisting_ShouldSucceed()
    {
        await using var ctx = FakeDbContextFactory.Create();

        var gosaYear = 2025;
        var exists = await ctx.OsaMeetings
            .AnyAsync(m => m.GosaYear == gosaYear);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CreateGosa_DuplicateYear_ShouldBeDetected()
    {
        await using var ctx = await FakeDbContextFactory.CreateSeededAsync(async db =>
        {
            db.OsaMeetings.Add(new OsaMeeting
            {
                Id = Guid.NewGuid(),
                OsaFormId = Guid.NewGuid(),
                GosaYear = 2025,
                GosaWindowStart = new DateOnly(2025, 3, 1),
                GosaWindowEnd = new DateOnly(2025, 6, 30),
                Title = "Годовое за 2025 год"
            });
            await db.SaveChangesAsync();
        });

        var existing = await ctx.OsaMeetings
            .AnyAsync(m => m.GosaYear == 2025);

        existing.Should().BeTrue();
    }

    [Fact]
    public async Task CreateGosa_DifferentYear_ShouldBeAllowed()
    {
        await using var ctx = await FakeDbContextFactory.CreateSeededAsync(async db =>
        {
            db.OsaMeetings.Add(new OsaMeeting
            {
                Id = Guid.NewGuid(),
                OsaFormId = Guid.NewGuid(),
                GosaYear = 2025,
                Title = "Годовое за 2025 год"
            });
            await db.SaveChangesAsync();
        });

        // 2026 — другой год, должно быть разрешено
        var existing2026 = await ctx.OsaMeetings
            .AnyAsync(m => m.GosaYear == 2026);

        existing2026.Should().BeFalse();
    }

    [Fact]
    public async Task EditGosa_SameYearNoChange_ShouldNotDetectDuplicate()
    {
        var meetingId = Guid.NewGuid();
        await using var ctx = await FakeDbContextFactory.CreateSeededAsync(async db =>
        {
            db.OsaMeetings.Add(new OsaMeeting
            {
                Id = meetingId,
                OsaFormId = Guid.NewGuid(),
                GosaYear = 2025,
                Title = "Годовое за 2025 год"
            });
            await db.SaveChangesAsync();
        });

        // Проверка: редактируем ту же запись — год не меняется, дубликатов нет
        var duplicate = await ctx.OsaMeetings
            .AnyAsync(m => m.Id != meetingId && m.GosaYear == 2025);

        duplicate.Should().BeFalse();
    }

    [Fact]
    public async Task EditGosa_ChangeYearToExistingYear_ShouldDetectDuplicate()
    {
        var meeting1Id = Guid.NewGuid();
        var meeting2Id = Guid.NewGuid();

        await using var ctx = await FakeDbContextFactory.CreateSeededAsync(async db =>
        {
            db.OsaMeetings.Add(new OsaMeeting
            {
                Id = meeting1Id,
                OsaFormId = Guid.NewGuid(),
                GosaYear = 2025,
                Title = "ГОСА 2025"
            });
            db.OsaMeetings.Add(new OsaMeeting
            {
                Id = meeting2Id,
                OsaFormId = Guid.NewGuid(),
                GosaYear = 2026,
                Title = "ГОСА 2026"
            });
            await db.SaveChangesAsync();
        });

        // Пытаемся изменить 2026 → 2025 (уже занято)
        var editTargetId = meeting2Id;
        var newYear = 2025;

        var duplicate = await ctx.OsaMeetings
            .AnyAsync(m => m.Id != editTargetId && m.GosaYear == newYear);

        duplicate.Should().BeTrue();
    }

    [Fact]
    public async Task SaveChanges_AddsRecordToDatabase()
    {
        await using var ctx = FakeDbContextFactory.Create();
        var id = Guid.NewGuid();

        ctx.OsaMeetings.Add(new OsaMeeting
        {
            Id = id,
            OsaFormId = Guid.NewGuid(),
            GosaYear = 2025,
            Title = "ГОСА 2025"
        });
        await ctx.SaveChangesAsync();

        var saved = await ctx.OsaMeetings.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("ГОСА 2025");
    }

    [Fact]
    public async Task MultipleRecords_DifferentYears_AllPersist()
    {
        await using var ctx = await FakeDbContextFactory.CreateSeededAsync(async db =>
        {
            for (int year = 2020; year <= 2025; year++)
            {
                db.OsaMeetings.Add(new OsaMeeting
                {
                    Id = Guid.NewGuid(),
                    OsaFormId = Guid.NewGuid(),
                    GosaYear = year,
                    Title = $"ГОСА {year}"
                });
            }
            await db.SaveChangesAsync();
        });

        var count = await ctx.OsaMeetings.CountAsync();
        count.Should().Be(6);

        var years = await ctx.OsaMeetings
            .Select(m => m.GosaYear)
            .OrderBy(y => y)
            .ToListAsync();

        years.Should().Equal(2020, 2021, 2022, 2023, 2024, 2025);
    }
}
