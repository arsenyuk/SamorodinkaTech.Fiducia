namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Серверный валидатор сохранения ЮЛ (BDR‑008).
/// Полностью изолирован от окружения: не зависит от БД, DI, HTTP-контекста.
/// Пригоден для unit-тестирования без моков.
/// </summary>
public static class LegalEntityValidator
{
    /// <summary>Код ПАО по ОКОПФ.</summary>
    public const string PaoCode = "12247";

    /// <summary>Максимальное количество акционеров для НАО/ООО.</summary>
    public const int MaxShareholdersForNonPao = 50;

    /// <summary>
    /// Выполняет полную серверную валидацию данных ЮЛ перед сохранением.
    /// </summary>
    /// <param name="model">Данные формы ЮЛ.</param>
    /// <returns>Результат валидации со списком ошибок (пуст — успех).</returns>
    public static LegalEntitySaveValidationResult Validate(LegalEntitySaveValidationModel model)
    {
        var result = new LegalEntitySaveValidationResult();

        var orgType = DetectOrgType(model.OkopfCode);

        ValidateBoardMandatory(model, orgType, result);
        ValidateShareholdersCount(model, orgType, result);
        ValidateBoardMembers(model, orgType, result);
        ValidateGosaWindow(model, orgType, result);
        ValidateDirector(model, result);

        return result;
    }

    private static void ValidateBoardMandatory(
        LegalEntitySaveValidationModel model,
        OrgValidationType orgType,
        LegalEntitySaveValidationResult result)
    {
        var mandatory = orgType == OrgValidationType.PAO
            || (orgType == OrgValidationType.NAO_AO && (model.ShareholdersCount ?? 1) >= MaxShareholdersForNonPao);

        if (mandatory && !model.HasBoardOfDirectors)
            result.AddError($"Для {OrgTypeLabel(orgType)} Совет директоров обязателен.");
    }

    private static void ValidateShareholdersCount(
        LegalEntitySaveValidationModel model,
        OrgValidationType orgType,
        LegalEntitySaveValidationResult result)
    {
        if (!model.HasBoardOfDirectors)
            return;

        if (model.ShareholdersCount is null or <= 0)
        {
            result.AddError("Укажите количество акционеров.");
            return;
        }

        if (orgType != OrgValidationType.PAO && model.ShareholdersCount.Value > MaxShareholdersForNonPao)
        {
            var label = OrgTypeLabel(orgType);
            result.AddError(
                $"Для {label} максимальное количество акционеров — {MaxShareholdersForNonPao}. " +
                $"Указано: {model.ShareholdersCount.Value}.");
        }
    }

    private static void ValidateBoardMembers(
        LegalEntitySaveValidationModel model,
        OrgValidationType orgType,
        LegalEntitySaveValidationResult result)
    {
        if (!model.HasBoardOfDirectors)
            return;

        if (model.BoardMemberNumber.HasValue && model.BoardMinNumber.HasValue
            && model.BoardMemberNumber.Value < model.BoardMinNumber.Value)
        {
            result.AddError(
                $"Количество участников СД ({model.BoardMemberNumber.Value}) " +
                $"не может быть меньше минимального ({model.BoardMinNumber.Value}).");
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

        if (orgType == OrgValidationType.PAO)
        {
            if (model.GosaWindowStart.Value < min || model.GosaWindowEnd.Value > max)
            {
                result.AddError("Для ПАО окно ГОСА должно находиться в пределах 01.03–30.06.");
            }
        }
        else if (orgType == OrgValidationType.NAO_AO)
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

    /// <summary>
    /// Определяет тип организации по коду ОКОПФ.
    /// Извлекает только цифры из кода, игнорирует пробелы и разделители.
    /// </summary>
    public static OrgValidationType DetectOrgType(string? okopfCode)
    {
        if (string.IsNullOrWhiteSpace(okopfCode))
            return OrgValidationType.Unknown;

        var normalized = new string(okopfCode.Where(char.IsDigit).ToArray());

        return normalized switch
        {
            PaoCode => OrgValidationType.PAO,
            "12267" => OrgValidationType.NAO_AO,
            "12260" => OrgValidationType.NAO_AO,
            "12300" => OrgValidationType.LLC,
            _ => OrgValidationType.Unknown
        };
    }

    /// <summary>Человекочитаемая метка типа организации для сообщений об ошибках.</summary>
    public static string OrgTypeLabel(OrgValidationType type) => type switch
    {
        OrgValidationType.PAO => "ПАО",
        OrgValidationType.NAO_AO => "непубличного АО",
        OrgValidationType.LLC => "ООО",
        _ => "данного типа общества"
    };
}

/// <summary>
/// Тип организации для целей валидации (не зависит от EF-сущностей).
/// </summary>
public enum OrgValidationType
{
    Unknown,
    LLC,
    PAO,
    NAO_AO
}
