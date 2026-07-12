namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Данные об учредителе (участнике) компании из СПАРК.
/// Поля зеркалируют структуру ответа API СПАРК: для ЮЛ — Name/Inn/Ogrn/Country,
/// для ФЛ — FullName/PersonInn/Citizenship + блок участия в других организациях.
/// </summary>
public class SparkFounder
{
    // ── ЮЛ ────────────────────────────────────────────────────────

    /// <summary>Наименование учредителя-ЮЛ.</summary>
    public string? Name { get; init; }

    /// <summary>ИНН учредителя-ЮЛ.</summary>
    public string? Inn { get; init; }

    /// <summary>ОГРН учредителя-ЮЛ.</summary>
    public string? Ogrn { get; init; }

    /// <summary>Страна регистрации учредителя-ЮЛ.</summary>
    public string? Country { get; init; }

    /// <summary>Признак иностранного юридического лица.</summary>
    public bool IsForeign { get; init; }

    // ── ФЛ ────────────────────────────────────────────────────────

    /// <summary>ФИО учредителя-ФЛ.</summary>
    public string? FullName { get; init; }

    /// <summary>ИНН учредителя-ФЛ.</summary>
    public string? PersonInn { get; init; }

    /// <summary>Гражданство учредителя-ФЛ.</summary>
    public string? Citizenship { get; init; }

    // ── Участие в других организациях (только для ФЛ) ─────────────

    /// <summary>Количество организаций, где ФЛ является руководителем.</summary>
    public int? HeadOfOther { get; init; }

    /// <summary>Количество организаций, где ФЛ является учредителем/участником.</summary>
    public int? FounderOfOther { get; init; }

    /// <summary>Зарегистрирован ли ФЛ как индивидуальный предприниматель.</summary>
    public bool IsEntrepreneur { get; init; }

    /// <summary>ОГРНИП учредителя-ФЛ (если ИП).</summary>
    public string? Ogrnip { get; init; }

    // ── Доля ──────────────────────────────────────────────────────

    /// <summary>Размер доли в рублях (номинальная стоимость).</summary>
    public decimal? ShareAmount { get; init; }

    /// <summary>Размер доли в процентах.</summary>
    public decimal? SharePercent { get; init; }

    // ── Даты ──────────────────────────────────────────────────────

    /// <summary>Дата вхождения в состав участников.</summary>
    public DateTime? EntryDate { get; init; }

    /// <summary>Дата выхода из состава участников (null — действующий).</summary>
    public DateTime? ExitDate { get; init; }
}
