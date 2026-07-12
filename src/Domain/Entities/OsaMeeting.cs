namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Запись об общем собрании акционеров (osa_meetings).
/// </summary>
public class OsaMeeting
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Форма проведения ОСА (osa_form_id).</summary>
    public Guid OsaFormId { get; set; }

    /// <summary>Дата начала окна ГОСА (gosa_window_start).</summary>
    public DateOnly? GosaWindowStart { get; set; }

    /// <summary>Дата окончания окна ГОСА (gosa_window_end).</summary>
    public DateOnly? GosaWindowEnd { get; set; }

    /// <summary>Количество акционеров (shareholders_count).</summary>
    public int? ShareholdersCount { get; set; }

    /// <summary>Минимальное количество участников СД (board_min_number).</summary>
    public int? BoardMinNumber { get; set; }

    /// <summary>Фактическое количество участников СД (board_member_number).</summary>
    public int? BoardMemberNumber { get; set; }

    /// <summary>Список акционеров получен (shareholders_list_received).</summary>
    public bool ShareholdersListReceived { get; set; }

    /// <summary>Заочное голосование (absentee_voting).</summary>
    public bool AbsenteeVoting { get; set; }

    /// <summary>ОСА проведено (osa_held).</summary>
    public bool OsaHeld { get; set; }

    /// <summary>Протокол подписан (protocol_signed).</summary>
    public bool ProtocolSigned { get; set; }

    /// <summary>Предусмотрен заместитель председателя (deputy_chair_provided).</summary>
    public bool DeputyChairProvided { get; set; }

    /// <summary>Предусмотрен секретарь СД (secretary_provided).</summary>
    public bool SecretaryProvided { get; set; }

    /// <summary>Секретарь подписывает протоколы (secretary_signs_protocols).</summary>
    public bool SecretarySignsProtocols { get; set; }

    /// <summary>Предусмотрен временный председательствующий (temporary_chair_provided).</summary>
    public bool TemporaryChairProvided { get; set; }

    /// <summary>Состав СД утверждён (board_composition_approved).</summary>
    public bool BoardCompositionApproved { get; set; }

    /// <summary>Способ выбора временного председательствующего (temporary_chair_selection).</summary>
    public string? TemporaryChairSelection { get; set; }

    /// <summary>ФИО временного председательствующего (temporary_chair_name).</summary>
    public string? TemporaryChairName { get; set; }

    /// <summary>Дата подписания протокола (protocol_signed_at).</summary>
    public DateTime? ProtocolSignedAt { get; set; }

    /// <summary>Дата окончания приёма бюллетеней (ballot_deadline).</summary>
    public DateTime? BallotDeadline { get; set; }

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Статус ОСА (status).</summary>
    public string Status { get; set; } = "DRAFT";

    /// <summary>Кто зафиксировал ОСА (finalized_by).</summary>
    public Guid? FinalizedBy { get; set; }

    /// <summary>Дата фиксации ОСА (finalized_at).</summary>
    public DateTime? FinalizedAt { get; set; }

    public OsaForm? OsaForm { get; set; }
}
