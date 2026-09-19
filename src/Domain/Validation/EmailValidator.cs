using System.Text.RegularExpressions;

namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Валидатор формата адреса электронной почты.
/// </summary>
public static class EmailValidator
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Проверяет, что email имеет корректный формат.
    /// Пустое или null-значение считается допустимым (поле необязательное).
    /// </summary>
    public static bool IsValid(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return true;

        return EmailRegex.IsMatch(email);
    }
}
