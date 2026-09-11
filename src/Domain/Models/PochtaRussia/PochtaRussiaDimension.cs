namespace SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

/// <summary>
/// Линейные размеры почтового отправления (сантиметры).
/// </summary>
public class PochtaRussiaDimension
{
    /// <summary>Высота в сантиметрах.</summary>
    public int Height { get; init; }

    /// <summary>Длина в сантиметрах.</summary>
    public int Length { get; init; }

    /// <summary>Ширина в сантиметрах.</summary>
    public int Width { get; init; }
}
