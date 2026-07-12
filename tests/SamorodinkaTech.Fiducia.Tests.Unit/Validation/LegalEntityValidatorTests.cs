using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Validation;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Validation;

public class LegalEntityValidatorTests
{
    // ─── OrgType detection ───────────────────────────────────────────────

    [Theory]
    [InlineData("12247", OrgValidationType.PAO)]
    [InlineData("12267", OrgValidationType.NAO_AO)]
    [InlineData("12260", OrgValidationType.NAO_AO)]
    [InlineData("12300", OrgValidationType.LLC)]
    [InlineData("99999", OrgValidationType.Unknown)]
    [InlineData("", OrgValidationType.Unknown)]
    [InlineData(null, OrgValidationType.Unknown)]
    [InlineData("12 247", OrgValidationType.PAO)]   // пробелы игнорируются
    [InlineData("12247-extra", OrgValidationType.PAO)] // нецифровые символы игнорируются
    public void DetectOrgType_ShouldIdentifyCorrectly(string? code, OrgValidationType expected)
    {
        LegalEntityValidator.DetectOrgType(code).Should().Be(expected);
    }

    // ─── Valid model: all org types ─────────────────────────────────────

    [Fact]
    public void Valid_PAO_ShouldReturnSuccess()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12247",
            HasBoardOfDirectors = true,
            ShareholdersCount = 1000,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaWindowStart = new DateOnly(2025, 3, 1),
            GosaWindowEnd = new DateOnly(2025, 6, 30),
            Position = "Генеральный директор",
            FullName = "Иванов И.И."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Valid_NAO_ShouldReturnSuccess()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12267",
            HasBoardOfDirectors = true,
            ShareholdersCount = 30,
            BoardMinNumber = 3,
            BoardMemberNumber = 5,
            GosaWindowStart = new DateOnly(2025, 3, 1),
            GosaWindowEnd = new DateOnly(2025, 6, 30),
            Position = "Директор",
            FullName = "Петров П.П."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_LLC_ShouldReturnSuccess()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12300",
            HasBoardOfDirectors = true,
            ShareholdersCount = 10,
            BoardMinNumber = 2,
            BoardMemberNumber = 3,
            GosaWindowStart = new DateOnly(2025, 3, 1),
            GosaWindowEnd = new DateOnly(2025, 6, 30),
            Position = "Генеральный директор",
            FullName = "Сидоров С.С."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

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

    [Fact]
    public void Valid_PAO_UnlimitedShareholders_ShouldPass()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12247",
            HasBoardOfDirectors = true,
            ShareholdersCount = 500_000,
            BoardMinNumber = 9,
            BoardMemberNumber = 11,
            Position = "CEO",
            FullName = "Крупный К.К."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    // ─── Shareholders count ─────────────────────────────────────────────

