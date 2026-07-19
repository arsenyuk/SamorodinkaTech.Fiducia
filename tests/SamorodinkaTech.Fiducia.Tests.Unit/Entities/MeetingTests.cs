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
        meeting.MeetingFormId.Should().Be(default(Guid));
        meeting.MeetingForm.Should().BeNull();
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
        var formId = Guid.NewGuid();

        var meeting = new Meeting
        {
            MeetingFormId = formId
        };

        meeting.MeetingFormId.Should().Be(formId);
    }

    /// <summary>
    /// Заседание может иметь навигационное свойство к справочнику форм.
    /// </summary>
    [Fact]
    public void Meeting_ShouldLinkToRefMeetingForm()
    {
        var form = new RefMeetingForm { Id = Guid.NewGuid(), Code = "MIXED", Name = "Смешанное" };

        var meeting = new Meeting
        {
            MeetingFormId = form.Id,
            MeetingForm = form
        };

        meeting.MeetingFormId.Should().Be(form.Id);
        meeting.MeetingForm.Should().Be(form);
        meeting.MeetingForm.Code.Should().Be("MIXED");
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
