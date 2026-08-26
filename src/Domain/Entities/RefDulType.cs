namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник видов документов, удостоверяющих личность (ref_dul_type).
/// Источник: Приложение №2 к Приказу ФНС от 31.08.2020 № ЕД-7-14/617@.
/// </summary>
public class RefDulType
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код документа (code): 03, 07, 08, 10, 11, 12, 13, 15, 18, 21, 23, 24, 91.</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование документа (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Документ имеет серию (has_series). True для паспорта РФ, военного билета и т.д.</summary>
    public bool HasSeries { get; set; }

    /// <summary>Документ имеет код подразделения (has_department_code). True только для паспорта РФ (код 21).</summary>
    public bool HasDepartmentCode { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
