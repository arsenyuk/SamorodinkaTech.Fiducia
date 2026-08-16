namespace SamorodinkaTech.Fiducia.Domain.Enums;

/// <summary>
/// Единица измерения срока: календарный или рабочий день.
/// </summary>
public enum MeasurementUnit
{
    /// <summary>Календарный день (включает выходные и праздники).</summary>
    CALENDAR = 0,

    /// <summary>Рабочий день (исключает выходные и праздники).</summary>
    BUSINESS = 1
}
