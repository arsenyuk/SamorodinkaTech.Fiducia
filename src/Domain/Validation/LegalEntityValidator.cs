namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Серверный валидатор сохранения ЮЛ (BDR‑008).
/// Полностью изолирован от окружения: не зависит от БД, DI, HTTP-контекста.
/// Пригоден для unit-тестирования без моков.
/// </summary>
public static class LegalEntityValidator
{
    /// <summary>Максимальное количество акционеров (участников) для НАО, АО, ООО.</summary>
    public const int MaxShareholdersForNonPao = 50;

    /// <summary>Минимально допустимый год избрания совета директоров.</summary>
    public const int MinElectionYear = 1990;

    /// <summary>Максимальное смещение года избрания вперёд от текущего года.</summary>
    public const int MaxElectionYearOffset = 5;

    /// <summary>Максимальный номер типового устава ООО (1–36).</summary>
    public const int MaxStandardCharterNumber = 36;

    /// <summary>
    /// Выполняет полную серверную валидацию данных ЮЛ перед сохранением.
    /// </summary>
    /// <param name="model">Данные формы ЮЛ.</param>
    /// <returns>Результат валидации со списком ошибок (пуст — успех).</returns>
    public static LegalEntitySaveValidationResult Validate(LegalEntitySaveValidationModel model)
    {
        var result = new LegalEntitySaveValidationResult();

        var orgType = OkopfTypeMapper.DetectType(model.OkopfCode);

        ValidateShareholders(model, orgType, result);
        ValidateBoardMembers(model, result);
        ValidateGosaWindow(model, orgType, result);
        ValidateDirector(model, result);
        ValidateStandardCharter(model, orgType, result);

        return result;
    }

    private static void ValidateShareholders(
        LegalEntitySaveValidationModel model,
        OrgValidationType orgType,
        LegalEntitySaveValidationResult result)
    {
        if (!model.HasBoardOfDirectors)
            return;

        // Для ООО количество участников не обязательно (нет публичного обращения акций)
        if (orgType == OrgValidationType.LLC)
            return;

        if (!model.ShareholdersCount.HasValue)
        {
            result.AddError("Укажите количество акционеров (участников).");
            return;
        }

        if (model.ShareholdersCount.Value <= 0)
        {
            result.AddError("количество акционеров (участников) должно быть больше нуля.");
            return;
        }

        if (orgType != OrgValidationType.PJSC && model.ShareholdersCount.Value > MaxShareholdersForNonPao)
        {
            result.AddError(
                $"Для {OkopfTypeMapper.TypeLabel(orgType)} количество акционеров (участников) не может превышать {MaxShareholdersForNonPao} (указано {model.ShareholdersCount.Value}).");
        }
    }

    private static void ValidateBoardMembers(
        LegalEntitySaveValidationModel model,
        LegalEntitySaveValidationResult result)
    {
        if (!model.BoardMinNumber.HasValue || !model.BoardMemberNumber.HasValue)
            return;

        if (model.BoardMemberNumber.Value < model.BoardMinNumber.Value)
        {
            result.AddError(
                $"Количество членов СД ({model.BoardMemberNumber.Value}) меньше минимального ({model.BoardMinNumber.Value}).");
        }
    }

    private static void ValidateGosaWindow(
        LegalEntitySaveValidationModel model,
        OrgValidationType orgType,
        LegalEntitySaveValidationResult result)
    {
        if (!model.GosaWindowStart.HasValue || !model.GosaWindowEnd.HasValue)
            return;

        if (model.GosaWindowEnd.Value < model.GosaWindowStart.Value)
        {
            result.AddError("Дата окончания окна ГОСА не может быть раньше даты начала.");
            return;
        }

        var year = model.GosaWindowStart.Value.Year;
        DateOnly min = new(year, 3, 1);
        DateOnly max = new(year, 6, 30);

        if (orgType == OrgValidationType.PJSC)
        {
            if (model.GosaWindowStart.Value < min || model.GosaWindowEnd.Value > max)
            {
                result.AddError("Для ПАО окно ГОСА должно находиться в пределах 01.03–30.06.");
            }
        }
        else if (orgType == OrgValidationType.NJSC)
        {
            if (model.GosaWindowStart.Value != min || model.GosaWindowEnd.Value != max)
            {
                result.AddError("Для АО/НАО интервал ГОСА фиксирован: 01.03–30.06.");
            }
        }
    }

    private static void ValidateDirector(
        LegalEntitySaveValidationModel model,
        LegalEntitySaveValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(model.FullName))
            result.AddError("Укажите ФИО руководителя.");

        if (string.IsNullOrWhiteSpace(model.Position))
            result.AddError("Укажите должность руководителя.");
    }

    private static void ValidateStandardCharter(
        LegalEntitySaveValidationModel model,
        OrgValidationType orgType,
        LegalEntitySaveValidationResult result)
    {
        if (string.IsNullOrEmpty(model.StandardCharterNumber))
            return;

        var num = model.StandardCharterNumber;

        if (num.Length != 2 || !int.TryParse(num, out var parsed) || parsed < 1 || parsed > MaxStandardCharterNumber)
        {
            result.AddError($"Номер типового устава должен быть от 01 до {MaxStandardCharterNumber:D2}.");
            return;
        }

        if (orgType != OrgValidationType.LLC)
            result.AddError("Типовой устав применим только для ООО.");
    }

    /// <summary>
    /// Определяет тип организации по коду ОКОПФ.
    /// Делегирует в <see cref="OkopfTypeMapper.DetectType"/>.
    /// </summary>
    public static OrgValidationType DetectOrgType(string? okopfCode) =>
        OkopfTypeMapper.DetectType(okopfCode);

    /// <summary>Человекочитаемая метка типа организации. Делегирует в <see cref="OkopfTypeMapper.TypeLabel"/>.</summary>
    public static string OrgTypeLabel(OrgValidationType type) =>
        OkopfTypeMapper.TypeLabel(type);
}
