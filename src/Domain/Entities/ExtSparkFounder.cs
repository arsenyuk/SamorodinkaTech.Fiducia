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

    /// <summary>ОГРН учредителя-ЮЛ (founder_ogrn).</summary>
    public string? FounderOgrn { get; set; }

    /// <summary>Страна регистрации учредителя-ЮЛ (country).</summary>
    public string? Country { get; set; }

    /// <summary>Признак иностранного ЮЛ (is_foreign).</summary>
    public bool IsForeign { get; set; }

    /// <summary>ФИО учредителя-ФЛ (full_name).</summary>
    public string? FullName { get; set; }

    /// <summary>ИНН учредителя-ФЛ (person_inn).</summary>
    public string? PersonInn { get; set; }

    /// <summary>Гражданство учредителя-ФЛ (citizenship).</summary>
    public string? Citizenship { get; set; }

    /// <summary>Размер доли в рублях, номинальная стоимость (share_amount).</summary>
    public decimal? ShareAmount { get; set; }

    /// <summary>Размер доли в процентах (share_percent).</summary>
    public decimal? SharePercent { get; set; }

    /// <summary>Дата вхождения в состав участников (entry_date).</summary>
    public DateTime? EntryDate { get; set; }

    /// <summary>Дата выхода из состава (exit_date).</summary>
    public DateTime? ExitDate { get; set; }

    /// <summary>Количество других организаций, где ФЛ — руководитель (director_count).</summary>
    public int? DirectorCount { get; set; }

    /// <summary>Временная метка получения данных из API (fetched_at).</summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
