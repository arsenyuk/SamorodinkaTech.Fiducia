namespace SamorodinkaTech.Fiducia.Domain.Enums;

/// <summary>
/// Методы расширения для отображения значений <see cref="MeasurementUnit"/>.
/// </summary>
public static class MeasurementUnitExtensions
{
    private static readonly Dictionary<MeasurementUnit, string> DisplayNames = new()
    {
        [MeasurementUnit.CALENDAR] = "День (календарный)",
        [MeasurementUnit.BUSINESS] = "Рабочий день"
    };

    private static readonly Dictionary<MeasurementUnit, string> ShortNames = new()
    {
        [MeasurementUnit.CALENDAR] = "календ. дн.",
        [MeasurementUnit.BUSINESS] = "раб. дн."
    };

    /// <summary>Полное отображаемое имя (например, «День (календарный)»).</summary>
    public static string GetDisplayName(this MeasurementUnit unit) =>
        DisplayNames.TryGetValue(unit, out var name) ? name : unit.ToString();

    /// <summary>Краткое имя для списка (например, «календ. дн.»).</summary>
    public static string GetShortName(this MeasurementUnit unit) =>
        ShortNames.TryGetValue(unit, out var name) ? name : unit.ToString();
}
