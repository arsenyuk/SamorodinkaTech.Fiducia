using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Validation;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Validation;

public class OsaMeetingValidatorTests
{
    [Fact]
    public void Valid_PAO_ShouldReturnSuccess()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Valid_NAO_ShouldReturnSuccess()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12267",
            ShareholdersCount = 30,
            BoardMinNumber = 3,
            BoardMemberNumber = 5
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_LLC_ShouldReturnSuccess()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12300",
            ShareholdersCount = 10,
            BoardMinNumber = 2,
            BoardMemberNumber = 3
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_PAO_UnlimitedShareholders_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 500_000,
            BoardMinNumber = 9,
            BoardMemberNumber = 11
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Shareholders_Null_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = null
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("количество акционеров"));
    }

    [Fact]
    public void Shareholders_Zero_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 0
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Shareholders_Negative_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = -5
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("12267", 51)]
    [InlineData("12267", 100)]
    [InlineData("12260", 60)]
    [InlineData("12300", 51)]
    [InlineData("12300", 100)]
    public void NonPao_ShareholdersAbove50_ShouldFail(string okopfCode, int count)
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = okopfCode,
            ShareholdersCount = count
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("50"));
    }

    [Theory]
    [InlineData("12267", 50)]
    [InlineData("12267", 1)]
    [InlineData("12300", 50)]
    public void NonPao_ShareholdersAtOrBelow50_ShouldPass(string okopfCode, int count)
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = okopfCode,
            ShareholdersCount = count
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void BoardMembers_BelowMinimum_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 3
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("3") && e.Contains("5") && e.Contains("меньше минимального"));
    }

    [Fact]
    public void BoardMembers_EqualsMinimum_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 7,
            BoardMemberNumber = 7
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void BoardMembers_MinNull_ShouldSkipCheck()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = null,
            BoardMemberNumber = 3
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void BoardMembers_MemberNull_ShouldSkipCheck()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = null
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void MultipleErrors_ShouldAllBeReported()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12267",
            ShareholdersCount = null,
            BoardMinNumber = 5,
            BoardMemberNumber = 2
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void UnknownOkopf_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "99999",
            ShareholdersCount = 30,
            BoardMinNumber = 3,
            BoardMemberNumber = 5
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void NullOkopf_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = null,
            ShareholdersCount = 30,
            BoardMinNumber = 3,
            BoardMemberNumber = 5
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AbsenteeWithGosaInterval_ShouldReject()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            HasGosaInterval = true,
            IsAbsentee = true
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("несовместимо"));
    }

    [Fact]
    public void AbsenteeWithoutGosaInterval_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            HasGosaInterval = false,
            IsAbsentee = true
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GosaIntervalWithoutAbsentee_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            HasGosaInterval = true,
            IsAbsentee = false
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }
}
