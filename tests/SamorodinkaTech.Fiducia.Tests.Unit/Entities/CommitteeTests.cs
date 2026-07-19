using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Entities;

/// <summary>
/// Тесты доменной сущности Committee: значения по умолчанию, типы поведения,
/// председатель и секретарь, деактивация.
/// </summary>
public class CommitteeTests
{
    /// <summary>
    /// Новая сущность Committee имеет корректные значения по умолчанию.
    /// </summary>
    [Fact]
    public void Committee_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var committee = new Committee();

        // Assert
        committee.Code.Should().BeEmpty();
        committee.Name.Should().BeEmpty();
        committee.BehaviorType.Should().Be(default);
        committee.IsActive.Should().BeTrue();
        committee.ChairId.Should().BeNull();
        committee.SecretaryId.Should().BeNull();
        committee.Members.Should().NotBeNull();
        committee.Members.Should().BeEmpty();
    }

    /// <summary>
    /// Комитет типа CONTROL имеет поведение CONTROL.
    /// </summary>
    [Fact]
    public void Committee_ControlType_ShouldHaveControlBehavior()
    {
        // Arrange & Act
        var committee = new Committee
        {
            Code = "AUDIT",
            Name = "Аудиторский комитет",
            BehaviorType = BehaviorType.CONTROL
        };

        // Assert
        committee.BehaviorType.Should().Be(BehaviorType.CONTROL);
    }

    /// <summary>
    /// Комитет типа STRATEGIC имеет поведение STRATEGIC.
    /// </summary>
    [Fact]
    public void Committee_StrategicType_ShouldHaveStrategicBehavior()
    {
        // Arrange & Act
        var committee = new Committee
        {
            Code = "STRATEGY",
            Name = "Стратегический комитет",
            BehaviorType = BehaviorType.STRATEGIC
        };

        // Assert
        committee.BehaviorType.Should().Be(BehaviorType.STRATEGIC);
    }

    /// <summary>
    /// Председатель и секретарь комитета — разные пользователи.
    /// </summary>
    [Fact]
    public void Committee_ChairAndSecretary_ShouldBeDifferentUsers()
    {
        // Arrange
        var committee = new Committee
        {
            ChairId = Guid.NewGuid(),
            SecretaryId = Guid.NewGuid()
        };

        // Act & Assert
        committee.ChairId.Should().NotBeNull();
        committee.SecretaryId.Should().NotBeNull();
        committee.ChairId!.Value.Should().NotBe(committee.SecretaryId!.Value);
    }

    /// <summary>
    /// Комитет можно деактивировать установкой IsActive = false.
    /// </summary>
    [Fact]
    public void Committee_CanBeDeactivated()
    {
        // Arrange
        var committee = new Committee { IsActive = true };

        // Act
        committee.IsActive = false;

        // Assert
        committee.IsActive.Should().BeFalse();
    }
}
