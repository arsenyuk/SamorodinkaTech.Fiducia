namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник сроков полномочий генерального директора ООО (ref_gd_term).
/// Числовое поле duration_years используется системой для расчёта срока полномочий.
/// </summary>
public class RefGdTerm
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код записи (code): 1_YEAR, 2_YEARS, ... , INDEFINITE.</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование для отображения в UI (name): «1 год», «2 года», ... , «Бессрочно».</summary>
    public string Name { get; set; } = default!;

    /// <summary>Числовое значение срока в годах (duration_years). NULL — бессрочно.</summary>
    public int? DurationYears { get; set; }

    /// <summary>Порядок сортировки в выпадающем списке (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
