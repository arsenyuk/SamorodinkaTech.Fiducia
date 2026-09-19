using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис чтения данных профиля пользователя.
/// </summary>
public interface IProfileReadService
{
    /// <summary>Загрузить профиль текущего пользователя.</summary>
    Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default);
}

/// <summary>DTO профиля пользователя.</summary>
public record ProfileDto(
    User User,
    Person? Person,
    LegalEntity? LegalEntity,
    BoardParticipant? Participant,
    BoardParticipantShare? Share,
    string? BoardRoleName,
    List<IdentityDocument> IdentityDocuments,
    bool IsParticipant,
    List<ChangeHistoryItem> ChangeHistory);

/// <summary>Запись истории информирования.</summary>
public record ChangeHistoryItem(
    Guid Id,
    DateTime SubmittedAt,
    string? Status,
    string? ReviewComment,
    string? ReviewedByLogin,
    string Description);
