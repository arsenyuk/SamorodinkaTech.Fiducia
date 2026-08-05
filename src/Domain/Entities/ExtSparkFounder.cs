namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Учредитель (участник) компании из СПАРК — внешний кэш (ext_spark_founder, BDR-009).
/// Не является авторитетным источником. Обновляется только через API.
/// </summary>
public class ExtSparkFounder
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>ИНН компании — ключ поиска (inn).</summary>
    public string Inn { get; set; } = default!;

    /// <summary>Наименование учредителя-ЮЛ (name).</summary>
    public string? Name { get; set; }

    /// <summary>ИНН учредителя-ЮЛ (founder_inn).</summary>
    public string? FounderInn { get; set; }

    /// <summary>ФИО учредителя-ФЛ (full_name).</summary>
    public string? FullName { get; set; }

    /// <summary>ИНН учредителя-ФЛ (person_inn).</summary>
    public string? PersonInn { get; set; }

    /// <summary>Размер доли в рублях, номинальная стоимость (share_amount).</summary>
    public decimal? ShareAmount { get; set; }

    /// <summary>Размер доли в процентах (share_percent).</summary>
    public decimal? SharePercent { get; set; }

    /// <summary>Временная метка получения данных из API (fetched_at).</summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
