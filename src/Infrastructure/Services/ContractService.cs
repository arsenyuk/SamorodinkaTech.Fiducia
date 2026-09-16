using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Validation;
using SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация IContractService: создание и обновление договоров.
/// Логика извлечена из ContractEndpoints (POST/PUT).
/// </summary>
public class ContractService : IContractService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly IChunkedUploadService _uploadService;
    private readonly ILogger<ContractService> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ContractService"/>.
    /// </summary>
    public ContractService(
        IDbContextFactory<FiduciaDbContext> dbFactory,
        IChunkedUploadService uploadService,
        ILogger<ContractService> logger)
    {
        _dbFactory = dbFactory;
        _uploadService = uploadService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(ContractCreateModel model, Stream? fileStream, string? fileName, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        if (string.IsNullOrWhiteSpace(model.CounterpartyInn) ||
            string.IsNullOrWhiteSpace(model.CounterpartyName))
        {
            throw new InvalidOperationException("contractType, counterpartyInn, counterpartyName are required");
        }

        if (model.LegalEntityId == Guid.Empty)
            throw new InvalidOperationException("Юридическое лицо не выбрано");

        // Загрузка файла договора
        Guid? documentId = await UploadFileAsync(ctx, fileStream, fileName, ct);

        // Загрузка файла правил реестра (REGISTRAR)
        Guid? rulesDocId = null;
        if (model.ContractType == ContractType.REGISTRAR && model.RulesFileStream is not null)
        {
            rulesDocId = await UploadFileAsync(ctx, model.RulesFileStream, model.RulesFileName, ct);
        }

        // ОГРНИП для MANAGEMENT_IP
        string? managerOgrnip = null;
        if (model.ContractType == ContractType.MANAGEMENT_IP && !string.IsNullOrWhiteSpace(model.ManagerOgrnip))
        {
            var (ogrnipValid, ogrnipError) = OgrnipValidator.Validate(model.ManagerOgrnip);
            if (!ogrnipValid)
                throw new InvalidOperationException(ogrnipError);
            managerOgrnip = model.ManagerOgrnip.Trim();
        }

        // Единицы измерения сроков (REGISTRAR)
        Guid? registryUnitId = await ResolveMeasurementUnitAsync(ctx, model.RegistryPreparationUnit, ct);
        Guid? dividendRegistryUnitId = await ResolveMeasurementUnitAsync(ctx, model.DividendRegistryPreparationUnit, ct);

        var entity = new Contract
        {
            Id = Guid.NewGuid(),
            LegalEntityId = model.LegalEntityId,
            ContractType = model.ContractType,
            CounterpartyName = model.CounterpartyName.Trim(),
            CounterpartyInn = model.CounterpartyInn.Trim(),
            ContractNumber = model.ContractNumber?.Trim(),
            ContractDate = model.ContractDate,
            ContractValidFrom = model.ContractValidFrom,
            ContractValidTo = model.IsIndefinite ? null : model.ContractValidTo,
            IsIndefinite = model.IsIndefinite,
            ContractDocumentId = documentId,
            ManagerOgrnip = managerOgrnip,
            ManagerLegalEntityId = model.ContractType == ContractType.MANAGEMENT_UL ? model.ManagerLegalEntityId : null,
            RegistryPreparationDays = model.RegistryPreparationDays,
            RegistryPreparationUnitId = registryUnitId,
            DividendRegistryPreparationDays = model.DividendRegistryPreparationDays,
            DividendRegistryPreparationUnitId = dividendRegistryUnitId,
            RegistryRulesUrl = model.RegistryRulesUrl,
            RegistryRulesDocumentId = rulesDocId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = model.UserId
        };

        ctx.Contracts.Add(entity);
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Создан договор {Id}, тип {Type}, ЮЛ={LeId}", entity.Id, entity.ContractType, model.LegalEntityId);

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Guid id, ContractUpdateModel model, Stream? fileStream, string? fileName, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var entity = await ctx.Contracts.FindAsync(new object[] { id }, ct);
        if (entity is null)
            throw new InvalidOperationException($"Договор {id} не найден");

        if (!string.IsNullOrWhiteSpace(model.ContractNumber))
            entity.ContractNumber = model.ContractNumber.Trim();

        if (model.ContractDate.HasValue)
            entity.ContractDate = model.ContractDate;

        if (model.ContractValidFrom.HasValue)
            entity.ContractValidFrom = model.ContractValidFrom;

        entity.IsIndefinite = model.IsIndefinite;
        if (!entity.IsIndefinite && model.ContractValidTo.HasValue)
            entity.ContractValidTo = model.ContractValidTo;
        else if (entity.IsIndefinite)
            entity.ContractValidTo = null;

        // REGISTRAR: специфичные поля
        if (entity.ContractType == ContractType.REGISTRAR)
        {
            entity.RegistryPreparationDays = model.RegistryPreparationDays;
            entity.RegistryPreparationUnitId = await ResolveMeasurementUnitAsync(ctx, model.RegistryPreparationUnit, ct);
            entity.DividendRegistryPreparationDays = model.DividendRegistryPreparationDays;
            entity.DividendRegistryPreparationUnitId = await ResolveMeasurementUnitAsync(ctx, model.DividendRegistryPreparationUnit, ct);
            entity.RegistryRulesUrl = model.RegistryRulesUrl;
        }

        // MANAGEMENT_IP: ОГРНИП
        if (entity.ContractType == ContractType.MANAGEMENT_IP && !string.IsNullOrWhiteSpace(model.ManagerOgrnip))
        {
            var (ogrnipValid, ogrnipError) = OgrnipValidator.Validate(model.ManagerOgrnip);
            if (!ogrnipValid)
                throw new InvalidOperationException(ogrnipError);
            entity.ManagerOgrnip = model.ManagerOgrnip.Trim();
        }

        // MANAGEMENT_UL: ссылка на ЮЛ
        if (entity.ContractType == ContractType.MANAGEMENT_UL && model.ManagerLegalEntityId.HasValue)
        {
            entity.ManagerLegalEntityId = model.ManagerLegalEntityId;
        }

        // Загрузка файла договора
        if (fileStream is not null)
        {
            var docId = await UploadFileAsync(ctx, fileStream, fileName, ct);
            if (docId.HasValue)
                entity.ContractDocumentId = docId;
        }

        // Загрузка файла правил реестра (REGISTRAR)
        if (entity.ContractType == ContractType.REGISTRAR && model.RulesFileStream is not null)
        {
            var rulesId = await UploadFileAsync(ctx, model.RulesFileStream, model.RulesFileName, ct);
            if (rulesId.HasValue)
                entity.RegistryRulesDocumentId = rulesId;
        }

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("Обновлён договор {Id}", id);
    }

    /// <summary>
    /// Загружает файл через IChunkedUploadService и возвращает ID записи FileEntry.
    /// </summary>
    private async Task<Guid?> UploadFileAsync(FiduciaDbContext ctx, Stream? fileStream, string? fileName, CancellationToken ct)
    {
        if (fileStream is null || string.IsNullOrWhiteSpace(fileName))
            return null;

        var uploadId = await _uploadService.InitiateUploadAsync(fileName, null, fileStream.Length, ct);
        await _uploadService.UploadChunkAsync(uploadId, 0, fileStream, ct);
        var fileEntry = await _uploadService.CompleteUploadAsync(uploadId, ct);
        return fileEntry.Id;
    }

    /// <summary>
    /// Разрешает код единицы измерения в FK на ref_measurement_unit.
    /// </summary>
    private static async Task<Guid?> ResolveMeasurementUnitAsync(FiduciaDbContext ctx, string? unitCode, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(unitCode))
            return null;

        return (await ctx.RefMeasurementUnits.FirstOrDefaultAsync(x => x.Code == unitCode, ct))?.Id;
    }
}
