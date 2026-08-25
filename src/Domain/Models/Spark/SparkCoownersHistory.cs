namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// История совладельцев компании из СПАРК — SOAP-метод GetCompanyCoownersHistory.
/// Содержит текущих и бывших совладельцев с датами изменения.
/// </summary>
public class SparkCoownersHistory
{
    /// <summary>ИНН компании.</summary>
    public string Inn { get; init; } = default!;

    /// <summary>Текущие совладельцы.</summary>
    public List<SparkCoownerHistoryItem> CurrentCoowners { get; init; } = new();

    /// <summary>Исторические совладельцы (бывшие).</summary>
    public List<SparkCoownerHistoryItem> HistoricalCoowners { get; init; } = new();
}

/// <summary>
/// Элемент истории совладельцев.
/// </summary>
public class SparkCoownerHistoryItem
{
    /// <summary>Наименование (для ЮЛ).</summary>
    public string? Name { get; init; }

    /// <summary>ИНН.</summary>
    public string? Inn { get; init; }

    /// <summary>ОГРН.</summary>
    public string? Ogrn { get; init; }

    /// <summary>ФИО (для ФЛ).</summary>
    public string? FullName { get; init; }

    /// <summary>ИНН ФЛ.</summary>
    public string? PersonInn { get; init; }

    /// <summary>Доля участия (в процентах).</summary>
    public decimal? SharePercent { get; init; }

    /// <summary>Доля участия (номинальная стоимость в рублях).</summary>
    public decimal? ShareAmount { get; init; }

    /// <summary>Дата вхождения.</summary>
    public DateTime? EntryDate { get; init; }

    /// <summary>Дата выхода (null — действующий).</summary>
    public DateTime? ExitDate { get; init; }

    /// <summary>Тип совладельца: ЮЛ/ФЛ/ИП.</summary>
    public string? CoownerType { get; init; }

    /// <summary>Страна регистрации.</summary>
    public string? Country { get; init; }
}
