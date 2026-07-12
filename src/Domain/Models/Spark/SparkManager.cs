namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Данные о руководителе (генеральном директоре) компании из СПАРК.
/// </summary>
public class SparkManager
{
    /// <summary>ФИО генерального директора.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>Должность (например, «Генеральный директор»).</summary>
    public string? Position { get; init; }

    /// <summary>ИНН физического лица (руководителя).</summary>
    public string? Inn { get; init; }

    /// <summary>Дата начала полномочий.</summary>
    public DateTime? StartDate { get; init; }
}
