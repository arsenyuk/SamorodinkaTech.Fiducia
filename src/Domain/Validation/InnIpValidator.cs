namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Валидатор ИНН индивидуального предпринимателя (12 цифр).
/// Алгоритм: 10-я цифра — контрольная по весам [7,2,4,10,3,5,9,4,6,8],
///            12-я цифра — контрольная по весам [3,7,2,4,10,3,5,9,4,6,8].
/// </summary>
public static class InnIpValidator
{
    public const int InnIpLength = 12;

    private static readonly int[] FirstWeights = { 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };
    private static readonly int[] SecondWeights = { 3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };

    public static (bool IsValid, string? Error) Validate(string? inn)
    {
        if (string.IsNullOrWhiteSpace(inn))
            return (false, "ИНН ИП обязателен");

        if (inn.Length != InnIpLength || !inn.All(char.IsDigit))
            return (false, $"ИНН ИП должен содержать ровно {InnIpLength} цифр");

        // Контроль 10-й цифры
        int sum = 0;
        for (int i = 0; i < 10; i++)
            sum += (inn[i] - '0') * FirstWeights[i];
        int expected10 = sum % 11 % 10;
        if (inn[10] - '0' != expected10)
            return (false, "Неверная контрольная сумма ИНН (10-я цифра)");

        // Контроль 12-й цифры
        sum = 0;
        for (int i = 0; i < 11; i++)
            sum += (inn[i] - '0') * SecondWeights[i];
        int expected12 = sum % 11 % 10;
        if (inn[11] - '0' != expected12)
            return (false, "Неверная контрольная сумма ИНН (12-я цифра)");

        return (true, null);
    }
}
