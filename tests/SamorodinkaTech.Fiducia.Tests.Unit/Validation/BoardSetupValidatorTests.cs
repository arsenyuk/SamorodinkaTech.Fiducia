using FluentAssertions;
using SamorodinkaTech.Fiducia.BoardPortal;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Validation;

/// <summary>
/// Unit-тесты валидатора состава Совета директоров (BoardSetupValidator).
/// Проверяет бизнес-правила назначения ролей: ровно 1 Председатель,
/// не более 1 Зам. председателя, не более 1 Секретаря, отсутствие дублей.
/// </summary>
public class BoardSetupValidatorTests
{
    // ─── Пустой список ──────────────────────────────────────────────────

    [Fact]
    public void ValidateSync_EmptyList_ShouldRequireAtLeastOneChair()
    {
        var result = BoardSetupValidator.ValidateSync(new());

        result.Should().ContainSingle(e => e.Contains("Председатель СД"));
    }

    [Fact]
    public void ValidateSync_NullList_ShouldRequireAtLeastOneChair()
    {
        var result = BoardSetupValidator.ValidateSync(null!);

        result.Should().ContainSingle(e => e.Contains("Председатель СД"));
    }

    // ─── Ровно 1 Председатель ──────────────────────────────────────────

    [Fact]
    public void ValidateSync_OneChair_ShouldPass()
    {
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "CHAIR" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSync_TwoChairs_ShouldFail()
    {
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "CHAIR" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "CHAIR" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().ContainSingle(e => e.Contains("ровно 1 Председатель"));
    }

    [Fact]
    public void ValidateSync_NoChair_ShouldFail()
    {
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "MEMBER" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().ContainSingle(e => e.Contains("ровно 1 Председатель"));
    }

    // ─── Не более 1 Зам. председателя ──────────────────────────────────

    [Fact]
    public void ValidateSync_OneChairAndOneDeputy_ShouldPass()
    {
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "CHAIR" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "DEPUTY_CHAIR" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSync_TwoDeputies_ShouldFail()
    {
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "CHAIR" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "DEPUTY_CHAIR" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "DEPUTY_CHAIR" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().Contain(e => e.Contains("Не более 1 Зам. председателя"));
    }

    // ─── Не более 1 Секретаря ──────────────────────────────────────────

    [Fact]
    public void ValidateSync_OneChairAndOneSecretary_ShouldPass()
    {
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "CHAIR" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "SECRETARY" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSync_TwoSecretaries_ShouldFail()
    {
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "CHAIR" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "SECRETARY" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "SECRETARY" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().Contain(e => e.Contains("Не более 1 Секретаря"));
    }

    // ─── Дубли ─────────────────────────────────────────────────────────

    [Fact]
    public void ValidateSync_DuplicateParticipantAndRole_ShouldFail()
    {
        var participantId = Guid.NewGuid();
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = participantId, RoleCode = "CHAIR" },
            new() { ParticipantId = participantId, RoleCode = "CHAIR" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().Contain(e => e.Contains("дублированные"));
    }

    [Fact]
    public void ValidateSync_SameParticipantDifferentRoles_ShouldPass()
    {
        var participantId = Guid.NewGuid();
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = participantId, RoleCode = "CHAIR" },
            new() { ParticipantId = participantId, RoleCode = "DEPUTY_CHAIR" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().BeEmpty();
    }

    // ─── Полный состав (Вариант 3: Председатель + Секретарь) ───────────

    [Fact]
    public void ValidateSync_ChairAndSecretaryAndMembers_ShouldPass()
    {
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "CHAIR" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "SECRETARY" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "MEMBER" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "MEMBER" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().BeEmpty();
    }

    // ─── Полный состав (Вариант 2: Председатель + Зам. председателя) ───

    [Fact]
    public void ValidateSync_ChairAndDeputyAndMembers_ShouldPass()
    {
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "CHAIR" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "DEPUTY_CHAIR" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "MEMBER" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().BeEmpty();
    }

    // ─── Все три ошибки сразу ──────────────────────────────────────────

    [Fact]
    public void ValidateSync_MultipleErrors_ShouldReturnAll()
    {
        var participantId = Guid.NewGuid();
        var assignments = new List<BoardSetupAssignment>
        {
            new() { ParticipantId = participantId, RoleCode = "DEPUTY_CHAIR" },
            new() { ParticipantId = participantId, RoleCode = "DEPUTY_CHAIR" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "SECRETARY" },
            new() { ParticipantId = Guid.NewGuid(), RoleCode = "SECRETARY" }
        };

        var result = BoardSetupValidator.ValidateSync(assignments);

        result.Should().Contain(e => e.Contains("ровно 1 Председатель"));
        result.Should().Contain(e => e.Contains("Не более 1 Зам. председателя"));
        result.Should().Contain(e => e.Contains("Не более 1 Секретаря"));
    }
}
