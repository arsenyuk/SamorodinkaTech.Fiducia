namespace SamorodinkaTech.Fiducia.Domain.Models.Edin;

/// <summary>Результат идентификации ЕДИН.</summary>
public record EdinPersonResult
{
    /// <summary>MasterId персоны (null при Ambiguous/Conflict).</summary>
    public Guid? MasterId { get; init; }

    /// <summary>Статус: Matched, Unmatched, Ambiguous, Conflict.</summary>
    public string Status { get; init; } = "";

    /// <summary>Есть ли дефекты данных.</summary>
    public bool HasDefects { get; init; }

    /// <summary>Описания дефектов.</summary>
    public IReadOnlyList<string> Defects { get; init; } = [];
}
