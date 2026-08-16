namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник единиц измерения сроков (ref_measurement_unit).
/// Календарный день или рабочий день.
/// </summary>
public class RefMeasurementUnit
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код записи (code): CALENDAR, BUSINESS.</summary>
    public string Code { get; set; } = default!;

    /// <summary>Полное наименование (name): «День (календарный)», «Рабочий день».</summary>
    public string Name { get; set; } = default!;

    /// <summary>Краткое наименование для отображения в списках (short_name): «календ. дн.», «раб. дн.».</summary>
    public string ShortName { get; set; } = default!;

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
