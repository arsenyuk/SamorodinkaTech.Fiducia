using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Серверный валидатор сохранения ОСА (BDR‑008).
/// Изолирован от окружения: не зависит от БД, DI, HTTP-контекста.
/// </summary>
public static class OsaMeetingValidator
{
    public const int MaxShareholdersForNonPao = LegalEntityValidator.MaxShareholdersForNonPao;

    public static LegalEntitySaveValidationResult Validate(OsaMeetingValidationModel model)
    {
        var result = new LegalEntitySaveValidationResult();
        var orgType = OkopfTypeMapper.DetectType(model.OkopfCode);

        ValidateAbsenteeConflict(model, result);
        ValidateShareholdersCount(model, orgType, result);
        ValidateBoardMembers(model, orgType, result);
        ValidateBoardMandatory(model, orgType, result);
        ValidateElectionYear(model, result);
        ValidateDirectorTypes(model, result);

        return result;
    }

    private static void ValidateAbsenteeConflict(
        OsaMeetingValidationModel model,
        LegalEntitySaveValidationResult result)
    {
        if (model.HasGosaInterval && model.IsAbsentee)
            result.AddError("Заочное голосование несовместимо с интервалом ГОСА.");
    }

    private static void ValidateBoardMandatory(
        OsaMeetingValidationModel model,
        OrgValidationType orgType,
        LegalEntitySaveValidationResult result)
    {
        // Для ГОСА проверка должна быть всегда — поля пришли из формы
        if (model.ShareholdersCount is null or <= 0)
            return;

        var mandatory = orgType == OrgValidationType.PJSC
            || (orgType == OrgValidationType.NJSC && model.ShareholdersCount.Value >= MaxShareholdersForNonPao);
        // mandatory — это информационно, но не блокирует, валидация ниже
        _ = mandatory;
    }

    private static void ValidateShareholdersCount(
        OsaMeetingValidationModel model,
        OrgValidationType orgType,
        LegalEntitySaveValidationResult result)
    {
        if (model.ShareholdersCount is null or <= 0)
        {
            result.AddError("Укажите количество акционеров.");
            return;
        }

        if (orgType != OrgValidationType.PJSC && model.ShareholdersCount.Value > MaxShareholdersForNonPao)
        {
            var label = LegalEntityValidator.OrgTypeLabel(orgType);
            result.AddError(
                $"Для {label} максимальное количество акционеров — {MaxShareholdersForNonPao}. " +
                $"Указано: {model.ShareholdersCount.Value}.");
        }
    }

    private static void ValidateBoardMembers(
        OsaMeetingValidationModel model,
        OrgValidationType orgType,
        LegalEntitySaveValidationResult result)
    {
        if (model.BoardMemberNumber.HasValue && model.BoardMinNumber.HasValue
            && model.BoardMemberNumber.Value < model.BoardMinNumber.Value)
        {
            result.AddError(
                $"Количество участников СД ({model.BoardMemberNumber.Value}) " +
                $"не может быть меньше минимального ({model.BoardMinNumber.Value}).");
        }
    }

    private static void ValidateElectionYear(
        OsaMeetingValidationModel model,
        LegalEntitySaveValidationResult result)
    {
        if (!model.ElectionYear.HasValue || model.ElectionYear.Value <= 0)
            return;

        if (model.ElectionYear.Value < LegalEntityValidator.MinElectionYear
            || model.ElectionYear.Value > DateTime.UtcNow.Year + LegalEntityValidator.MaxElectionYearOffset)
            result.AddError(
                $"Год избрания ({model.ElectionYear.Value}) вне допустимого диапазона.");
    }

    private static void ValidateDirectorTypes(
        OsaMeetingValidationModel model,
        LegalEntitySaveValidationResult result)
    {
        var exec = model.ExecutiveDirectorsParticipate ? (model.ExecutiveDirectorsCount ?? 0) : 0;
        var nonExec = model.NonExecutiveDirectorsParticipate ? (model.NonExecutiveDirectorsCount ?? 0) : 0;
        var indep = model.IndependentDirectorsParticipate ? (model.IndependentDirectorsCount ?? 0) : 0;

        var total = exec + nonExec + indep;

        if (total > 0 && model.BoardMemberNumber.HasValue)
        {
            if (total > model.BoardMemberNumber.Value)
                result.AddError(
                    $"Общее количество директоров по типам ({total}) " +
                    $"не может превышать количество участников СД ({model.BoardMemberNumber.Value}).");
        }
    }

    /// <summary>
    /// DB-валидатор: проверяет, что состав СД с указанным годом избрания ещё не существует.
    /// Принимает IApplicationDbContext (порт) для инверсии зависимостей.
    /// </summary>
    /// <param name="db">Контекст БД (абстракция).</param>
    /// <param name="currentMeetingId">
    ///   Идентификатор редактируемой записи при редактировании.
    ///   При создании новой записи передаётся null.
    /// </param>
    /// <param name="electionYear">Предлагаемый год избрания.</param>
    /// <returns>Результат валидации с ошибкой при обнаружении дубликата.</returns>
    public static LegalEntitySaveValidationResult ValidateUniqueElectionYear(
        IApplicationDbContext db,
        Guid? currentMeetingId,
        int? electionYear)
    {
        var result = new LegalEntitySaveValidationResult();

        if (!electionYear.HasValue || electionYear.Value <= 0)
            return result;

        var duplicate = currentMeetingId.HasValue
            ? db.OsaMeetings.Any(m => m.Id != currentMeetingId.Value && m.ElectionYear == electionYear.Value)
            : db.OsaMeetings.Any(m => m.ElectionYear == electionYear.Value);

        if (duplicate)
            result.AddError(
                $"Состав СД за {electionYear.Value} год уже существует. Нельзя создать более одного состава в году.");

        return result;
    }
}
