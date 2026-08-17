using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Шаблон вехи организационного мероприятия (tpl_org_milestones).
/// Поддерживает все типы вех: обычные, межэтапные, юридические, контрольные, интеграционные.
/// </summary>
public class TplOrgMilestone
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор цели (intent_id).</summary>
    public Guid IntentId { get; set; }

    /// <summary>Цель.</summary>
    public TplOrgIntent? Intent { get; set; }

    /// <summary>Идентификатор этапа (stage_id). null для вех, привязанных к цели напрямую.</summary>
    public Guid? StageId { get; set; }

    /// <summary>Этап.</summary>
    public TplOrgStage? Stage { get; set; }

    /// <summary>Наименование вехи (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Тип вехи (milestone_type).</summary>
    public MilestoneType MilestoneType { get; set; }

    /// <summary>JSON-массив ID задач-предшественников (predecessor_offer_ids).</summary>
    public string? PredecessorOfferIds { get; set; }

    /// <summary>JSON-массив ID этапов-предшественников (predecessor_stage_ids).</summary>
    public string? PredecessorStageIds { get; set; }

    /// <summary>Смещение (дни) от предшественника: FS + N (offset_days).</summary>
    public int? OffsetDays { get; set; }

    /// <summary>Единица измерения смещения (measurement_unit_id).</summary>
    public Guid? MeasurementUnitId { get; set; }

    /// <summary>Справочник единиц измерения.</summary>
    public RefMeasurementUnit? MeasurementUnit { get; set; }

    /// <summary>Идентификатор задачи-контроля (control_offer_id). Для ЮВ/КВ — задача, которую система отслеживает.</summary>
    public Guid? ControlOfferId { get; set; }

    /// <summary>Задача-контроль.</summary>
    public TplOrgTaskOffer? ControlOffer { get; set; }

    /// <summary>Ссылка на норму закона / внутренний документ (legal_reference).</summary>
    public string? LegalReference { get; set; }

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
