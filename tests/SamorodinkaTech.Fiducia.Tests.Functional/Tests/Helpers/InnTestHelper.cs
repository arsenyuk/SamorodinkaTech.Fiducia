namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для генерации тестовых ИНН. Используется в E2E-тестах.
/// </summary>
public static class InnTestHelper
{
    /// <summary>Генерирует валидный 10-значный ИНН юридического лица (регион 77, ИФНС 01).</summary>
    public static string GenerateValidInn()
    {
        var rnd = Random.Shared;
        var digits = new[] { 7, 7, 0, 1, rnd.Next(0, 10), rnd.Next(0, 10), rnd.Next(0, 10), rnd.Next(0, 10), rnd.Next(0, 10) };
        int check = (2 * digits[0] + 4 * digits[1] + 10 * digits[2] + 3 * digits[3] + 5 * digits[4] + 9 * digits[5] + 4 * digits[6] + 6 * digits[7] + 8 * digits[8]) % 11 % 10;
        return $"{digits[0]}{digits[1]}{digits[2]}{digits[3]}{digits[4]}{digits[5]}{digits[6]}{digits[7]}{digits[8]}{check}";
    }
}
