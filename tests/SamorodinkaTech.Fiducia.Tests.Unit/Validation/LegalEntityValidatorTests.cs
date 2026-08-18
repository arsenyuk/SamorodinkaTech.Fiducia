using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Validation;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Validation;

/// <summary>
/// Тесты form-валидатора юридического лица: проверяет поля формы
/// без обращения к БД — ОКОПФ, интервал ГОСА, руководитель, типовой устав.
/// Валидация состава СД и количества акционеров вынесена в OsaMeetingValidator.
/// </summary>
public class LegalEntityValidatorTests
{
    // ─── OrgType detection ───────────────────────────────────────────────

    /// <summary>
    /// Определение типа организации по коду ОКОПФ: ПАО, НАО, ООО, неизвестный.
    /// </summary>
    [Theory]
    [InlineData("12247", OrgValidationType.PJSC)]
    [InlineData("12267", OrgValidationType.NJSC)]
    [InlineData("12300", OrgValidationType.LLC)]
    [InlineData("99999", OrgValidationType.Unknown)]
    [InlineData("", OrgValidationType.Unknown)]
    [InlineData(null, OrgValidationType.Unknown)]
    [InlineData("12 247", OrgValidationType.PJSC)]   // пробелы игнорируются
    [InlineData("12247-extra", OrgValidationType.PJSC)] // нецифровые символы игнорируются
    public void DetectOrgType_ShouldIdentifyCorrectly(string? code, OrgValidationType expected)
    {
        LegalEntityValidator.DetectOrgType(code).Should().Be(expected);
    }

    // ─── Valid model: all org types ─────────────────────────────────────

    /// <summary>
    /// ПАО с корректными данными — валидация успешна.
    /// </summary>
    [Fact]
    public void Valid_PAO_ShouldReturnSuccess()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12247",
            HasBoardOfDirectors = true,
            GosaWindowStart = new DateOnly(2025, 3, 1),
            GosaWindowEnd = new DateOnly(2025, 6, 30),
            Position = "Генеральный директор",
            FullName = "Иванов И.И."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// НАО с корректными данными — валидация успешна.
    /// </summary>
    [Fact]
    public void Valid_NAO_ShouldReturnSuccess()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12267",
            HasBoardOfDirectors = true,
            GosaWindowStart = new DateOnly(2025, 3, 1),
            GosaWindowEnd = new DateOnly(2025, 6, 30),
            Position = "Директор",
            FullName = "Петров П.П."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// ООО с корректными данными — валидация успешна.
    /// </summary>
    [Fact]
    public void Valid_LLC_ShouldReturnSuccess()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12300",
            HasBoardOfDirectors = true,
            GosaWindowStart = new DateOnly(2025, 3, 1),
            GosaWindowEnd = new DateOnly(2025, 6, 30),
            Position = "Генеральный директор",
            FullName = "Сидоров С.С."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Совет директоров отключён — проверки состава СД и акционеров пропускаются.
    /// </summary>
    [Fact]
    public void Valid_NoBoard_ShouldSkipBoardChecks()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12300",
            HasBoardOfDirectors = false,
            Position = "Руководитель",
            FullName = "Фёдоров Ф.Ф."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// ПАО с неограниченным количеством акционеров — валидация успешна.
    /// </summary>
    [Fact]
    public void Valid_PAO_UnlimitedShareholders_ShouldPass()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12247",
            HasBoardOfDirectors = true,
            Position = "CEO",
            FullName = "Крупный К.К."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    // ─── GOSA window ────────────────────────────────────────────────────

    /// <summary>
    /// Дата окончания окна ГОСА раньше даты начала — ошибка валидации.
    /// </summary>
    [Fact]
    public void Gosa_EndBeforeStart_ShouldFail()
    {
        var model = ValidPaoModel() with
        {
            GosaWindowStart = new DateOnly(2025, 6, 30),
            GosaWindowEnd = new DateOnly(2025, 3, 1)
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("раньше"));
    }

    /// <summary>
    /// Для ПАО окно ГОСА за пределами законного диапазона 01.03–30.06 — ошибка валидации.
    /// </summary>
    [Fact]
    public void Gosa_PAO_OutsideLegalWindow_ShouldFail()
    {
        var model = ValidPaoModel() with
        {
            GosaWindowStart = new DateOnly(2025, 2, 1),
            GosaWindowEnd = new DateOnly(2025, 3, 15)
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("01.03–30.06"));
    }