    [Fact]
    public void Shareholders_Null_WhenBoardEnabled_ShouldFail()
    {
        var model = ValidPaoModel() with { ShareholdersCount = null };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("количество акционеров"));
    }

    [Fact]
    public void Shareholders_Zero_WhenBoardEnabled_ShouldFail()
    {
        var model = ValidPaoModel() with { ShareholdersCount = 0 };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("количество акционеров"));
    }

    [Fact]
    public void Shareholders_Negative_WhenBoardEnabled_ShouldFail()
    {
        var model = ValidPaoModel() with { ShareholdersCount = -5 };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("количество акционеров"));
    }

    [Fact]
    public void Shareholders_Null_WhenBoardDisabled_ShouldPass()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12300",
            HasBoardOfDirectors = false,
            ShareholdersCount = null,
            Position = "CEO",
            FullName = "Иванов И.И."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    // ─── Max 50 for Non-PAO ────────────────────────────────────────────

    [Theory]
    [InlineData("12267", 51)]
    [InlineData("12267", 100)]
    [InlineData("12260", 60)]
    [InlineData("12300", 51)]
    [InlineData("12300", 100)]
    public void NonPao_ShareholdersAbove50_ShouldFail(string okopfCode, int count)
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = okopfCode,
            HasBoardOfDirectors = true,
            ShareholdersCount = count,
            BoardMinNumber = 3,
            BoardMemberNumber = 5,
            Position = "CEO",
            FullName = "Иванов И.И."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("50") && e.Contains(count.ToString()));
    }

    [Theory]
    [InlineData("12267", 50)]
    [InlineData("12267", 1)]
    [InlineData("12300", 50)]
    public void NonPao_ShareholdersAtOrBelow50_ShouldPass(string okopfCode, int count)
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = okopfCode,
            HasBoardOfDirectors = true,
            ShareholdersCount = count,
            BoardMinNumber = count >= 50 ? 3 : 2,
            BoardMemberNumber = count >= 50 ? 5 : 3,
            Position = "Директор",
            FullName = "Петров П.П."
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    // ─── Board member number vs minimum ────────────────────────────────

    [Fact]
    public void BoardMembers_BelowMinimum_ShouldFail()
    {
        var model = ValidPaoModel() with { BoardMinNumber = 5, BoardMemberNumber = 3 };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.Contains("3") && e.Contains("5") && e.Contains("меньше минимального"));
    }

    [Fact]
    public void BoardMembers_EqualsMinimum_ShouldPass()
    {
        var model = ValidPaoModel() with { BoardMinNumber = 7, BoardMemberNumber = 7 };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void BoardMembers_AboveMinimum_ShouldPass()
    {
        var model = ValidPaoModel() with { BoardMinNumber = 5, BoardMemberNumber = 11 };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void BoardMembers_MinNull_ShouldSkipCheck()
    {
        var model = ValidPaoModel() with { BoardMinNumber = null, BoardMemberNumber = 3 };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void BoardMembers_MemberNull_ShouldSkipCheck()
    {
        var model = ValidPaoModel() with { BoardMinNumber = 5, BoardMemberNumber = null };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    // ─── GOSA window ────────────────────────────────────────────────────

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

    [Fact]
    public void Director_EmptyFullName_ShouldFail()
    {
        var model = ValidPaoModel() with { FullName = "" };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ФИО"));
    }

    [Fact]
    public void Director_NullFullName_ShouldFail()
    {
        var model = ValidPaoModel() with { FullName = null };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ФИО"));
    }

    [Fact]
    public void Director_WhitespaceFullName_ShouldFail()
    {
        var model = ValidPaoModel() with { FullName = "   " };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ФИО"));
    }

    [Fact]
    public void Director_EmptyPosition_ShouldFail()
    {
        var model = ValidPaoModel() with { Position = "" };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("должность"));
    }

    [Fact]
    public void Director_NullPosition_ShouldFail()
    {
        var model = ValidPaoModel() with { Position = null };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("должность"));
    }

    // ─── Multiple errors ────────────────────────────────────────────────

    [Fact]
    public void MultipleErrors_ShouldAllBeReported()
    {
        var model = new LegalEntitySaveValidationModel
        {
            OkopfCode = "12267",
            HasBoardOfDirectors = true,
            ShareholdersCount = null,
            BoardMinNumber = 5,
            BoardMemberNumber = 2,
            GosaWindowStart = new DateOnly(2025, 7, 1),
            GosaWindowEnd = new DateOnly(2025, 6, 1),
            Position = null,
            FullName = ""
        };

        var result = LegalEntityValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(4);
    }

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

    [Theory]
    [InlineData(OrgValidationType.PAO, "ПАО")]
    [InlineData(OrgValidationType.NAO_AO, "непубличного АО")]
    [InlineData(OrgValidationType.LLC, "ООО")]
    [InlineData(OrgValidationType.Unknown, "данного типа общества")]
    public void OrgTypeLabel_ShouldReturnCorrect(OrgValidationType type, string expected)
    {
        LegalEntityValidator.OrgTypeLabel(type).Should().Be(expected);
    }

    // ─── Helper factories ───────────────────────────────────────────────

    private static LegalEntitySaveValidationModel ValidPaoModel() => new()
    {
        OkopfCode = "12247",
        HasBoardOfDirectors = true,
        ShareholdersCount = 100,
        BoardMinNumber = 5,
        BoardMemberNumber = 7,
        GosaWindowStart = new DateOnly(2025, 3, 1),
        GosaWindowEnd = new DateOnly(2025, 6, 30),
        Position = "Генеральный директор",
        FullName = "Иванов И.И."
    };

    private static LegalEntitySaveValidationModel ValidNaoModel() => new()
    {
        OkopfCode = "12267",
        HasBoardOfDirectors = true,
        ShareholdersCount = 10,
        BoardMinNumber = 3,
        BoardMemberNumber = 5,
        GosaWindowStart = new DateOnly(2025, 3, 1),
        GosaWindowEnd = new DateOnly(2025, 6, 30),
        Position = "CEO",
        FullName = "Сидоров С.С."
    };
}
