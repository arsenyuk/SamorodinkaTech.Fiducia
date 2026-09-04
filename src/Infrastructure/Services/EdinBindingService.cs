using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Сервис привязки MPI MasterId к участнику экосистемы и поиска учётной записи.
/// После ЕДИН resolve: привязка MasterId → поиск УЗ в БД → поиск в LDAP.
/// </summary>
public class EdinBindingService : IEdinBindingService
{
    private readonly IEdinApiClient _edinClient;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<EdinBindingService> _logger;

    public EdinBindingService(
        IEdinApiClient edinClient,
        IApplicationDbContext dbContext,
        ILogger<EdinBindingService> logger)
    {
        _edinClient = edinClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<EdinBindingResult> ResolveAndBindAsync(
        Guid ecosystemParticipantId,
        string lastName, string firstName, string? middleName,
        string? inn, string? snils,
        string? dulType, string? dulSeries, string? dulNumber,
        CancellationToken ct = default)
    {
        _logger.LogDebug("ЕДИН resolve для EcosystemParticipant={EcoId}: {LastName} {FirstName}",
            ecosystemParticipantId, lastName, firstName);

        var resolveResult = await _edinClient.ResolvePersonAsync(
            lastName, firstName, middleName,
            inn, snils, dulType, dulSeries, dulNumber, ct);

        if (resolveResult is null)
        {
            return new EdinBindingResult { Error = "Сервис ЕДИН недоступен" };
        }

        if (resolveResult.MasterId is null)
        {
            return new EdinBindingResult
            {
                Error = $"ЕДИН: статус {resolveResult.Status}. " +
                        (resolveResult.HasDefects ? $"Дефекты: {string.Join(", ", resolveResult.Defects)}" : "MasterId не определён")
            };
        }

        var masterId = resolveResult.MasterId.Value;

        // Привязка MasterId к участнику
        var participant = await _dbContext.EcosystemParticipants.FindAsync([ecosystemParticipantId], ct);
        if (participant is null)
        {
            return new EdinBindingResult { Error = $"Участник {ecosystemParticipantId} не найден" };
        }

        if (participant.MpiMasterId == masterId && participant.UserId.HasValue)
        {
            return new EdinBindingResult { Success = true, MpiMasterId = masterId, LinkedUserId = participant.UserId };
        }

        participant.MpiMasterId = masterId;

        // Поиск УЗ: в БД по mpi_master_id (источник: LDAP/AD синхронизация)
        Guid? linkedUserId = null;
        string? userSource = null;

        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.MpiMasterId == masterId, ct);

        if (existingUser is not null)
        {
            linkedUserId = existingUser.Id;
            userSource = "db";
            _logger.LogInformation("ЕДИН: УЗ найдена в БД по MPI MasterId={MasterId}: User={UserId}",
                masterId, existingUser.Id);
        }

        if (linkedUserId.HasValue)
        {
            participant.UserId = linkedUserId.Value;
        }

        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("ЕДИН привязан: EcosystemParticipant={EcoId} → MasterId={MasterId}, User={UserId} ({Source})",
            ecosystemParticipantId, masterId, linkedUserId?.ToString() ?? "-", userSource ?? "none");

        return new EdinBindingResult
        {
            Success = true,
            MpiMasterId = masterId,
            LinkedUserId = linkedUserId,
            UserSource = userSource
        };
    }
}
