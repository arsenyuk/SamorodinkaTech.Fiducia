using SamorodinkaTech.Fiducia.Domain.Entities;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис чтения данных участников общества.
/// Заменяет loopback HTTP GET-вызовы из Blazor-страниц к /api/participants.
/// </summary>
public interface IParticipantReadService
{
    /// <summary>Список участников ЮЛ (BoardParticipant + Company + Share).</summary>
    /// <param name="legalEntityId">Идентификатор юридического лица.</param>
    /// <returns>Список участников, отсортированных по размеру доли (убывание).</returns>
    Task<List<BoardParticipant>> GetByLegalEntityAsync(Guid legalEntityId);

    /// <summary>Один участник по ID (с включениями: EcosystemParticipant, Person, Company, Share).</summary>
    /// <param name="id">Идентификатор участника.</param>
    /// <returns>Участник с полными данными или null.</returns>
    Task<BoardParticipant?> GetDetailAsync(Guid id);

    /// <summary>Текущий участник по userId (активный BoardParticipant + его SharePercent).</summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="legalEntityId">Идентификатор юридического лица.</param>
    /// <returns>Кортеж (участник, доля) или (null, null).</returns>
    Task<(BoardParticipant? participant, decimal? sharePercent)> GetCurrentAsync(Guid userId, Guid legalEntityId);

    /// <summary>Казначейские доли ЮЛ.</summary>
    /// <param name="legalEntityId">Идентификатор юридического лица.</param>
    /// <returns>Список казначейских долей, отсортированных по SortOrder.</returns>
    Task<List<BoardTreasuryShare>> GetTreasuryAsync(Guid legalEntityId);

    /// <summary>Загрузки реестра ЮЛ.</summary>
    /// <param name="legalEntityId">Идентификатор юридического лица.</param>
    /// <returns>Список загрузок реестра, отсортированных по дате загрузки (убывание).</returns>
    Task<List<BoardRegistryUpload>> GetRegistryUploadsAsync(Guid legalEntityId);

    /// <summary>Записи информирования (BoardParticipantChange) по ЮЛ, опционально по участнику.</summary>
    /// <param name="legalEntityId">Идентификатор юридического лица.</param>
    /// <param name="participantId">Идентификатор участника (опционально).</param>
    /// <returns>Список записей информирования, отсортированных по дате подачи (убывание).</returns>
    Task<List<BoardParticipantChange>> GetChangesAsync(Guid legalEntityId, Guid? participantId = null);
}
