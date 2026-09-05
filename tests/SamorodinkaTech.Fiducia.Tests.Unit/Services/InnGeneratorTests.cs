using FluentAssertions;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Юнит-тесты генерации ИНН для E2E-тестов.
/// </summary>
public class InnGeneratorTests
{
    [Fact]
    public void GenerateValidInn_ShouldProduceValid10DigitInn()
    {
        for (int i = 0; i < 100; i++)
        {
            var inn = InnTestHelper.GenerateValidInn();
            inn.Should().HaveLength(10, "ИНН юридического лица должен быть 10 цифр");
            inn.Should().MatchRegex(@"^\d{10}$", "ИНН должен содержать только цифры");
        }
    }

    [Fact]
    public void GenerateValidInn_ShouldHaveCorrectChecksum()
    {
        for (int i = 0; i < 100; i++)
        {
            var inn = InnTestHelper.GenerateValidInn();
            var digits = inn.Select(c => c - '0').ToArray();
            int expectedCheck = (2 * digits[0] + 4 * digits[1] + 10 * digits[2] + 3 * digits[3] +
                                 5 * digits[4] + 9 * digits[5] + 4 * digits[6] + 6 * digits[7] + 8 * digits[8]) % 11 % 10;
            digits[9].Should().Be(expectedCheck, $"контрольная сумма ИНН {inn} некорректна");
        }
    }

    [Fact]
    public void GenerateValidInn_ShouldStartWithRegion77()
    {
        var inn = InnTestHelper.GenerateValidInn();
        inn.Should().StartWith("77", "ИНН должен начинаться с кода региона 77");
    }
}

/// <summary>
/// Хелпер для генерации тестовых ИНН.
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
