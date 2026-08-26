namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Информирование об изменении сведений участника (board_participant_change).
/// Хранит новые данные участника и документ-подтверждение.
/// </summary>
public class BoardParticipantChange
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Идентификатор участника (participant_id).</summary>
    public Guid ParticipantId { get; set; }

    /// <summary>Тип участника: FL — физлицо, UL — юрлицо, IP — ИП (participant_type).</summary>
    public string ParticipantType { get; set; } = "FL";

    // ── ФЛ ────────────────────────────────────────────────────────

    /// <summary>ФИО участника-ФЛ (full_name).</summary>
    public string? FullName { get; set; }

    /// <summary>Идентификатор вида документа, удостоверяющего личность (dul_type_id).</summary>
    public Guid? DulTypeId { get; set; }

    /// <summary>Серия паспорта (passport_series).</summary>
    public string? PassportSeries { get; set; }

    /// <summary>Номер паспорта (passport_number).</summary>
    public string? PassportNumber { get; set; }

    /// <summary>Кем выдан паспорт (passport_issued_by).</summary>
    public string? PassportIssuedBy { get; set; }

    /// <summary>Дата выдачи паспорта (passport_issue_date).</summary>
    public DateOnly? PassportIssueDate { get; set; }

    /// <summary>Код подразделения (passport_department_code).</summary>
    public string? PassportDepartmentCode { get; set; }

    /// <summary>Адрес регистрации по паспорту (passport_registration_address).</summary>
    public string? PassportRegistrationAddress { get; set; }

    /// <summary>ИНН физического лица (person_inn).</summary>
    public string? PersonInn { get; set; }

    /// <summary>Гражданство (citizenship).</summary>
    public string? Citizenship { get; set; }

    // ── ЮЛ ────────────────────────────────────────────────────────

    /// <summary>Наименование юридического лица (company_name).</summary>
    public string? CompanyName { get; set; }

    /// <summary>ИНН юридического лица (company_inn).</summary>
    public string? CompanyInn { get; set; }

    /// <summary>ОГРН юридического лица (company_ogrn).</summary>
    public string? CompanyOgrn { get; set; }

    /// <summary>КПП юридического лица (company_kpp).</summary>
    public string? CompanyKpp { get; set; }

    /// <summary>Адрес юридического лица (company_address).</summary>
    public string? CompanyAddress { get; set; }

    // ── ИП ────────────────────────────────────────────────────────

    /// <summary>ОГРНИП (ogrnip).</summary>
    public string? Ogrnip { get; set; }

    // ── Доля ──────────────────────────────────────────────────────

    /// <summary>Размер доли в процентах (share_percent).</summary>
    public decimal? SharePercent { get; set; }

    /// <summary>Номинальная стоимость доли в рублях (share_amount).</summary>
    public decimal? ShareAmount { get; set; }

    // ── Мета ──────────────────────────────────────────────────────

    /// <summary>Идентификатор документа-подтверждения (document_file_id).</summary>
    public Guid? DocumentFileId { get; set; }

    /// <summary>Исходное имя документа (document_original_name).</summary>
    public string? DocumentOriginalName { get; set; }

    /// <summary>Источник: paper — бумажное, electronic — электронное (source).</summary>
    public string? Source { get; set; }

    /// <summary>Дата информирования (для бумажных) (date).</summary>
    public string? Date { get; set; }

    /// <summary>Номер бумажного документа (paper_doc_number).</summary>
    public string? PaperDocNumber { get; set; }

    /// <summary>Комментарий / примечание (comment).</summary>
    public string? Comment { get; set; }

    /// <summary>Идентификатор пользователя, подавшего информацию (submitted_by).</summary>
    public Guid? SubmittedBy { get; set; }

    /// <summary>Дата подачи (submitted_at).</summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Статус: pending / approved / rejected (status).</summary>
    public string Status { get; set; } = "pending";

    /// <summary>Комментарий при рассмотрении (review_comment).</summary>
    public string? ReviewComment { get; set; }

    /// <summary>Идентификатор рассмотревшего (reviewed_by).</summary>
    public Guid? ReviewedBy { get; set; }

    /// <summary>Дата рассмотрения (reviewed_at).</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Время последнего обновления (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
