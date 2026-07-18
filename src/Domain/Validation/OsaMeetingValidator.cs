namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Серверный валидатор сохранения ОСА (BDR‑008).
/// Изолирован от окружения: не зависит от БД, DI, HTTP-контекста.
/// </summary>
public static class OsaMeetingValidator
{
    public const string PaoCode = "12247";
    public const int MaxShareholdersForNonPao = 50;

    public static LegalEntitySaveValidationResult Validate(OsaMeetingValidationModel model)
    {
        var result = new LegalEntitySaveValidationResult();
        var orgType = LegalEntityValidator.DetectOrgType(model.OkopfCode);

        ValidateAbsenteeConflict(model, result);
        ValidateShareholdersCount(model, orgType, result);
        ValidateBoardMembers(model, orgType, result);
        ValidateBoardMandatory(model, orgType, result);
        ValidateGosaYear(model, result);
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

        var mandatory = orgType == OrgValidationType.PAO
            || (orgType == OrgValidationType.NAO_AO && model.ShareholdersCount.Value >= MaxShareholdersForNonPao);
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

        if (orgType != OrgValidationType.PAO && model.ShareholdersCount.Value > MaxShareholdersForNonPao)
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

    private static void ValidateGosaYear(
        OsaMeetingValidationModel model,
        LegalEntitySaveValidationResult result)
    {
        if (!model.GosaYear.HasValue || model.GosaYear.Value <= 0)
            return;

        // Проверка: год должен быть разумным (не раньше основания АО и не в далёком будущем)
        if (model.GosaYear.Value < 1990 || model.GosaYear.Value > DateTime.UtcNow.Year + 5)
            result.AddError(
                $"Год проведения ГОСА ({model.GosaYear.Value}) вне допустимого диапазона.");
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
}
