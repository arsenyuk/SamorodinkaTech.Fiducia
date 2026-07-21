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
        meeting.MeetingFormId.Should().Be(string.Empty);
        meeting.Status.Should().Be(MeetingStatus.DRAFT);
        meeting.VotingStartAt.Should().BeNull();
        meeting.VotingEndAt.Should().BeNull();
        meeting.CreatedBy.Should().BeNull();
        meeting.AgendaQuestions.Should().NotBeNull();
        meeting.AgendaQuestions.Should().BeEmpty();
    }

    /// <summary>
    /// Заседание с формой ОЧН (очное) сохраняет MeetingFormId.
    /// </summary>
    [Fact]
    public void Meeting_InPersonForm_ShouldSetFormId()
    {
        var meeting = new Meeting
        {
            MeetingFormId = "OCHN"
        };

        meeting.MeetingFormId.Should().Be("OCHN");
    }

    /// <summary>
    /// Форма проведения хранится как код (OCHN/ZAOCHN) в соответствии со схемой БД.
    /// </summary>
    [Fact]
    public void Meeting_ShouldStoreFormCodes()
    {
        var meeting = new Meeting
        {
            MeetingFormId = "ZAOCHN"
        };

        meeting.MeetingFormId.Should().Be("ZAOCHN");
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
