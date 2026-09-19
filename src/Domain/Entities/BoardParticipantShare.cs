namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Версия доли участника совета директоров (board_participant_share).
/// SCD Type 2: каждая версия — отдельная запись. Активная: is_active = true.
/// </summary>
public class BoardParticipantShare
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор участника СД (participant_id).</summary>
    public Guid ParticipantId { get; set; }

    /// <summary>Участник общества.</summary>
    public BoardParticipant? Participant { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Процент доли (share_percent).</summary>
    public decimal? SharePercent { get; set; }

    /// <summary>Доля в виде простой дроби, например "1/3" (share_fraction).</summary>
    public string? ShareFraction { get; set; }

    /// <summary>Номинальная стоимость доли в рублях (share_amount).</summary>
    public decimal? ShareAmount { get; set; }

    /// <summary>Сведения об оплате доли (payment_info).</summary>
    public string? PaymentInfo { get; set; }

    /// <summary>Информация о регистрации операций с долей (share_registration_info).</summary>
    public string? ShareRegistrationInfo { get; set; }

    /// <summary>Активная версия (is_active).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата обновления записи (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
