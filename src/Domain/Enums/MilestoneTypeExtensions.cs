namespace SamorodinkaTech.Fiducia.Domain.Enums;

/// <summary>
/// Методы расширения для отображения значений <see cref="MilestoneType"/>.
/// </summary>
public static class MilestoneTypeExtensions
{
    private static readonly Dictionary<MilestoneType, string> Markers = new()
    {
        [MilestoneType.REGULAR] = "▼В",
        [MilestoneType.PHASE_GATE] = "▼ВР",
        [MilestoneType.LEGAL] = "⚡ЮВ",
        [MilestoneType.CONTROL] = "⚡КВ",
        [MilestoneType.INTEGRATION] = "▼ИН"
    };

    private static readonly Dictionary<MilestoneType, string> DisplayNames = new()
    {
        [MilestoneType.REGULAR] = "Обычная веха",
        [MilestoneType.PHASE_GATE] = "Межэтапная веха",
        [MilestoneType.LEGAL] = "Юридическая веха",
        [MilestoneType.CONTROL] = "Контрольная веха",
        [MilestoneType.INTEGRATION] = "Интеграционная веха"
    };

    /// <summary>Маркер вехи для ID (например, «▼В», «⚡ЮВ»).</summary>
    public static string GetMarker(this MilestoneType type) =>
        Markers.TryGetValue(type, out var marker) ? marker : type.ToString();

    /// <summary>Отображаемое имя (например, «Юридическая веха»).</summary>
    public static string GetDisplayName(this MilestoneType type) =>
        DisplayNames.TryGetValue(type, out var name) ? name : type.ToString();
}
