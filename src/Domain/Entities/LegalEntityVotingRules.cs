using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Правила голосования в совете директоров, индивидуальные для юридического лица (legal_entity_voting_rules).
/// Задаются уставом общества согласно ст. 68, 69 208-ФЗ.
/// </summary>
public class LegalEntityVotingRules
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Юридическое лицо.</summary>
    public LegalEntity? LegalEntity { get; set; }

    /// <summary>Порог кворума в процентах от числа избранных членов СД (quorum_percent).</summary>
    public int QuorumPercent { get; set; } = 50;

    /// <summary>Решающий голос председателя при равенстве голосов (chair_tiebreaker).</summary>
    public bool ChairTiebreaker { get; set; }

    /// <summary>Учёт письменных мнений отсутствующих членов при определении кворума (absentee_opinions).</summary>
    public bool AbsenteeOpinions { get; set; }

    /// <summary>Порог квалифицированного большинства в процентах (qualified_majority_percent).</summary>
    public int QualifiedMajorityPercent { get; set; } = 75;

    /// <summary>Разрешено очное голосование — всегда включено (in_person_allowed).</summary>
    public bool InPersonAllowed { get; set; } = true;

    /// <summary>Разрешено заочное голосование (absentee_allowed).</summary>
    public bool AbsenteeAllowed { get; set; }

    /// <summary>Разрешено смешанное голосование (mixed_allowed).</summary>
    public bool MixedAllowed { get; set; }

    /// <summary>Вид документооборота, применяемый юридическим лицом (document_flow).</summary>
    public DocumentFlowType DocumentFlow { get; set; } = DocumentFlowType.Paper;

    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
