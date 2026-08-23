namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Валидатор ОГРНИП (15 цифр).
/// Алгоритм: первые 14 цифр умножаются на веса [1..14], сумма mod 10 = последняя цифра.
/// </summary>
public static class OgrnipValidator
{
    public const int OgrnipLength = 15;

    public static (bool IsValid, string? Error) Validate(string? ogrnip)
    {
        if (string.IsNullOrWhiteSpace(ogrnip))
            return (true, null); // ОГРНИП необязателен

        if (ogrnip.Length != OgrnipLength || !ogrnip.All(char.IsDigit))
            return (false, $"ОГРНИП должен содержать ровно {OgrnipLength} цифр");

        int sum = 0;
        for (int i = 0; i < 14; i++)
            sum += (ogrnip[i] - '0') * (i + 1);

        int expected = sum % 10;
        if (ogrnip[14] - '0' != expected)
            return (false, "Неверная контрольная сумма ОГРНИП");

        return (true, null);
    }
}
