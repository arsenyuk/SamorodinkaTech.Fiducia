using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Services;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class NotificationTextBuilderTests
{
    private static readonly Guid OchnFormId = new("10000000-0000-0000-0000-000000000001");
    private static readonly Guid ZaOchnFormId = new("10000000-0000-0000-0000-000000000002");
    private static readonly Guid MixedFormId = new("10000000-0000-0000-0000-000000000003");

    private static RefMeetingForm MakeForm(Guid id, string code) => new() { Id = id, Code = code, Name = code };

    private static IApplicationDbContext CreateEmptyContext()
    {
        var options = new DbContextOptionsBuilder<FiduciaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new EmptyDbContext(options);
    }

    private readonly NotificationTextBuilder _sut = new(CreateEmptyContext());

    // ── BuildFirstMeetingSummons ──────────────────────────────────

    [Fact]
    public async Task BuildFirstMeetingSummons_OchnMeeting_ReturnsTitleAndBody()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingFormId = OchnFormId,
            MeetingForm = MakeForm(OchnFormId, "OCHN"),
            MeetingNumber = "001",
            VotingStartAt = new DateTime(2025, 6, 15, 10, 0, 0)
        };

        var (title, body) = await _sut.BuildFirstMeetingSummonsAsync(meeting, "ООО «Тест»");

        title.Should().Contain("ООО «Тест»");
        body.Should().Contain("первого (организационного) заседания");
        body.Should().Contain("15.06.2025");
        body.Should().Contain("очная");
    }

    [Fact]
    public void BuildFirstMeetingSummons_ZaOchnMeeting_ThrowsArgumentException()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingFormId = ZaOchnFormId,
            MeetingForm = MakeForm(ZaOchnFormId, "ZAOCHN")
        };

        Func<Task> act = () => _sut.BuildFirstMeetingSummonsAsync(meeting, "ООО «Тест»");

        act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*не может быть проведено*");
    }

    [Fact]
    public async Task BuildFirstMeetingSummons_NullNumber_UsesId()
    {
        var meeting = new Meeting
        {
            Id = new Guid("12345678-1234-1234-1234-123456789abc"),
            MeetingFormId = OchnFormId,
            MeetingForm = MakeForm(OchnFormId, "OCHN"),
            MeetingNumber = null
        };

        var (title, body) = await _sut.BuildFirstMeetingSummonsAsync(meeting, "Тест");

        title.Should().NotBeNullOrEmpty();
        body.Should().NotBeNullOrEmpty();
    }

    // ── BuildMeetingSummons ───────────────────────────────────────

    [Theory]
    [InlineData("OCHN", "очное")]
    [InlineData("ZAOCHN", "заочное")]
    [InlineData("MIXED", "смешанное")]
    public async Task BuildMeetingSummons_DifferentForms_ReturnsFormText(string formCode, string expectedForm)
    {
        var formId = formCode switch
        {
            "OCHN" => OchnFormId,
            "ZAOCHN" => ZaOchnFormId,
            "MIXED" => MixedFormId,
            _ => Guid.Empty
        };

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingFormId = formId,
            MeetingForm = MakeForm(formId, formCode),
            MeetingNumber = "002"
        };

        var (title, body) = await _sut.BuildMeetingSummonsAsync(meeting);

        body.Should().Contain(expectedForm);
    }

    [Fact]
    public async Task BuildMeetingSummons_WithVotingDates_IncludesVotingLine()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingFormId = OchnFormId,
            MeetingForm = MakeForm(OchnFormId, "OCHN"),
            MeetingNumber = "003",
            VotingStartAt = new DateTime(2025, 6, 15, 10, 0, 0),
            VotingEndAt = new DateTime(2025, 6, 20, 18, 0, 0)
        };

        var (_, body) = await _sut.BuildMeetingSummonsAsync(meeting);

        body.Should().Contain("Голосование:");
        body.Should().Contain("15.06.2025");
        body.Should().Contain("20.06.2025");
    }

    [Fact]
    public async Task BuildMeetingSummons_WithoutVotingDates_NoVotingLine()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingFormId = OchnFormId,
            MeetingForm = MakeForm(OchnFormId, "OCHN"),
            MeetingNumber = "004"
        };

        var (_, body) = await _sut.BuildMeetingSummonsAsync(meeting);

        body.Should().NotContain("Голосование:");
    }

    // ── BuildVoteReminder ─────────────────────────────────────────

    [Fact]
    public async Task BuildVoteReminder_WithDeadline_IncludesDeadline()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "005",
            VotingEndAt = new DateTime(2025, 6, 20, 18, 0, 0)
        };

        var (title, body) = await _sut.BuildVoteReminderAsync(meeting);

        title.Should().Contain("Напоминание");
        body.Should().Contain("20.06.2025 18:00");
    }

    [Fact]
    public async Task BuildVoteReminder_WithoutDeadline_UsesDefault()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "006"
        };

        var (_, body) = await _sut.BuildVoteReminderAsync(meeting);

        body.Should().Contain("установленный срок");
    }

    // ── BuildVoteDeadline ─────────────────────────────────────────

    [Fact]
    public async Task BuildVoteDeadline_ReturnsCorrectText()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "007"
        };

        var (title, body) = await _sut.BuildVoteDeadlineAsync(meeting);

        title.Should().Contain("Завершение голосования");
        body.Should().Contain("завершено");
    }

    // ── BuildProtocolSigned ───────────────────────────────────────

    [Fact]
    public async Task BuildProtocolSigned_ReturnsCorrectText()
    {
        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            MeetingNumber = "008"
        };

        var (title, body) = await _sut.BuildProtocolSignedAsync(meeting);

        title.Should().Contain("Протокол подписан");
        body.Should().Contain("подписан");
    }

    // ── BuildCommitteeProtocolSigned ──────────────────────────────

    [Fact]
    public async Task BuildCommitteeProtocolSigned_WithCode_IncludesCode()
    {
        var committee = new Committee
        {
            Id = Guid.NewGuid(),
            Code = "AUDIT",
            Name = "Аудиторский комитет"
        };

        var (title, body) = await _sut.BuildCommitteeProtocolSignedAsync(committee, "П-001");

        title.Should().Contain("AUDIT — Аудиторский комитет");
        body.Should().Contain("П-001");
    }

    [Fact]
    public async Task BuildCommitteeProtocolSigned_WithoutCode_UsesNameOnly()
    {
        var committee = new Committee
        {
            Id = Guid.NewGuid(),
            Code = "",
            Name = "Ревизионная комиссия"
        };

        var (title, body) = await _sut.BuildCommitteeProtocolSignedAsync(committee, "П-002");

        title.Should().Be("Протокол подписан — Ревизионная комиссия");
        body.Should().Contain("П-002");
    }

    // ── BuildChairmanNomination ───────────────────────────────────

    [Fact]
    public async Task BuildChairmanNomination_ReturnsCorrectText()
    {
        var (title, body) = await _sut.BuildChairmanNominationAsync("ООО «Тест»");

        title.Should().Contain("Председатель СД");
        title.Should().Contain("ООО «Тест»");
        body.Should().Contain("кандидатурам");
    }

    // ── BuildDeputyChairmanNomination ─────────────────────────────

    [Fact]
    public async Task BuildDeputyChairmanNomination_ReturnsCorrectText()
    {
        var (title, body) = await _sut.BuildDeputyChairmanNominationAsync("ООО «Тест»");

        title.Should().Contain("Заместитель председателя СД");
        body.Should().Contain("кандидатурам");
    }

    // ── BuildChairmanConsent ──────────────────────────────────────

    [Fact]
    public async Task BuildChairmanConsent_ReturnsCorrectText()
    {
        var (title, body) = await _sut.BuildChairmanConsentAsync("ООО «Тест»", "Иванов И.И.");

        title.Should().Contain("Согласие на должность Председателя СД");
        body.Should().Contain("Иванов И.И.");
        body.Should().Contain("согласие на выдвижение");
    }

    // ── BuildDeputyChairmanConsent ────────────────────────────────

    [Fact]
    public async Task BuildDeputyChairmanConsent_ReturnsCorrectText()
    {
        var (title, body) = await _sut.BuildDeputyChairmanConsentAsync("ООО «Тест»", "Петров П.П.");

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

// Minimal DbContext for testing with InMemory provider
public class EmptyDbContext : FiduciaDbContext
{
    public EmptyDbContext(DbContextOptions<FiduciaDbContext> options) : base(options) { }
}
