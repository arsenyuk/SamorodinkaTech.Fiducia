namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Данные об учредителе (участнике) компании из СПАРК.
/// Для ЮЛ — поля Name/Inn/Ogrn/Country, для ФЛ — поля FullName/PersonInn/Citizenship.
/// </summary>
public class SparkFounder
{
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

    /// <summary>ФИО учредителя-ФЛ.</summary>
    public string? FullName { get; init; }

    /// <summary>ИНН учредителя-ФЛ.</summary>
    public string? PersonInn { get; init; }

    /// <summary>Гражданство учредителя-ФЛ.</summary>
    public string? Citizenship { get; init; }

    /// <summary>Размер доли в рублях (номинальная стоимость).</summary>
    public decimal? ShareAmount { get; init; }

    /// <summary>Размер доли в процентах.</summary>
    public decimal? SharePercent { get; init; }

    /// <summary>Дата вхождения в состав участников.</summary>
    public DateTime? EntryDate { get; init; }

    /// <summary>Дата выхода из состава участников (null — действующий).</summary>
    public DateTime? ExitDate { get; init; }

    /// <summary>Количество других организаций, где ФЛ является руководителем (только для ФЛ).</summary>
    public int? DirectorCount { get; init; }
}
