namespace SamorodinkaTech.Fiducia.Infrastructure.Common;

/// <summary>
/// Парсинг и форматирование размера доли.
/// Поддерживает два формата ввода: простая дробь (1/3) и процент (50, 12.5).
/// </summary>
public static class ShareParser
{
    /// <summary>
    /// Парсит ввод пользователя: дробь "1/3" или процент "50" / "12.5%".
    /// </summary>
    /// <param name="input">Строка ввода.</param>
    /// <returns>Процент (decimal) и исходная дробь (если была).</returns>
    /// <exception cref="ArgumentException">При некорректном формате.</exception>
    public static (decimal percent, string? fraction) Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Значение не может быть пустым");

        var trimmed = input.Trim().TrimEnd('%').Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException("Значение не может быть пустым");

        if (trimmed.Contains('/'))
        {
            var parts = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new ArgumentException("Некорректный формат дроби. Используйте формат: числитель/знаменатель (например, 1/3)");

            if (!decimal.TryParse(parts[0], System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var numerator))
                throw new ArgumentException($"Некорректное числово: {parts[0]}");

            if (!decimal.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var denominator))
                throw new ArgumentException($"Некорректный знаменатель: {parts[1]}");

            if (denominator == 0)
                throw new ArgumentException("Знаменатель не может быть нулём");

            var percent = Math.Round(numerator * 100m / denominator, 2, MidpointRounding.AwayFromZero);

            if (percent <= 0)
                throw new ArgumentException("Доля должна быть больше нуля");

            if (percent > 100)
                throw new ArgumentException("Доля не может превышать 100%");

            return (percent, trimmed);
        }

        if (!decimal.TryParse(trimmed, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
            throw new ArgumentException($"Некорректное числовое значение: {trimmed}");

        if (value <= 0)
            throw new ArgumentException("Доля должна быть больше нуля");

        if (value > 100)
            throw new ArgumentException("Доля не может превышать 100%");

        return (value, null);
    }

    /// <summary>
    /// Форматирует долю для отображения: дробь (если есть) или процент.
    /// </summary>
    public static string Format(decimal? percent, string? fraction)
    {
        if (!string.IsNullOrWhiteSpace(fraction))
            return fraction;

        if (percent.HasValue)
            return $"{percent.Value:0.##}%";

        return "—";
    }
}
