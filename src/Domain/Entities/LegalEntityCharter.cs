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

    /// <summary>Переход доли к участникам без согласия остальных (transfer_to_participants_without_consent).</summary>
    public bool TransferToParticipantsWithoutConsent { get; set; } = true;

    /// <summary>Переход доли к третьим лицам без согласия остальных (transfer_to_third_parties_without_consent).</summary>
    public bool TransferToThirdPartiesWithoutConsent { get; set; }

    /// <summary>Преимущественное право покупки доли участниками (preemptive_right).</summary>
    public bool PreemptiveRight { get; set; } = true;

    /// <summary>Переход доли к наследникам без согласия остальных (inheritance_without_consent).</summary>
    public bool InheritanceWithoutConsent { get; set; } = true;

    /// <summary>Тип единоличного исполнительного органа: A — гендиректор, B — каждый участник, C — все совместно (executive_body).</summary>
    public char ExecutiveBody { get; set; } = 'A';

    /// <summary>Подтверждение решений подписанием протокола всеми участниками, а не нотариально (decision_confirmation_by_all_sign).</summary>
    public bool DecisionConfirmationByAllSign { get; set; }

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