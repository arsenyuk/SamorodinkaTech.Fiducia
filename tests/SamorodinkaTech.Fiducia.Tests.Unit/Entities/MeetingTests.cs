using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Entities;

/// <summary>
/// Тесты доменной сущности Meeting: значения по умолчанию, формы проведения,
/// временные рамки голосования, поддержка всех статусов.
/// </summary>
public class MeetingTests
{
    /// <summary>
    /// Новая сущность Meeting имеет корректные значения по умолчанию.
    /// </summary>
    [Fact]
    public void Meeting_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var meeting = new Meeting();

        // Assert
        meeting.MeetingNumber.Should().BeNull();
        meeting.MeetingForm.Should().Be(default);
        meeting.Status.Should().Be(MeetingStatus.DRAFT);
        meeting.VotingStartAt.Should().BeNull();
        meeting.VotingEndAt.Should().BeNull();
        meeting.CreatedBy.Should().BeNull();
        meeting.AgendaQuestions.Should().NotBeNull();
        meeting.AgendaQuestions.Should().BeEmpty();
    }

    /// <summary>
    /// Заседание с формой ОЧН (очное) сохраняет MeetingForm.OCHN.
    /// </summary>
    [Fact]
    public void Meeting_InPersonForm_ShouldHaveOCHNType()
    {
        // Arrange & Act
        var meeting = new Meeting
        {
            MeetingForm = MeetingForm.OCHN
        };

        // Assert
        meeting.MeetingForm.Should().Be(MeetingForm.OCHN);
    }

    /// <summary>
    /// Заседание с формой ЗАОЧН (заочное) сохраняет MeetingForm.ZAOCHN.
    /// </summary>
    [Fact]
    public void Meeting_AbsenteeForm_ShouldHaveZAOCHNType()
    {
        // Arrange & Act
        var meeting = new Meeting
        {
            MeetingForm = MeetingForm.ZAOCHN
        };

        // Assert
        meeting.MeetingForm.Should().Be(MeetingForm.ZAOCHN);
    }

    /// <summary>
    /// Дата окончания голосования должна быть позже даты начала.
    /// </summary>
    [Fact]
    public void Meeting_VotingEndAt_ShouldBeAfterVotingStartAt()
    {
        // Arrange
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddDays(4);

        // Act
        var meeting = new Meeting
        {
            VotingStartAt = startAt,
            VotingEndAt = endAt
        };

        // Assert
        meeting.VotingEndAt.Should().BeAfter(meeting.VotingStartAt!.Value);
    }

    /// <summary>
    /// Заседание поддерживает все статусы жизненного цикла.
    /// </summary>
    [Theory]
    [InlineData(MeetingStatus.DRAFT)]
    [InlineData(MeetingStatus.NOTIFIED)]
    [InlineData(MeetingStatus.VOTING)]
    [InlineData(MeetingStatus.PROTOCOL)]
    [InlineData(MeetingStatus.ARCHIVE)]
    public void Meeting_ShouldSupportAllStatuses(MeetingStatus status)
    {
        // Arrange & Act
        var meeting = new Meeting { Status = status };

        // Assert
        meeting.Status.Should().Be(status);
    }
}
