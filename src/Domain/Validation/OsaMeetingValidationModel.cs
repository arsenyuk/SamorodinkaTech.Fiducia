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
}
