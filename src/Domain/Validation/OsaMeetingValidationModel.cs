namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Входная модель для серверной валидации сохранения ОСА.
/// </summary>
public record OsaMeetingValidationModel
{
    public int? ShareholdersCount { get; set; }
    public string? OkopfCode { get; set; }
    public int? BoardMinNumber { get; set; }
    public int? BoardMemberNumber { get; set; }
    public bool HasGosaInterval { get; set; }
    public bool IsAbsentee { get; set; }

    /// <summary>Год проведения ГОСА для проверки уникальности.</summary>
    public int? GosaYear { get; set; }

    // Директора по типам
    public bool ExecutiveDirectorsParticipate { get; set; }
    public int? ExecutiveDirectorsCount { get; set; }
    public bool NonExecutiveDirectorsParticipate { get; set; }
    public int? NonExecutiveDirectorsCount { get; set; }
    public bool IndependentDirectorsParticipate { get; set; }
    public int? IndependentDirectorsCount { get; set; }
}
