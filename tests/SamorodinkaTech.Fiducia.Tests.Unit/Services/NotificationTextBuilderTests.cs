using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class NotificationTextBuilderTests
{
    private readonly NotificationTextBuilder _sut = new();

    // ── BuildFirstMeetingSummons ──────────────────────────────────

    [Fact]
    public void BuildFirstMeetingSummons_OchnMeeting_ReturnsTitleAndBody()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingFormId = "OCHN",
            MeetingNumber = "001",
            VotingStartAt = new DateTime(2025, 6, 15, 10, 0, 0)
        };

        var (title, body) = _sut.BuildFirstMeetingSummons(meeting, "ООО «Тест»");

        title.Should().Contain("ООО «Тест»");
        body.Should().Contain("первого (организационного) заседания");
        body.Should().Contain("15.06.2025");
        body.Should().Contain("очная форма");
    }

    [Fact]
    public void BuildFirstMeetingSummons_ZaOchnMeeting_ThrowsArgumentException()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingFormId = "ZAOCHN"
        };

        Action act = () => _sut.BuildFirstMeetingSummons(meeting, "ООО «Тест»");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*не может быть проведено*");
    }

    [Fact]
    public void BuildFirstMeetingSummons_NullNumber_UsesId()
    {
        var meeting = new Meeting
        {
            Id = new Guid("12345678-1234-1234-1234-123456789abc"),
            MeetingFormId = "OCHN",
            MeetingNumber = null
        };

        var (title, body) = _sut.BuildFirstMeetingSummons(meeting, "Тест");

        title.Should().NotBeNullOrEmpty();
        body.Should().NotBeNullOrEmpty();
    }

    // ── BuildMeetingSummons ───────────────────────────────────────

    [Theory]
    [InlineData("OCHN", "очное")]
    [InlineData("ZAOCHN", "заочное")]
    [InlineData("MIXED", "смешанное")]
    public void BuildMeetingSummons_DifferentForms_ReturnsFormText(string formId, string expectedForm)
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingFormId = formId,
            MeetingNumber = "002"
        };

        var (title, body) = _sut.BuildMeetingSummons(meeting);

        body.Should().Contain(expectedForm);
    }

    [Fact]
    public void BuildMeetingSummons_WithVotingDates_IncludesVotingLine()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingFormId = "OCHN",
            MeetingNumber = "003",
            VotingStartAt = new DateTime(2025, 6, 15, 10, 0, 0),
            VotingEndAt = new DateTime(2025, 6, 20, 18, 0, 0)
        };

        var (_, body) = _sut.BuildMeetingSummons(meeting);

        body.Should().Contain("Голосование:");
        body.Should().Contain("15.06.2025");
        body.Should().Contain("20.06.2025");
    }

    [Fact]
    public void BuildMeetingSummons_WithoutVotingDates_NoVotingLine()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingFormId = "OCHN",
            MeetingNumber = "004"
        };

        var (_, body) = _sut.BuildMeetingSummons(meeting);

        body.Should().NotContain("Голосование:");
    }

    // ── BuildVoteReminder ─────────────────────────────────────────

    [Fact]
    public void BuildVoteReminder_WithDeadline_IncludesDeadline()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "005",
            VotingEndAt = new DateTime(2025, 6, 20, 18, 0, 0)
        };

        var (title, body) = _sut.BuildVoteReminder(meeting);

        title.Should().Contain("Напоминание");
        body.Should().Contain("20.06.2025 18:00");
    }

    [Fact]
    public void BuildVoteReminder_WithoutDeadline_UsesDefault()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "006"
        };

        var (_, body) = _sut.BuildVoteReminder(meeting);

        body.Should().Contain("установленный срок");
    }

    // ── BuildVoteDeadline ─────────────────────────────────────────

    [Fact]
    public void BuildVoteDeadline_ReturnsCorrectText()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "007"
        };

        var (title, body) = _sut.BuildVoteDeadline(meeting);

        title.Should().Contain("Завершение голосования");
        body.Should().Contain("завершено");
    }

    // ── BuildProtocolSigned ───────────────────────────────────────

    [Fact]
    public void BuildProtocolSigned_ReturnsCorrectText()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "008"
        };

        var (title, body) = _sut.BuildProtocolSigned(meeting);

        title.Should().Contain("Протокол подписан");
        body.Should().Contain("подписан");
    }

    // ── BuildCommitteeProtocolSigned ──────────────────────────────

    [Fact]
    public void BuildCommitteeProtocolSigned_WithCode_IncludesCode()
    {
        var committee = new Committee
        {
            Id = Guid.NewGuid(),
            Code = "AUDIT",
            Name = "Аудиторский комитет"
        };

        var (title, body) = _sut.BuildCommitteeProtocolSigned(committee, "П-001");

        title.Should().Contain("AUDIT — Аудиторский комитет");
        body.Should().Contain("П-001");
    }

    [Fact]
    public void BuildCommitteeProtocolSigned_WithoutCode_UsesNameOnly()
    {
        var committee = new Committee
        {
            Id = Guid.NewGuid(),
            Code = "",
            Name = "Ревизионная комиссия"
        };

        var (title, body) = _sut.BuildCommitteeProtocolSigned(committee, "П-002");

        title.Should().Be("Протокол подписан — Ревизионная комиссия");
        body.Should().Contain("П-002");
    }

    // ── BuildChairmanNomination ───────────────────────────────────

    [Fact]
    public void BuildChairmanNomination_ReturnsCorrectText()
    {
        var (title, body) = _sut.BuildChairmanNomination("ООО «Тест»");

        title.Should().Contain("Председатель СД");
        title.Should().Contain("ООО «Тест»");
        body.Should().Contain("кандидатурам");
    }

    // ── BuildDeputyChairmanNomination ─────────────────────────────

    [Fact]
    public void BuildDeputyChairmanNomination_ReturnsCorrectText()
    {
        var (title, body) = _sut.BuildDeputyChairmanNomination("ООО «Тест»");

        title.Should().Contain("Заместитель председателя СД");
        body.Should().Contain("кандидатурам");
    }

    // ── BuildChairmanConsent ──────────────────────────────────────

    [Fact]
    public void BuildChairmanConsent_ReturnsCorrectText()
    {
        var (title, body) = _sut.BuildChairmanConsent("ООО «Тест»", "Иванов И.И.");

        title.Should().Contain("Согласие на должность Председателя СД");
        body.Should().Contain("Иванов И.И.");
        body.Should().Contain("согласие на выдвижение");
    }

    // ── BuildDeputyChairmanConsent ────────────────────────────────

    [Fact]
    public void BuildDeputyChairmanConsent_ReturnsCorrectText()
    {
        var (title, body) = _sut.BuildDeputyChairmanConsent("ООО «Тест»", "Петров П.П.");

        title.Should().Contain("Согласие на должность Заместителя председателя СД");
        body.Should().Contain("Петров П.П.");
    }

    // ── BuildGeneral ──────────────────────────────────────────────

    [Fact]
    public void BuildGeneral_ReturnsProvidedTitleAndBody()
    {
        var (title, body) = _sut.BuildGeneral("Custom Title", "Custom Body");

        title.Should().Be("Custom Title");
        body.Should().Be("Custom Body");
    }
}
