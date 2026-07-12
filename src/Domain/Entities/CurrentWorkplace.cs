namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Текущее место работы руководителя (current_workplace).
/// Singleton-таблица — одна запись на систему (BDR‑007).
/// </summary>
public class CurrentWorkplace
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>ФИО руководителя (full_name).</summary>
    public string FullName { get; set; } = default!;

    /// <summary>Должность (position).</summary>
    public string? Position { get; set; }
}