    /// <summary>
    /// Для ПАО окно ГОСА внутри законного диапазона 01.03–30.06 — валидация успешна.
    /// </summary>
    [Fact]
    public void Gosa_PAO_WithinLegalWindow_ShouldPass()
    {
        var model = ValidPaoModel() with
        {
            GosaWindowStart = new DateOnly(2025, 4, 1),
            GosaWindowEnd = new DateOnly(2025, 5, 15)
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Для ПАО окно ГОСА точно совпадает с законным диапазоном — валидация успешна.
    /// </summary>
    [Fact]
    public void Gosa_PAO_ExactWindow_ShouldPass()
    {
        var model = ValidPaoModel() with
        {
            GosaWindowStart = new DateOnly(2025, 3, 1),
            GosaWindowEnd = new DateOnly(2025, 6, 30)
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Для НАО окно ГОСА точно совпадает с фиксированным диапазоном — валидация успешна.
    /// </summary>
    [Fact]
    public void Gosa_NAO_ExactWindow_ShouldPass()
    {
        var model = ValidNaoModel() with
        {
            GosaWindowStart = new DateOnly(2025, 3, 1),
            GosaWindowEnd = new DateOnly(2025, 6, 30)
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Для НАО окно ГОСА отличается от фиксированного — ошибка валидации.
    /// </summary>
    [Fact]
    public void Gosa_NAO_CustomWindow_ShouldFail()
    {
        var model = ValidNaoModel() with
        {
            GosaWindowStart = new DateOnly(2025, 4, 1),
            GosaWindowEnd = new DateOnly(2025, 5, 1)
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("фиксирован"));
    }

    // ─── Director fields ────────────────────────────────────────────────

    /// <summary>
    /// ФИО руководителя — пустая строка: ошибка валидации.
    /// </summary>
    [Fact]
    public void Director_EmptyFullName_ShouldFail()
    {
        var model = ValidPaoModel() with { FullName = "" };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ФИО"));
    }

    /// <summary>
    /// ФИО руководителя — null: ошибка валидации.
    /// </summary>
    [Fact]
    public void Director_NullFullName_ShouldFail()
    {
        var model = ValidPaoModel() with { FullName = null };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ФИО"));
    }

    /// <summary>
    /// ФИО руководителя — только пробелы: ошибка валидации.
    /// </summary>
    [Fact]
    public void Director_WhitespaceFullName_ShouldFail()
    {
        var model = ValidPaoModel() with { FullName = "   " };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ФИО"));
    }

    /// <summary>
    /// Должность руководителя — пустая строка: ошибка валидации.
    /// </summary>
    [Fact]
    public void Director_EmptyPosition_ShouldFail()
    {
        var model = ValidPaoModel() with { Position = "" };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("должность"));
    }

    /// <summary>
    /// Должность руководителя — null: ошибка валидации.
    /// </summary>
    [Fact]
    public void Director_NullPosition_ShouldFail()
    {
        var model = ValidPaoModel() with { Position = null };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("должность"));
    }

    // ─── Multiple errors ────────────────────────────────────────────────

    /// <summary>
    /// При нескольких ошибках валидации все они попадают в результат.
    /// </summary>
    [Fact]
    public void MultipleErrors_ShouldAllBeReported()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12267",
            HasBoardOfDirectors = true,
            GosaWindowStart = new DateOnly(2025, 7, 1),
            GosaWindowEnd = new DateOnly(2025, 6, 1),
            Position = null,
            FullName = ""
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    /// <summary>
    /// При отключённом СД ошибки — только по руководителю (ФИО и должность).
    /// </summary>
    [Fact]
    public void EmptyModel_NoBoard_ShouldFailOnlyDirector()
    {
        var model = new LegalEntitySaveValidationModel
        {
            HasBoardOfDirectors = false
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Contains("ФИО"));
        result.Errors.Should().Contain(e => e.Contains("должность"));
    }

    // ─── OrgTypeLabel ───────────────────────────────────────────────────

    /// <summary>
    /// Отображение типа организации в тексте ошибок.
    /// </summary>
    [Theory]
    [InlineData(OrgValidationType.PJSC, "ПАО")]
    [InlineData(OrgValidationType.NJSC, "непубличного АО")]
    [InlineData(OrgValidationType.LLC, "ООО")]
    [InlineData(OrgValidationType.Unknown, "данного типа общества")]
    public void OrgTypeLabel_ShouldReturnCorrect(OrgValidationType type, string expected)
    {
        LegalEntityValidator.OrgTypeLabel(type).Should().Be(expected);
    }

    // ─── Helper factories ───────────────────────────────────────────────

    /// <summary>
    /// Фабрика корректной модели ПАО для тестов.
    /// </summary>
    private static LegalEntitySaveValidationModel ValidPaoModel() => new()
    {
        OkopfCode = "12247",
        HasBoardOfDirectors = true,
        GosaWindowStart = new DateOnly(2025, 3, 1),
        GosaWindowEnd = new DateOnly(2025, 6, 30),
        Position = "Генеральный директор",
        FullName = "Иванов И.И."
    };

    /// <summary>
    /// Фабрика корректной модели НАО для тестов.
    /// </summary>
    private static LegalEntitySaveValidationModel ValidNaoModel() => new()
    {
        OkopfCode = "12267",
        HasBoardOfDirectors = true,
        GosaWindowStart = new DateOnly(2025, 3, 1),
        GosaWindowEnd = new DateOnly(2025, 6, 30),
        Position = "CEO",
        FullName = "Сидоров С.С."
    };
}
