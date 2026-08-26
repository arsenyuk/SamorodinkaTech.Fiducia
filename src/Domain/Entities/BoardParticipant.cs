namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Участник общества — реестр (board_participant).
/// Хранит актуальный состав участников с данными ДУЛ/реквизитов ЮЛ.
/// </summary>
public class BoardParticipant
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Идентификатор участника экосистемы (ecosystem_participant_id).</summary>
    public Guid? EcosystemParticipantId { get; set; }

    /// <summary>Участник экосистемы.</summary>
    public EcosystemParticipant? EcosystemParticipant { get; set; }

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

    /// <summary>Сведения об оплате доли (payment_info).</summary>
    public string? PaymentInfo { get; set; }

    /// <summary>Информация о регистрации операций с долей (share_registration_info).</summary>
    public string? ShareRegistrationInfo { get; set; }

    // ── Статус и даты ─────────────────────────────────────────────

    /// <summary>Дата вхождения в состав участников (entry_date).</summary>
    public DateOnly? EntryDate { get; set; }

    /// <summary>Дата выхода из состава (exit_date).</summary>
    public DateOnly? ExitDate { get; set; }

    /// <summary>Действующий участник (is_active).</summary>
    public bool IsActive { get; set; } = true;

    // ── Мета ──────────────────────────────────────────────────────

    /// <summary>Порядок сортировки (sort_order).</summary>
    public int SortOrder { get; set; }

    /// <summary>Время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Время последнего обновления (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
