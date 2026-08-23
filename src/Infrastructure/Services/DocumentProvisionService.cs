using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Сервис автоматической подгрузки документов при принятии требования REQUEST_INFORMATION.
/// Создаёт ShareRequestItem на каждую группу запрошенных типов и прикрепляет файлы из системы.
/// </summary>
public class DocumentProvisionService : IDocumentProvisionService
{
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly ILogger<DocumentProvisionService> _logger;

    public DocumentProvisionService(
        IDbContextFactory<FiduciaDbContext> dbFactory,
        ILogger<DocumentProvisionService> logger)
    {
        _dbFactory = dbFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task AutoProvisionDocumentsAsync(Guid shareRequestId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var request = await ctx.ShareRequests
            .Include(r => r.RequestType)
            .FirstOrDefaultAsync(r => r.Id == shareRequestId, ct);

        if (request is null)
        {
            _logger.LogWarning("Требование {Id} не найдено", shareRequestId);
            return;
        }

        if (request.RequestType?.Code != "REQUEST_INFORMATION")
        {
            _logger.LogDebug("Требование {Id} не является REQUEST_INFORMATION, пропуск", shareRequestId);
            return;
        }

        if (string.IsNullOrEmpty(request.Payload))
        {
            _logger.LogWarning("Требование {Id} не содержит payload", shareRequestId);
            return;
        }

        // Парсим запрошенные типы документов
        var requestedCodes = ParseDocumentTypeCodes(request.Payload);
        if (requestedCodes.Count == 0)
        {
            _logger.LogWarning("Требование {Id}: нет запрошенных типов документов в payload", shareRequestId);
            return;
        }

        // Загружаем справочник типов документов
        var allDocTypes = await ctx.DocumentTypes
            .Where(d => requestedCodes.Contains(d.Code))
            .ToListAsync(ct);

        // Группируем по groupCode
        var groups = allDocTypes
            .GroupBy(d => d.GroupCode)
            .Select(g => new
            {
                GroupCode = g.Key,
                GroupName = g.First().GroupName,
                TypeCodes = g.Select(d => d.Code).ToList()
            })
            .ToList();

        // Загружаем данные для маппинга файлов
        var charter = await ctx.LegalEntityCharters
            .FirstOrDefaultAsync(c => c.LegalEntityId == request.LegalEntityId, ct);

        var maxSeq = await ctx.ShareRequestItems
            .Where(i => i.ShareRequestId == shareRequestId)
            .MaxAsync(i => (int?)i.SequenceNumber, ct) ?? 0;

        int seq = maxSeq;

        foreach (var group in groups)
        {
            seq++;
            var item = new ShareRequestItem
            {
                Id = Guid.NewGuid(),
                ShareRequestId = shareRequestId,
                SequenceNumber = seq,
                Title = group.GroupName,
                Description = $"Запрошенные типы: {string.Join(", ", group.TypeCodes)}",
                Status = "pending"
            };
            ctx.ShareRequestItems.Add(item);
            await ctx.SaveChangesAsync(ct);

            // Ищем и прикрепляем файлы для каждого типа в группе
            foreach (var typeCode in group.TypeCodes)
            {
                var fileIds = FindFilesForDocumentType(typeCode, charter);
                foreach (var fileId in fileIds)
                {
                    var exists = await ctx.ShareRequestItemFiles
                        .AnyAsync(f => f.ShareRequestItemId == item.Id && f.FileId == fileId, ct);
                    if (exists) continue;

                    ctx.ShareRequestItemFiles.Add(new ShareRequestItemFile
                    {
                        Id = Guid.NewGuid(),
                        ShareRequestItemId = item.Id,
                        FileId = fileId
                    });
                }
            }

            await ctx.SaveChangesAsync(ct);

            // Обновляем статус пункта если есть файлы
            var fileCount = await ctx.ShareRequestItemFiles
                .CountAsync(f => f.ShareRequestItemId == item.Id, ct);
            if (fileCount > 0)
            {
                item.Status = "approved";
                await ctx.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation(
            "Требование {Id}: создано {Count} пунктов с документами",
            shareRequestId, groups.Count);
    }

    /// <summary>
    /// Ищет файлы в системе, соответствующие коду типа документа.
    /// Маппинг: CHARTER → устав, FOUNDING_DOCUMENT → положение о СД.
    /// Остальные типы загружаются ЕИО вручную.
    /// </summary>
    private static List<Guid> FindFilesForDocumentType(string typeCode, LegalEntityCharter? charter)
    {
        var fileIds = new List<Guid>();

        switch (typeCode)
        {
            case "CHARTER" when charter?.CharterDocumentId.HasValue == true:
                fileIds.Add(charter.CharterDocumentId.Value);
                break;

            case "FOUNDING_DOCUMENT" when charter?.BoardRegulationDocumentId.HasValue == true:
                fileIds.Add(charter.BoardRegulationDocumentId.Value);
                break;
        }

        return fileIds;
    }

    /// <summary>
    /// Извлекает коды типов документов из JSON payload.
    /// Ожидаемый формат: { "documentTypeCodes": ["CHARTER", "PROTOCOL_BOD", ...] }
    /// </summary>
    private static List<string> ParseDocumentTypeCodes(string payload)
    {
        var codes = new List<string>();
        try
        {
            var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("documentTypeCodes", out var codesProp))
            {
                foreach (var code in codesProp.EnumerateArray())
                {
                    var codeStr = code.GetString();
                    if (!string.IsNullOrEmpty(codeStr))
                        codes.Add(codeStr);
                }
            }
        }
        catch (Exception)
        {
            // payload не JSON — пропускаем
        }
        return codes;
    }
}
