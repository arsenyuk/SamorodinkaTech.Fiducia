using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Данные для сохранения собрания: поля формы и состав СД.
/// Используется IMeetingSaveService.
/// </summary>
public class MeetingSaveModel
{
    public Guid MeetingId { get; init; }

    // ── GOsa ──────────────────────────────────────────────────────
    public bool IsGosa { get; init; }
    public int? GosaYear { get; init; }
    public DateOnly GosaStart { get; init; }
    public DateOnly GosaEnd { get; init; }
    public string? Title { get; init; }

    // ── Параметры ────────────────────────────────────────────────
    public int? ShareholdersCount { get; init; }
    public int? BoardMemberNumber { get; init; }
    public bool ExecutiveDirectorsParticipate { get; init; }
    public int? ExecutiveDirectorsCount { get; init; }
    public bool NonExecutiveDirectorsParticipate { get; init; }
    public int? NonExecutiveDirectorsCount { get; init; }
    public bool IndependentDirectorsParticipate { get; init; }
    public int? IndependentDirectorsCount { get; init; }
    public bool ShareholdersListReceived { get; init; }
    public bool AbsenteeVoting { get; init; }
    public DateTime? BallotDeadline { get; init; }

    // ── Проведение ───────────────────────────────────────────────
    public bool OsaHeld { get; init; }
    public bool ProtocolSigned { get; init; }
    public DateTime? ProtocolSignedAt { get; init; }
    public bool DeputyChairProvided { get; init; }
    public bool SecretaryProvided { get; init; }
    public bool SecretarySignsProtocols { get; init; }
    public bool TemporaryChairProvided { get; init; }
    public string? TemporaryChairSelection { get; init; }

    // ── Состав СД ────────────────────────────────────────────────
    public bool BoardCompositionApproved { get; init; }
    public bool BoardMandatory { get; init; }
    public bool BoardApproved { get; init; }
    public List<BoardMemberRowModel> BoardMembers { get; init; } = new();
}

/// <summary>
/// Строка состава СД для сохранения.
/// </summary>
public class BoardMemberRowModel
{
    public string Name { get; init; } = "";
    public Guid? MemberTypeId { get; init; }
    public Guid? RoleId { get; init; }
    public string? Account { get; init; }
    public string? Email { get; init; }
    public Guid? UserId { get; init; }
    public string? StartedAt { get; init; }
}

/// <summary>
/// Результат сохранения собрания.
/// </summary>
public class MeetingSaveResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public Guid? BoardOfDirectorsId { get; init; }
}

/// <summary>
/// Сервис сохранения собрания: обновление OsaMeeting, BoardOfDirectors, BoardMembers.
/// Валидация остаётся на уровне страницы.
/// </summary>
public interface IMeetingSaveService
{
    /// <summary>
    /// Сохраняет собрание: обновляет сущность OsaMeeting, создаёт/обновляет BoardOfDirectors,
    /// заменяет состав BoardMembers + BoardMemberAppointments.
    /// Проверяет уникальность года ГОСА (DB-валидация).
    /// </summary>
    Task<MeetingSaveResult> SaveAsync(
        MeetingSaveModel model,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Сервис загрузки данных собрания для редактирования.
/// </summary>
public interface IMeetingLoadService
{
    /// <summary>
    /// Загружает все данные, необходимые для формы редактирования собрания:
    /// OsaMeeting с юридическим лицом, состав СД, назначения, BoardOfDirectors.
    /// </summary>
    Task<MeetingEditData?> LoadAsync(
        Guid meetingId,
        CancellationToken cancellationToken = default);
}

/// <summary>
    /// Данные собрания, загруженные для редактирования.
    /// </summary>
public class MeetingEditData
{
    public OsaMeeting Meeting { get; init; } = null!;
    public LegalEntity LegalEntity { get; init; } = null!;
    public string OkopfCode { get; init; } = "";
    public bool IsPao { get; init; }
    public BoardOfDirectors? BoardOfDirectors { get; init; }
    public List<BoardMemberRowModel> BoardMembers { get; init; } = new();
}