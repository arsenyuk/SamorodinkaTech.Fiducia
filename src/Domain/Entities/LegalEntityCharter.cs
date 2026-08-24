namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Параметры устава ООО — для типового и нетипового (legal_entity_charter).
/// 1:1 связь с LegalEntity. При типовом уставе параметры копируются из RefStandardCharter.
/// </summary>
public class LegalEntityCharter
{
    /// <summary>Идентификатор юридического лица — первичный ключ (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Юридическое лицо.</summary>
    public LegalEntity? LegalEntity { get; set; }

    /// <summary>Выход участника из общества разрешён (exit_allowed).</summary>
    public bool ExitAllowed { get; set; }

    /// <summary>
    /// Минимальная доля участника для права на выход (exit_allowed_min_share_percent).
    /// NULL — выход разрешён всем. Только для нетипового устава.
    /// </summary>
    public decimal? ExitAllowedMinSharePercent { get; set; }

    /// <summary>
    /// Максимальная доля участника для права на выход (exit_allowed_max_share_percent).
    /// NULL — без ограничения по максимуму. Только для нетипового устава.
    /// </summary>
    public decimal? ExitAllowedMaxSharePercent { get; set; }

    /// <summary>
    /// Условия выхода — свободный текст (exit_condition_description).
    /// Например: "по истечении 2 лет с момента вступления". NULL — без условий.
    /// </summary>
    public string? ExitConditionDescription { get; set; }

    /// <summary>
    /// Выход требует единогласного решения ОСУ (exit_requires_unanimous_osu).
    /// Если TRUE — заявление о выходе направляется на рассмотрение ОСУ.
    /// </summary>
    public bool ExitRequiresUnanimousOsu { get; set; }

    /// <summary>Переход доли к участникам без согласия остальных (transfer_to_participants_without_consent).</summary>
    public bool TransferToParticipantsWithoutConsent { get; set; } = true;

    /// <summary>Переход доли к третьим лицам: CONSENT — с согласия, WITHOUT_CONSENT — без согласия, FORBIDDEN — запрещён (transfer_to_third_parties).</summary>
    public string TransferToThirdParties { get; set; } = "CONSENT";

    /// <summary>Преимущественное право покупки доли участниками (preemptive_right).</summary>
    public bool PreemptiveRight { get; set; } = true;

    /// <summary>Переход доли к наследникам без согласия остальных (inheritance_without_consent).</summary>
    public bool InheritanceWithoutConsent { get; set; } = true;

    /// <summary>Тип единоличного исполнительного органа: A — гендиректор, B — каждый участник, C — все совместно (executive_body).</summary>
    public char ExecutiveBody { get; set; } = 'A';

    /// <summary>Идентификатор способа подтверждения протоколов ОСУ (protocol_confirmation_method_id).</summary>
    public Guid? ProtocolConfirmationMethodId { get; set; }

    /// <summary>Способ подтверждения протоколов ОСУ.</summary>
    public RefProtocolConfirmationMethod? ProtocolConfirmationMethod { get; set; }

    /// <summary>Файл текста устава (charter_document_id).</summary>
    public Guid? CharterDocumentId { get; set; }

    /// <summary>Документ устава.</summary>
    public FileEntry? CharterDocument { get; set; }

    /// <summary>Файл положения о Совете директоров (board_regulation_document_id).</summary>
    public Guid? BoardRegulationDocumentId { get; set; }

    /// <summary>Положение о Совете директоров.</summary>
    public FileEntry? BoardRegulationDocument { get; set; }

    /// <summary>Файл положения о комитетах (committee_regulation_document_id).</summary>
    public Guid? CommitteeRegulationDocumentId { get; set; }

    /// <summary>Положение о комитетах.</summary>
    public FileEntry? CommitteeRegulationDocument { get; set; }

    /// <summary>Обязательный аудит — выручка >800 млн или активы >400 млн (mandatory_audit). null — не указано.</summary>
    public bool? MandatoryAudit { get; set; }

    /// <summary>Наличие ревизионной комиссии — >15 участников ООО или ПАО/НАО≥50 (has_revision_commission). null — не указано.</summary>
    public bool? HasRevisionCommission { get; set; }

    /// <summary>Наличие Совета директоров — для ООО с нетиповым уставом (has_board_of_directors).</summary>
    public bool HasBoardOfDirectors { get; set; }

    /// <summary>Идентификатор срока полномочий ГД (gd_term_id). Только для нетипового устава с executive_body = 'A'.</summary>
    public Guid? GdTermId { get; set; }

    /// <summary>Срок полномочий ГД.</summary>
    public RefGdTerm? GdTerm { get; set; }

    /// <summary>Порог доли участника для требования о созыве ВОСУ (vosu_threshold_percent). null = 10% по закону.</summary>
    public decimal? VosuThresholdPercent { get; set; }

    /// <summary>СД принимает решение о созыве ОСУ (board_decides_convening_osu).
    /// Для ООО с нетиповым уставом. Определяет流向 требований участников: если включено — СД вместо ГД.</summary>
    public bool BoardDecidesConveningOsu { get; set; }
}