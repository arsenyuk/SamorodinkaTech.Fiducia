using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Validation;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Validation;

/// <summary>
/// Тесты form-валидатора ОСА: проверяет значения полей формы
/// без обращения к БД — диапазоны, обязательность, совместимость.
/// </summary>
public class OsaMeetingValidatorTests
{
    /// <summary>
    /// ПАО с допустимым количеством акционеров и членов СД — валидация успешна.
    /// </summary>
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

    /// <summary>
    /// НАО с допустимым количеством акционеров и членов СД — валидация успешна.
    /// </summary>
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

    /// <summary>
    /// ООО с допустимым количеством участников и членов СД — валидация успешна.
    /// </summary>
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

    /// <summary>
    /// ПАО с неограниченным количеством акционеров (500 000) — валидация успешна.
    /// </summary>
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

    /// <summary>
    /// Количество акционеров не указано — ошибка валидации.
    /// </summary>
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

    /// <summary>
    /// Количество акционеров равно нулю — ошибка валидации.
    /// </summary>
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

    /// <summary>
    /// Отрицательное количество акционеров — ошибка валидации.
    /// </summary>
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

    /// <summary>
    /// Для не-ПАО количество акционеров/участников не может превышать 50.
    /// </summary>
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

    /// <summary>
    /// Для не-ПАО количество акционеров ≤ 50 — валидация успешна.
    /// </summary>
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

