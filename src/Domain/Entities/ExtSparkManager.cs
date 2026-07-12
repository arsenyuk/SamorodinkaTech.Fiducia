namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Руководитель компании из СПАРК — внешний кэш (ext_spark_manager, BDR-009).
/// Не является авторитетным источником. Обновляется только через API.
/// </summary>
public class ExtSparkManager
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>ИНН компании — ключ поиска (inn).</summary>
    public string Inn { get; set; } = default!;

    /// <summary>ФИО руководителя (full_name).</summary>
    public string FullName { get; set; } = default!;

    /// <summary>Должность (position).</summary>
    public string? Position { get; set; }

    /// <summary>ИНН физического лица — руководителя (person_inn).</summary>
    public string? PersonInn { get; set; }

    /// <summary>Дата начала полномочий (start_date).</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Временная метка получения данных из API (fetched_at).</summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
