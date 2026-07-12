namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Предложение кандидатур на выборы должностных лиц СД (election_proposals).
/// </summary>
public class ElectionProposal
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на Совет директоров (board_of_directors_id).</summary>
    public Guid BoardOfDirectorsId { get; set; }

    /// <summary>Должность, на которую проводится избрание (position): CHAIR, DEPUTY_CHAIR, SECRETARY.</summary>
    public string Position { get; set; } = default!;

    /// <summary>Статус (status): OPEN, CLOSED.</summary>
    public string Status { get; set; } = "OPEN";

    /// <summary>Дата и время создания (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ElectionCandidacy> Candidacies { get; set; } = new List<ElectionCandidacy>();
}
