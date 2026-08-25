namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Анкета соответствия критериям независимости (independence_declarations).
/// Привязана к участнику экосистемы.
/// </summary>
public class IndependenceDeclaration
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на участника экосистемы (ecosystem_participant_id).</summary>
    public Guid EcosystemParticipantId { get; set; }

    /// <summary>Скрытые доли в акционерных капиталах других организаций (hidden_shares).</summary>
    public string? HiddenShares { get; set; }

    /// <summary>Родственные связи с топ-менеджментом компании (family_connections).</summary>
    public string? FamilyConnections { get; set; }

    /// <summary>Участие в других советах директоров (other_boards).</summary>
    public string? OtherBoards { get; set; }

    /// <summary>Подтверждение отсутствия судимости (no_criminal_record).</summary>
    public bool NoCriminalRecord { get; set; }

    /// <summary>Подтверждение отсутствия фактов банкротства (no_bankruptcy).</summary>
    public bool NoBankruptcy { get; set; }

    /// <summary>Анкета заполнена (completed).</summary>
    public bool Completed { get; set; }

    /// <summary>Дата заполнения (completed_at).</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    public EcosystemParticipant? EcosystemParticipant { get; set; }
}