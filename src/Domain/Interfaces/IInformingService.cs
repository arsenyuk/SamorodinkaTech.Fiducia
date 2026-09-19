using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис информирования об изменении сведений участника.
/// </summary>
public interface IInformingService
{
    /// <summary>Загрузить данные текущего участника для формы информирования.</summary>
    Task<InformingParticipantDto?> GetCurrentParticipantAsync(
        Guid userId, Guid legalEntityId, CancellationToken ct = default);

    /// <summary>Получить историю информирования участника.</summary>
    Task<List<ChangeHistoryItem>> GetChangeHistoryAsync(
        Guid participantId, CancellationToken ct = default);
}

/// <summary>DTO участника для формы информирования.</summary>
public record InformingParticipantDto(
    Guid ParticipantId,
    string? ParticipantType,
    string? FullName,
    string? LastName,
    string? FirstName,
    string? MiddleName,
    Guid? DulTypeId,
    string? DulSeries,
    string? DulNumber,
    string? PassportIssuedBy,
    DateOnly? PassportIssueDate,
    string? PassportDepartmentCode,
    string? PassportRegistrationAddress,
    string? PersonInn,
    string? Snils,
    string? Citizenship,
    string? CompanyName,
    string? CompanyInn,
    string? CompanyOgrn,
    string? CompanyKpp,
    string? CompanyAddress,
    string? Ogrnip,
    decimal? SharePercent,
    string? ShareFraction,
    decimal? ShareAmount,
    string? Email);
