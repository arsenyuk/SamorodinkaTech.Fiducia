using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Validation;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Validation;

/// <summary>
/// Тесты DB-валидатора уникальности года ГОСА.
/// Проверяют бизнес-правило «не более 1 ГОСА в году»
/// через Moq-мок IApplicationDbContext (Clean Architecture / Dependency Inversion).
/// </summary>
public class OsaMeetingBusinessRulesTests
{
    /// <summary>
    /// Нет существующих записей — создание нового ГОСА должно пройти успешно.
    /// </summary>
    [Fact]
    public void CreateGosa_NoExisting_ShouldSucceed()
    {
        var mockDb = MockDb(Array.Empty<OsaMeeting>());

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, null, 2025);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Существующая запись на тот же год — дубликат обнаружен.
    /// </summary>
    [Fact]
    public void CreateGosa_DuplicateYear_ShouldBeDetected()
    {
        var mockDb = MockDb(new[] { Gosa(2025) });

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, null, 2025);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("2025") && e.Contains("уже существует"));
    }

    /// <summary>
    /// Существующая запись на другой год — создание разрешено.
    /// </summary>
    [Fact]
    public void CreateGosa_DifferentYear_ShouldBeAllowed()
    {
        var mockDb = MockDb(new[] { Gosa(2025) });

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, null, 2026);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// При редактировании (год не меняется) — дубликат не должен обнаруживаться.
    /// </summary>
    [Fact]
    public void EditGosa_SameYearNoChange_ShouldNotDetectDuplicate()
    {
        var meetingId = Guid.NewGuid();
        var mockDb = MockDb(new[] { Gosa(2025, meetingId) });

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, meetingId, 2025);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// При редактировании смена года на уже занятый — дубликат обнаружен.
    /// </summary>
    [Fact]
    public void EditGosa_ChangeYearToExistingYear_ShouldDetectDuplicate()
    {
        var meeting1Id = Guid.NewGuid();
        var meeting2Id = Guid.NewGuid();
        var mockDb = MockDb(new[]
        {
            Gosa(2025, meeting1Id),
            Gosa(2026, meeting2Id)
        });

        // Редактируем встречу 2026 → 2025 (уже занято)
        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, meeting2Id, 2025);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("2025") && e.Contains("уже существует"));
    }

    /// <summary>
    /// Год не указан (null) — проверка пропускается.
    /// </summary>
    [Fact]
    public void CreateGosa_NullYear_ShouldSkipCheck()
    {
        var mockDb = MockDb(Array.Empty<OsaMeeting>());

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, null, null);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Год равен нулю — проверка пропускается (0 означает «год не указан»).
    /// </summary>
    [Fact]
    public void CreateGosa_ZeroYear_ShouldSkipCheck()
    {
        var mockDb = MockDb(new[] { Gosa(0) });

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, null, 0);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Множество записей за разные годы — создание на новый год разрешено.
    /// </summary>
    [Fact]
    public void CreateGosa_AmongMultipleYears_ShouldSucceed()
    {
        var mockDb = MockDb(new[]
        {
            Gosa(2020), Gosa(2021), Gosa(2022),
            Gosa(2023), Gosa(2024)
        });

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, null, 2025);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Множество записей за разные годы — создание на занятый год даёт ошибку.
    /// </summary>
    [Fact]
    public void CreateGosa_AmongMultipleYears_DuplicateDetected()
    {
        var mockDb = MockDb(new[]
        {
            Gosa(2020), Gosa(2021), Gosa(2022),
            Gosa(2023), Gosa(2024)
        });

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, null, 2022);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("2022") && e.Contains("уже существует"));
    }

    /// <summary>
    /// При редактировании сброс года в null не должен обнаруживать дубликатов.
    /// </summary>
    [Fact]
    public void EditGosa_ClearYearToNull_ShouldNotDetectDuplicate()
    {
        var meetingId = Guid.NewGuid();
        var mockDb = MockDb(new[] { Gosa(2025, meetingId) });

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, meetingId, null);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// При редактировании сброс года в 0 не должен обнаруживать дубликатов.
    /// </summary>
    [Fact]
    public void EditGosa_ClearYearToZero_ShouldNotDetectDuplicate()
    {
        var meetingId = Guid.NewGuid();
        var mockDb = MockDb(new[] { Gosa(2025, meetingId) });

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, meetingId, 0);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Создание ГОСА на год, который уже занят, с отличающимися полями — всё равно дубликат.
    /// </summary>
    [Fact]
    public void CreateGosa_SameYearDifferentFileds_ShouldBeDetected()
    {
        var mockDb = MockDb(new[]
        {
            new OsaMeeting
            {
                Id = Guid.NewGuid(),
                GosaYear = 2025,
                Title = "Совсем другое ГОСА",
                GosaWindowStart = new DateOnly(2025, 4, 1),
                GosaWindowEnd = new DateOnly(2025, 5, 15)
            }
        });

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, null, 2025);

        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// После удаления ГОСА (запись исключена из БД) год свободен — создание разрешено.
    /// </summary>
    [Fact]
    public void DeleteGosa_ThenRecreateSameYear_ShouldBeAllowed()
    {
        // Удалённая запись не передаётся в mockDb — год свободен
        var mockDb = MockDb(Array.Empty<OsaMeeting>());

        var result = OsaMeetingValidator.ValidateUniqueGosaYear(mockDb.Object, null, 2025);

        result.IsValid.Should().BeTrue();
    }

    // ── helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Создаёт мок IApplicationDbContext с синхронным DbSet, подкреплённым списком.
    /// DbSet реализует IQueryable, поэтому .Any() работает без асинхронного провайдера.
    /// </summary>
    private static Mock<IApplicationDbContext> MockDb(IEnumerable<OsaMeeting> data)
    {
        var queryable = data.AsQueryable();

        var mockSet = new Mock<DbSet<OsaMeeting>>();
        mockSet.As<IQueryable<OsaMeeting>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<OsaMeeting>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<OsaMeeting>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<OsaMeeting>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        var mockDb = new Mock<IApplicationDbContext>();
        mockDb.Setup(db => db.OsaMeetings).Returns(mockSet.Object);

        return mockDb;
    }

    /// <summary>
    /// Создаёт минимальную запись ОСА с заданным годом.
    /// </summary>
    private static OsaMeeting Gosa(int? year, Guid? id = null)
    {
        return new OsaMeeting
        {
            Id = id ?? Guid.NewGuid(),
            OsaFormId = Guid.NewGuid(),
            GosaYear = year,
            Title = year.HasValue && year > 0
                ? $"ГОСА {year}"
                : "ГОСА без года"
        };
    }
}
