using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Validation;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Validation;

/// <summary>
/// Тесты валидатора EmailValidator: проверка формата адреса электронной почты.
/// </summary>
public class EmailValidatorTests
{
    /// <summary>
    /// Пустое значение — допустимо (поле необязательное).
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_NullOrEmpty_ReturnsTrue(string? email)
    {
        EmailValidator.IsValid(email).Should().BeTrue();
    }

    /// <summary>
    /// Корректные email-адреса проходят валидацию.
    /// </summary>
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@domain.org")]
    [InlineData("admin@company.ru")]
    [InlineData("name+tag@sub.domain.com")]
    [InlineData("a@b.co")]
    public void IsValid_ValidEmail_ReturnsTrue(string email)
    {
        EmailValidator.IsValid(email).Should().BeTrue();
    }

    /// <summary>
    /// Некорректные email-адреса не проходят валидацию.
    /// </summary>
    [Theory]
    [InlineData("user")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@domain")]
    [InlineData("user domain@example.com")]
    [InlineData("user@.com")]
    [InlineData("user@com.")]
    public void IsValid_InvalidEmail_ReturnsFalse(string email)
    {
        EmailValidator.IsValid(email).Should().BeFalse();
    }
}
