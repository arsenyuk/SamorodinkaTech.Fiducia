namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис привязки MPI MasterId к участнику экосистемы и поиска учётной записи.
/// </summary>
public interface IEdinBindingService
{
    /// <summary>Resolve через ЕДИН + привязка к участнику + поиск УЗ.</summary>
    Task<EdinBindingResult> ResolveAndBindAsync(
        Guid ecosystemParticipantId,
        string lastName, string firstName, string? middleName,
        string? inn, string? snils,
        string? dulType, string? dulSeries, string? dulNumber,
        CancellationToken ct = default);
}

/// <summary>Результат операции привязки MPI.</summary>
public record EdinBindingResult
{
    /// <summary>Успешность операции.</summary>
    public bool Success { get; init; }

    /// <summary>Привязанный MasterId (null при ошибке).</summary>
    public Guid? MpiMasterId { get; init; }

    /// <summary>Id найденной учётной записи (null если УЗ не найдена).</summary>
    public Guid? LinkedUserId { get; init; }

    /// <summary>Источник УЗ: "db" (таблица users) / "ldap" / null.</summary>
    public string? UserSource { get; init; }

    /// <summary>Сообщение об ошибке (null при успехе).</summary>
    public string? Error { get; init; }
}
