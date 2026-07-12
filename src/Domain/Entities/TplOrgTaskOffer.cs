namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Шаблон задачи организационного мероприятия (tpl_org_offers).
/// Третий уровень иерархии, привязан к OrgStage. Каждый офер — шаблон одной будущей задачи.
/// </summary>
public class TplOrgTaskOffer
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор этапа (stage_id).</summary>
    public Guid StageId { get; set; }

    /// <summary>Этап.</summary>
    public TplOrgStage? Stage { get; set; }

    /// <summary>Наименование задачи-офера (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Описание (description).</summary>
    public string? Description { get; set; }

    /// <summary>Смещение начала относительно родителя, дни (start_offset_days).</summary>
    public int? StartOffsetDays { get; set; }

    /// <summary>Принцип вычисления дедлайна (deadline_rule).</summary>
    public string? DeadlineRule { get; set; }

    /// <summary>Количество дней до дедлайна (deadline_days).</summary>
    public int? DeadlineDays { get; set; }

    /// <summary>Роли-кандидаты для данного офера.</summary>
    public ICollection<TplOrgOfferRole>? OfferRoles { get; set; }

    /// <summary>Идентификатор роли исполнителя (assigned_role_id).</summary>
    public Guid? AssignedRoleId { get; set; }

    /// <summary>Роль исполнителя.</summary>
    public RefRole? AssignedRole { get; set; }

    /// <summary>Идентификатор должности в Совете директоров (assigned_board_role_id).</summary>
    public Guid? AssignedBoardRoleId { get; set; }

    /// <summary>Должность в Совете директоров.</summary>
    public RefBoardRole? AssignedBoardRole { get; set; }

    /// <summary>Включать только при нотариальном подтверждении решений (require_notary_confirmation). null — без проверки.</summary>
    public bool? RequireNotaryConfirmation { get; set; }

    /// <summary>Включать только при подписании решений всеми участниками (require_all_sign_confirmation). null — без проверки.</summary>
    public bool? RequireAllSignConfirmation { get; set; }

    /// <summary>Включать только при наличии обязательных комитетов СД (require_committees). null — без проверки.</summary>
    public bool? RequireCommittees { get; set; }

    /// <summary>Включать только при наличии Положения о СД (require_board_regulation). null — без проверки.</summary>
    public bool? RequireBoardRegulation { get; set; }

    /// <summary>Включать только для нетипового устава (require_custom_charter). null — без проверки.</summary>
    public bool? RequireCustomCharter { get; set; }

    /// <summary>Включать только для исполнительного органа типа A — гендиректор (require_executive_body_a). null — без проверки.</summary>
    public bool? RequireExecutiveBodyA { get; set; }

    /// <summary>Включать только если сформирован Совет директоров (require_board_of_directors). null — без проверки.</summary>
    public bool? RequireBoardOfDirectors { get; set; }

    /// <summary>Включать только при ЮЗЭДО — Mixed или LegalElectronic (require_document_flow_legal_electronic). null — без проверки.</summary>
    public bool? RequireDocumentFlowLegalElectronic { get; set; }

    /// <summary>Включать только при обязательном аудите — выручка >800 млн или активы >400 млн (require_mandatory_audit). null — без проверки.</summary>
    public bool? RequireMandatoryAudit { get; set; }

    /// <summary>Включать только при обязательной ревизионной комиссии — >15 участников ООО или ПАО/НАО≥50 акционеров (require_revision_commission). null — без проверки.</summary>
    public bool? RequireRevisionCommission { get; set; }

    /// <summary>JSON-массив ID оферов-предшественников (predecessor_offer_ids).</summary>
    public string? PredecessorOfferIds { get; set; }

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
