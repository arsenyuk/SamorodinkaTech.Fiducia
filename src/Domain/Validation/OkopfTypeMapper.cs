namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Единый источник истины для маппинга кодов ОКОПФ на внутренние типы ЮЛ.
/// ADR-024: каждый поддерживаемый тип фиксируется отдельной строкой.
/// Английские сокращения — из docs/legal-entity-types.md.
/// </summary>
public static class OkopfTypeMapper
{
    /// <summary>Код ОКОПФ — ПАО (Public Joint-Stock Company).</summary>
    public const string PjscCode = "12247";

    /// <summary>Код ОКОПФ — НАО (Non-Public Joint-Stock Company).</summary>
    public const string NjscCode = "12267";

    /// <summary>Код ОКОПФ — ООО (Limited Liability Company).</summary>
    public const string LlcCode = "12300";

    /// <summary>
    /// Определяет тип организации по коду ОКОПФ.
    /// Извлекает только цифры из кода, игнорирует пробелы и разделители.
    /// Неизвестные коды → Unknown.
    /// </summary>
    public static OrgValidationType DetectType(string? okopfCode)
    {
        if (string.IsNullOrWhiteSpace(okopfCode))
            return OrgValidationType.Unknown;

        var normalized = new string(okopfCode.Where(char.IsDigit).ToArray());

        return normalized switch
        {
            PjscCode => OrgValidationType.PJSC,
            NjscCode => OrgValidationType.NJSC,
            LlcCode => OrgValidationType.LLC,
            _ => OrgValidationType.Unknown
        };
    }

    /// <summary>ПАО — Публичное акционерное общество (Public Joint-Stock Company).</summary>
    public static bool IsPjsc(string? okopfCode) =>
        string.Equals(okopfCode?.Trim(), PjscCode, StringComparison.Ordinal);

    /// <summary>ООО — Общество с ограниченной ответственностью (Limited Liability Company).</summary>
    public static bool IsLlc(string? okopfCode) =>
        string.Equals(okopfCode?.Trim(), LlcCode, StringComparison.Ordinal);

    /// <summary>Человекочитаемая метка типа организации для сообщений об ошибках.</summary>
    public static string TypeLabel(OrgValidationType type) => type switch
    {
        OrgValidationType.PJSC => "ПАО",
        OrgValidationType.NJSC => "непубличного АО",
        OrgValidationType.LLC => "ООО",
        _ => "данного типа общества"
    };
}

/// <summary>
/// Тип организации для целей валидации (не зависит от EF-сущностей).
/// ADR-024: каждый код ОКОПФ → отдельное значение enum.
/// Английские сокращения — из docs/legal-entity-types.md.
/// </summary>
public enum OrgValidationType
{
    Unknown,
    /// <summary>ООО — Общество с ограниченной ответственностью (Limited Liability Company).</summary>
    LLC,
    /// <summary>ПАО — Публичное акционерное общество (Public Joint-Stock Company).</summary>
    PJSC,
    /// <summary>НАО — Непубличное акционерное общество (Non-Public Joint-Stock Company).</summary>
    NJSC
}
