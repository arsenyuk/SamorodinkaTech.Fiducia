using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Entities;

/// <summary>
/// Тесты доменной сущности User: значения по умолчанию, флаг внешнего директора,
/// фиксация даты подписания ПЭП.
/// </summary>
public class UserTests
{
    /// <summary>
    /// Новая сущность User имеет корректные значения по умолчанию.
    /// </summary>
    [Fact]
    public void User_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.LastName.Should().BeEmpty();
        user.FirstName.Should().BeEmpty();
        user.MiddleName.Should().BeNull();
        user.Email.Should().BeEmpty();
        user.Phone.Should().BeEmpty();
        user.IsExternal.Should().BeFalse();
        user.PepAgreementSigned.Should().BeFalse();
        user.PepSignedAt.Should().BeNull();
        user.UserRoles.Should().NotBeNull();
        user.UserRoles.Should().BeEmpty();
    }

    /// <summary>
    /// Пользователь с флагом IsExternal = true является внешним директором.
    /// </summary>
    [Fact]
    public void User_WithExternalFlag_ShouldBeExternal()
    {
        // Arrange & Act
        var user = new User
        {
            LastName = "Козлов",
            FirstName = "Алексей",
            IsExternal = true
        };

        // Assert
        user.IsExternal.Should().BeTrue();
    }

    /// <summary>
    /// При подписании ПЭП фиксируется дата подписания.
    /// </summary>
    [Fact]
    public void User_PepAgreementSigned_ShouldTrackDate()
    {
        // Arrange
        var signedAt = DateTime.UtcNow;

        // Act
        var user = new User
        {
            PepAgreementSigned = true,
            PepSignedAt = signedAt
        };

        // Assert
        user.PepAgreementSigned.Should().BeTrue();
        user.PepSignedAt.Should().Be(signedAt);
    }
}
