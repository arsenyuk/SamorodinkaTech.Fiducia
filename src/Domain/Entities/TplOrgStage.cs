using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Этап организационного мероприятия (tpl_org_stages).
/// Второй уровень иерархии, привязан к OrgIntent.
/// </summary>
public class TplOrgStage
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор цели (intent_id).</summary>
    public Guid IntentId { get; set; }

    /// <summary>Цель.</summary>
    public TplOrgIntent? Intent { get; set; }

    /// <summary>Наименование этапа (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Смещение начала относительно родителя, дни (start_offset_days).</summary>
    public int? StartOffsetDays { get; set; }

    /// <summary>Принцип вычисления дедлайна: FIXED_DAYS, BEFORE_DATE, AFTER_START (deadline_rule).</summary>
    public string? DeadlineRule { get; set; }

    /// <summary>Количество дней до дедлайна (deadline_days).</summary>
    public int? DeadlineDays { get; set; }

    /// <summary>Единица измерения сроков (measurement_unit_id).</summary>
    public Guid? MeasurementUnitId { get; set; }

    /// <summary>Справочник единиц измерения.</summary>
    public RefMeasurementUnit? MeasurementUnit { get; set; }

    /// <summary>Тип зависимости: FS — Финиш-Старт, SS — Старт-Старт (dependency_type).</summary>
    public DependencyType DependencyType { get; set; } = DependencyType.FS;

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>Оферы, относящиеся к данному этапу.</summary>
    public ICollection<TplOrgTaskOffer>? Offers { get; set; }

    /// <summary>Вехи, относящиеся к данному этапу.</summary>
    public ICollection<TplOrgMilestone>? Milestones { get; set; }

    /// <summary>JSON-массив ID этапов-предшественников (predecessor_stage_ids).</summary>
    public string? PredecessorStageIds { get; set; }
}