    /// <summary>
    /// НАО с ровно 50 акционерами и полным набором корректных полей — валидация успешна.
    /// </summary>
    [Fact]
    public void NAO_Exactly50Shareholders_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12267",
            ShareholdersCount = 50,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            HasGosaInterval = false,
            IsAbsentee = false,
            GosaYear = DateTime.UtcNow.Year,
            ExecutiveDirectorsParticipate = true,
            ExecutiveDirectorsCount = 3,
            NonExecutiveDirectorsParticipate = true,
            NonExecutiveDirectorsCount = 2,
            IndependentDirectorsParticipate = true,
            IndependentDirectorsCount = 2
        };

        var result = OsaMeetingValidator.Validate(model);

        // 50 акционеров — граница для НАО, все поля корректны
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Количество членов СД меньше минимального — ошибка валидации.
    /// </summary>
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

    /// <summary>
    /// Количество членов СД равно минимальному — валидация успешна.
    /// </summary>
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

    /// <summary>
    /// Количество членов СД больше минимального — валидация успешна.
    /// </summary>
    [Fact]
    public void BoardMembers_AboveMinimum_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 11
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Минимальное количество членов СД не задано — проверка пропускается.
    /// </summary>
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

    /// <summary>
    /// Количество членов СД не задано — проверка пропускается.
    /// </summary>
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

    /// <summary>
    /// При нескольких ошибках валидации все они попадают в результат.
    /// </summary>
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

    /// <summary>
    /// Неизвестный ОКОПФ — валидация успешна (неизвестный тип не имеет жёстких ограничений).
    /// </summary>
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

    /// <summary>
    /// ОКОПФ не указан — валидация успешна (отсутствие кода не создаёт ограничений).
    /// </summary>
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

    /// <summary>
    /// Заочное голосование несовместимо с наличием интервала ГОСА — ошибка валидации.
    /// </summary>
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

    /// <summary>
    /// Заочное голосование без интервала ГОСА — валидация успешна.
    /// </summary>
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

    /// <summary>
    /// Интервал ГОСА без заочного голосования — валидация успешна.
    /// </summary>
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

    // ── GosaYear ────────────────────────────────────────────────────────

    /// <summary>
    /// Год ГОСА равен текущему году — валидация успешна.
    /// </summary>
    [Fact]
    public void GosaYear_ValidCurrentYear_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = DateTime.UtcNow.Year
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Год ГОСА равен следующему году — валидация успешна.
    /// </summary>
    [Fact]
    public void GosaYear_NextYear_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = DateTime.UtcNow.Year + 1
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Год ГОСА равен текущему году + 2 — валидация успешна.
    /// </summary>
    [Fact]
    public void GosaYear_TwoYearsAhead_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = DateTime.UtcNow.Year + 2
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Год ГОСА меньше 1990 — ошибка валидации (выход за нижнюю границу диапазона).
    /// </summary>
    [Fact]
    public void GosaYear_Below1990_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = 1989
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("вне допустимого диапазона"));
    }

    /// <summary>
    /// Год ГОСА равен ровно 1990 (нижняя граница) — валидация успешна.
    /// </summary>
    [Fact]
    public void GosaYear_Exactly1990_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = 1990
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Год ГОСА равен текущему + 10 (слишком далеко в будущем) — ошибка валидации.
    /// </summary>
    [Fact]
    public void GosaYear_FarFuture_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = DateTime.UtcNow.Year + 10
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Год ГОСА не указан (null) — проверка пропускается.
    /// </summary>
    [Fact]
    public void GosaYear_Null_ShouldSkipCheck()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = null
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Год ГОСА равен нулю — проверка пропускается (ноль эквивалентен отсутствию).
    /// </summary>
    [Fact]
    public void GosaYear_Zero_ShouldSkipCheck()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = 0
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Год ГОСА равен текущему + 5 (граница допустимого) — валидация успешна.
    /// </summary>
    [Fact]
    public void GosaYear_BoundaryPlus5_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = DateTime.UtcNow.Year + 5
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Год ГОСА равен текущему + 6 (за границей допустимого) — ошибка валидации.
    /// </summary>
    [Fact]
    public void GosaYear_BoundaryPlus6_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = DateTime.UtcNow.Year + 6
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("вне допустимого диапазона");
    }

    /// <summary>
    /// Год ГОСА равен прошлому году — валидация успешна.
    /// </summary>
    [Fact]
    public void GosaYear_PastYear_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            GosaYear = DateTime.UtcNow.Year - 1
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Нет ни заочного голосования, ни интервала ГОСА — валидация успешна.
    /// </summary>
    [Fact]
    public void AbsenteeNeither_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            HasGosaInterval = false,
            IsAbsentee = false
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Комбинация ошибок по году ГОСА и числу членов СД — обе ошибки попадают в результат.
    /// </summary>
    [Fact]
    public void Combined_GosaYearPlusBoardMembers_ShouldReportAllErrors()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 2,
            GosaYear = DateTime.UtcNow.Year + 10
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Contains("вне допустимого диапазона"));
        result.Errors.Should().Contain(e => e.Contains("меньше минимального"));
    }

    // ── Negative: non-PAO org types (NAO / LLC) ─────────────────────────

    /// <summary>
    /// НАО: количество акционеров не указано — ошибка валидации.
    /// </summary>
    [Fact]
    public void NAO_Shareholders_Null_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12267",
            ShareholdersCount = null,
            BoardMinNumber = 3,
            BoardMemberNumber = 5
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Укажите количество акционеров"));
    }

    /// <summary>
    /// ООО: количество участников равно нулю — ошибка валидации.
    /// </summary>
    [Fact]
    public void LLC_Shareholders_Zero_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12300",
            ShareholdersCount = 0
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Укажите количество акционеров"));
    }

    /// <summary>
    /// НАО: отрицательное количество акционеров — ошибка валидации.
    /// </summary>
    [Fact]
    public void NAO_Shareholders_Negative_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12267",
            ShareholdersCount = -1
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("акционеров"));
    }

    /// <summary>
    /// НАО: количество членов СД меньше минимального — ошибка валидации.
    /// </summary>
    [Fact]
    public void NAO_BoardMembers_BelowMinimum_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12267",
            ShareholdersCount = 30,
            BoardMinNumber = 5,
            BoardMemberNumber = 3
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("меньше минимального"));
    }

    /// <summary>
    /// ООО: количество членов СД меньше минимального — ошибка валидации.
    /// </summary>
    [Fact]
    public void LLC_BoardMembers_BelowMinimum_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12300",
            ShareholdersCount = 15,
            BoardMinNumber = 3,
            BoardMemberNumber = 2
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("меньше минимального"));
    }

    /// <summary>
    /// НАО: заочное голосование несовместимо с интервалом ГОСА — ошибка валидации.
    /// </summary>
    [Fact]
    public void NAO_AbsenteeWithGosaInterval_ShouldReject()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12267",
            ShareholdersCount = 30,
            BoardMinNumber = 3,
            BoardMemberNumber = 5,
            HasGosaInterval = true,
            IsAbsentee = true
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("несовместимо"));
    }

    /// <summary>
    /// ООО: заочное голосование несовместимо с интервалом ГОСА — ошибка валидации.
    /// </summary>
    [Fact]
    public void LLC_AbsenteeWithGosaInterval_ShouldReject()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12300",
            ShareholdersCount = 10,
            BoardMinNumber = 2,
            BoardMemberNumber = 3,
            HasGosaInterval = true,
            IsAbsentee = true
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("несовместимо"));
    }

    // ── Smoke: fully populated valid models ─────────────────────────────

    /// <summary>
    /// ПАО со всеми заполненными полями, все значения корректны — валидация успешна.
    /// </summary>
    [Fact]
    public void Valid_PAO_FullModel_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 500,
            BoardMinNumber = 7,
            BoardMemberNumber = 9,
            HasGosaInterval = true,
            IsAbsentee = false,
            GosaYear = DateTime.UtcNow.Year,
            ExecutiveDirectorsParticipate = true,
            ExecutiveDirectorsCount = 3,
            NonExecutiveDirectorsParticipate = true,
            NonExecutiveDirectorsCount = 3,
            IndependentDirectorsParticipate = true,
            IndependentDirectorsCount = 3
        };

        var result = OsaMeetingValidator.Validate(model);

        // 3+3+3 = 9 ≤ 9, ПАО без ограничения акционеров, год в диапазоне
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// ООО со всеми заполненными полями, все значения корректны — валидация успешна.
    /// </summary>
    [Fact]
    public void Valid_LLC_FullModel_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12300",
            ShareholdersCount = 30,
            BoardMinNumber = 3,
            BoardMemberNumber = 5,
            HasGosaInterval = true,
            IsAbsentee = false,
            GosaYear = DateTime.UtcNow.Year + 1,
            ExecutiveDirectorsParticipate = true,
            ExecutiveDirectorsCount = 2,
            NonExecutiveDirectorsParticipate = true,
            NonExecutiveDirectorsCount = 2,
            IndependentDirectorsParticipate = true,
            IndependentDirectorsCount = 1
        };

        var result = OsaMeetingValidator.Validate(model);

        // 2+2+1 = 5 ≤ 5, ООО ≤ 50 участников, год +1 в диапазоне
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // ── Director Types ──────────────────────────────────────────────────

    /// <summary>
    /// Сумма директоров по типам не превышает общее количество участников СД — валидация успешна.
    /// </summary>
    [Fact]
    public void DirectorTypes_TotalWithinLimit_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            ExecutiveDirectorsParticipate = true,
            ExecutiveDirectorsCount = 2,
            NonExecutiveDirectorsParticipate = true,
            NonExecutiveDirectorsCount = 2,
            IndependentDirectorsParticipate = true,
            IndependentDirectorsCount = 3
        };

        var result = OsaMeetingValidator.Validate(model);

        // 2+2+3 = 7 ≤ 7 → ок
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Сумма директоров по типам превышает общее количество участников СД — ошибка валидации.
    /// </summary>
    [Fact]
    public void DirectorTypes_TotalExceedsBoardMembers_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 5,
            ExecutiveDirectorsParticipate = true,
            ExecutiveDirectorsCount = 2,
            NonExecutiveDirectorsParticipate = true,
            NonExecutiveDirectorsCount = 3,
            IndependentDirectorsParticipate = true,
            IndependentDirectorsCount = 2
        };

        var result = OsaMeetingValidator.Validate(model);

        // 2+3+2 = 7 > 5 → ошибка
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("не может превышать количество участников СД"));
    }

    /// <summary>
    /// Ни один тип директоров не выбран — валидация успешна.
    /// </summary>
    [Fact]
    public void DirectorTypes_NoneSelected_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 5,
            ExecutiveDirectorsParticipate = false,
            NonExecutiveDirectorsParticipate = false,
            IndependentDirectorsParticipate = false
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Выбрано два из трёх типов директоров, сумма в пределах лимита — валидация успешна.
    /// </summary>
    [Fact]
    public void DirectorTypes_TwoOfThreeTypes_WithinLimit_ShouldPass()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            ExecutiveDirectorsParticipate = true,
            ExecutiveDirectorsCount = 4,
            NonExecutiveDirectorsParticipate = true,
            NonExecutiveDirectorsCount = 2,
            IndependentDirectorsParticipate = false
        };

        var result = OsaMeetingValidator.Validate(model);

        // 4+2=6 ≤ 7 → ок
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Выбран только один тип директоров с количеством, превышающим лимит — ошибка валидации.
    /// </summary>
    [Fact]
    public void DirectorTypes_SingleTypeExceeds_ShouldFail()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 3,
            BoardMemberNumber = 3,
            ExecutiveDirectorsParticipate = true,
            ExecutiveDirectorsCount = 5,
            NonExecutiveDirectorsParticipate = false,
            IndependentDirectorsParticipate = false
        };

        var result = OsaMeetingValidator.Validate(model);

        // 5 > 3 → ошибка
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("не может превышать количество участников СД"));
        result.Errors.Should().Contain(e => e.Contains("5") && e.Contains("3"));
    }

    /// <summary>
    /// Флаг участия включён, но количество равно 0 — директора этого типа не учитываются.
    /// </summary>
    [Fact]
    public void DirectorTypes_ParticipateTrueCountZero_ShouldNotCount()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 7,
            ExecutiveDirectorsParticipate = true,
            ExecutiveDirectorsCount = 0,
            NonExecutiveDirectorsParticipate = false,
            IndependentDirectorsParticipate = false
        };

        var result = OsaMeetingValidator.Validate(model);

        // total = 0, лимит не проверяется
        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Общее количество директоров по типам > 0, но BoardMemberNumber не задан — проверка пропускается.
    /// </summary>
    [Fact]
    public void DirectorTypes_BoardMemberNumberNull_ShouldSkipCheck()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = null,
            ExecutiveDirectorsParticipate = true,
            ExecutiveDirectorsCount = 3
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    /// <summary>
    /// Заочное голосование несовместимо с интервалом ГОСА + количество акционеров не указано.
    /// </summary>
    [Fact]
    public void Combined_AbsenteePlusShareholdersNull_ShouldReportBoth()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = null,
            HasGosaInterval = true,
            IsAbsentee = true
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Contains("несовместимо"));
        result.Errors.Should().Contain(e => e.Contains("количество акционеров"));
    }

    /// <summary>
    /// Сумма директоров по типам превышает лимит + год ГОСА вне диапазона — обе ошибки в результате.
    /// </summary>
    [Fact]
    public void Combined_DirectorTypesPlusGosaYear_ShouldReportBoth()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12247",
            ShareholdersCount = 100,
            BoardMinNumber = 5,
            BoardMemberNumber = 5,
            ExecutiveDirectorsParticipate = true,
            ExecutiveDirectorsCount = 3,
            NonExecutiveDirectorsParticipate = true,
            NonExecutiveDirectorsCount = 3,
            IndependentDirectorsParticipate = false,
            GosaYear = DateTime.UtcNow.Year + 6
        };

        var result = OsaMeetingValidator.Validate(model);

        // 3+3=6 > 5 — ошибка директоров, год +6 — ошибка года
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Contains("не может превышать количество участников СД"));
        result.Errors.Should().Contain(e => e.Contains("вне допустимого диапазона"));
    }

    /// <summary>
    /// Не-ПАО с акционерами > 50 и заочным конфликтом — обе ошибки в результате.
    /// </summary>
    [Fact]
    public void Combined_NonPaoShareholdersPlusAbsentee_ShouldReportBoth()
    {
        var model = new OsaMeetingValidationModel
        {
            OkopfCode = "12267",
            ShareholdersCount = 55,
            HasGosaInterval = true,
            IsAbsentee = true
        };

        var result = OsaMeetingValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Errors.Should().Contain(e => e.Contains("несовместимо"));
        result.Errors.Should().Contain(e => e.Contains("50"));
    }
}
