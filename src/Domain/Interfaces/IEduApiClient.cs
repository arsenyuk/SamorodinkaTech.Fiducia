using SamorodinkaTech.Fiducia.Domain.Models.Edin;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Клиент ЕДИН (Mnemonios MPI) — идентификация физических лиц.
/// </summary>
public interface IEdinApiClient
{
    /// <summary>Идентификация по ФИО + ИНН/СНИЛС/ДУЛ. Возвращает MasterId или null.</summary>
    Task<EdinPersonResult?> ResolvePersonAsync(
        string lastName, string firstName, string? middleName,
        string? inn, string? snils,
        string? dulType, string? dulSeries, string? dulNumber,
        CancellationToken cancellationToken = default);

    /// <summary>Получение данных персоны по MasterId.</summary>
    Task<EdinPersonResult?> GetPersonAsync(
        Guid masterId,
        CancellationToken cancellationToken = default);
}
