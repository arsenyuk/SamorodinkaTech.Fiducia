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
    /// <exception cref="ArgumentException">При некорректном формате или значении.</exception>
    public static (decimal percent, string? fraction) Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Значение не может быть пустым");

        var trimmed = input.Trim().TrimEnd('%').Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException("Значение не может быть пустым");

        // ── Дробь ──────────────────────────────────────────
        if (trimmed.Contains('/'))
        {
            var parts = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                throw new ArgumentException("Некорректный формат дроби. Используйте формат: числитель/знаменатель (например, 1/3)");

            if (!decimal.TryParse(parts[0], System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var numerator))
                throw new ArgumentException($"Некорректное числитель: {parts[0]}");

            if (!decimal.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var denominator))
                throw new ArgumentException($"Некорректный знаменатель: {parts[1]}");

            if (numerator < 0 || denominator < 0)
                throw new ArgumentException("Доля не может быть отрицательной");

            if (denominator == 0)
                throw new ArgumentException("Знаменатель не может быть нулём");

            if (numerator == 0)
                throw new ArgumentException("Доля не может быть равна нулю");

            if (numerator > denominator)
                throw new ArgumentException("Доля не может превышать 100% (дробь больше 1)");

            var percent = Math.Round(numerator * 100m / denominator, 2, MidpointRounding.AwayFromZero);

            return (percent, trimmed);
        }

        // ── Процент ────────────────────────────────────────
        if (!decimal.TryParse(trimmed, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
            throw new ArgumentException($"Некорректное числовое значение: {trimmed}");

        if (value < 0)
            throw new ArgumentException("Доля не может быть отрицательной");

        if (value == 0)
            throw new ArgumentException("Доля не может быть равна нулю");

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

    /// <summary>
    /// Серверная валидация размера доли (decimal?).
    /// Правила идентичны клиентским (Parse): null/0/отрицательная/> 100.
    /// </summary>
    /// <param name="value">Значение доли в процентах.</param>
    /// <exception cref="ArgumentException">При некорректном значении.</exception>
    public static void ValidateServer(decimal? value)
    {
        if (value is null)
            throw new ArgumentException("Размер доли обязателен");

        if (value < 0)
            throw new ArgumentException("Доля не может быть отрицательной");

        if (value == 0)
            throw new ArgumentException("Доля не может быть равна нулю");

        if (value > 100)
            throw new ArgumentException("Доля не может превышать 100%");
    }
}
