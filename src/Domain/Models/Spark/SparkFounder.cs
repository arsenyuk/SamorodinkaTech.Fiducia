namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Данные об учредителе (участнике) компании из СПАРК.
/// Для ЮЛ — поля Name/Inn, для ФЛ — поля FullName/PersonInn.
/// </summary>
public class SparkFounder
{
    /// <summary>Наименование учредителя-ЮЛ.</summary>
    public string? Name { get; init; }

    /// <summary>ИНН учредителя-ЮЛ.</summary>
    public string? Inn { get; init; }

    /// <summary>ФИО учредителя-ФЛ.</summary>
    public string? FullName { get; init; }

    /// <summary>ИНН учредителя-ФЛ.</summary>
    public string? PersonInn { get; init; }

    /// <summary>Размер доли в рублях (номинальная стоимость).</summary>
    public decimal? ShareAmount { get; init; }

    /// <summary>Размер доли в процентах.</summary>
    public decimal? SharePercent { get; init; }
}
