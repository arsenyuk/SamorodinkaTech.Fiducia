namespace SamorodinkaTech.Fiducia.Domain.Models.DaData;

/// <summary>
/// Organization data from DaData (EGRUL source).
/// Note: EGRUL data may be up to 3 days old.
/// </summary>
public record DaDataCompanyInfo
{
    /// <summary>ИНН организации.</summary>
    public string? Inn { get; init; }

    /// <summary>ОГРН организации.</summary>
    public string? Ogrn { get; init; }

    /// <summary>КПП организации.</summary>
    public string? Kpp { get; init; }

    /// <summary>Полное наименование.</summary>
    public string? Name { get; init; }

    /// <summary>Краткое наименование.</summary>
    public string? ShortName { get; init; }

    /// <summary>Код ОКВЭД.</summary>
    public string? Okved { get; init; }

    /// <summary>Наименование ОКВЭД.</summary>
    public string? OkvedName { get; init; }

    /// <summary>Юридический адрес.</summary>
    public string? Address { get; init; }

    /// <summary>Статус организации.</summary>
    public string? Status { get; init; }

    /// <summary>Дата регистрации.</summary>
    public DateTime? RegistrationDate { get; init; }

    /// <summary>Дата последнего обновления данных из ЕГРЮЛ (до 3 дней).</summary>
    public DateTime? EgrulUpdatedAt { get; init; }
}
