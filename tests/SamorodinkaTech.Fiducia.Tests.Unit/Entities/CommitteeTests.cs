using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Entities;

public class CommitteeTests
{
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
