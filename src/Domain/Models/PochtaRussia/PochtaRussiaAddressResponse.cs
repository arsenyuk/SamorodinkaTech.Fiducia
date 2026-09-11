namespace SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

/// <summary>
/// Ответ на запрос нормализации адреса (POST /1.0/clean/address).
/// </summary>
public class PochtaRussiaAddressResponse
{
    /// <summary>Идентификатор записи (связь с запросом).</summary>
    public string Id { get; init; } = default!;

    /// <summary>Оригинальный адрес.</summary>
    public string OriginalAddress { get; init; } = default!;

    /// <summary>Код качества нормализации: GOOD, POSTAL_BOX, ON_DEMAND, UNDEF_05 и др.</summary>
    public string QualityCode { get; init; } = default!;

    /// <summary>Код проверки: VALIDATED, OVERRIDDEN, CONFIRMED_MANUALLY и др.</summary>
    public string ValidationCode { get; init; } = default!;

    /// <summary>Почтовый индекс.</summary>
    public string? Index { get; init; }

    /// <summary>Населённый пункт.</summary>
    public string? Place { get; init; }

    /// <summary>Область, регион.</summary>
    public string? Region { get; init; }

    /// <summary>Улица.</summary>
    public string? Street { get; init; }

    /// <summary>Номер здания.</summary>
    public string? House { get; init; }

    /// <summary>Корпус.</summary>
    public string? Corpus { get; init; }

    /// <summary>Строение.</summary>
    public string? Building { get; init; }

    /// <summary>Литера.</summary>
    public string? Letter { get; init; }

    /// <summary>Дробь.</summary>
    public string? Slash { get; init; }

    /// <summary>Номер помещения.</summary>
    public string? Room { get; init; }

    /// <summary>Район.</summary>
    public string? Area { get; init; }

    /// <summary>Микрорайон.</summary>
    public string? Location { get; init; }

    /// <summary>Номер для а/я, войсковая часть.</summary>
    public string? NumAddressType { get; init; }

    /// <summary>Тип адреса: DEFAULT, POBOX, RESIDENCE, HOTEL, EXTENSION.</summary>
    public string? AddressType { get; init; }

    /// <summary>
    /// Адрес корректен для отправки.
    /// quality-code ∈ {GOOD, POSTAL_BOX, ON_DEMAND, UNDEF_05}
    /// И validation-code ∈ {VALIDATED, OVERRIDDEN, CONFIRMED_MANUALLY}.
    /// </summary>
    public bool IsDeliverable =>
        QualityCode is "GOOD" or "POSTAL_BOX" or "ON_DEMAND" or "UNDEF_05"
        && ValidationCode is "VALIDATED" or "OVERRIDDEN" or "CONFIRMED_MANUALLY";
}
