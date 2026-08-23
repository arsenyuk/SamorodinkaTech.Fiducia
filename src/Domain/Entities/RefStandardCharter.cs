namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник типовых уставов ООО (ref_standard_charter).
/// 36 вариантов, утверждённых Приказом Минэкономразвития России от 01.08.2018 № 411.
/// </summary>
public class RefStandardCharter
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Номер типового устава 01–36, с ведущим нулём для 1–9 (number). Формат ФНС для форм Р11001 и Р13014.</summary>
    public string Number { get; set; } = default!;

    /// <summary>Выход участника из общества разрешён (exit_allowed).</summary>
    public bool ExitAllowed { get; set; }

    /// <summary>Переход доли к участникам без согласия остальных (transfer_to_participants_without_consent).</summary>
    public bool TransferToParticipantsWithoutConsent { get; set; }

    /// <summary>Переход доли к третьим лицам без согласия остальных (transfer_to_third_parties_without_consent).</summary>
    public bool TransferToThirdPartiesWithoutConsent { get; set; }

    /// <summary>Преимущественное право покупки доли участниками (preemptive_right).</summary>
    public bool PreemptiveRight { get; set; }

    /// <summary>Переход доли к наследникам без согласия остальных (inheritance_without_consent).</summary>
    public bool InheritanceWithoutConsent { get; set; }

    /// <summary>Тип единоличного исполнительного органа: A/B/C (executive_body).</summary>
    public char ExecutiveBody { get; set; }

    /// <summary>Идентификатор способа подтверждения протоколов ОСУ (protocol_confirmation_method_id).</summary>
    public Guid ProtocolConfirmationMethodId { get; set; }

    /// <summary>Способ подтверждения протоколов ОСУ.</summary>
    public RefProtocolConfirmationMethod ProtocolConfirmationMethod { get; set; } = default!;
    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}