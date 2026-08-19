using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для реестра участников общества (Board Portal).
/// Доступно только для ООО (ОКОПФ 12300). Попытки доступа логируются в аудит.
/// </summary>
public static class ParticipantEndpoints
{
    private const string LlcOkopfCode = "12300";
    private const string AuditActionAccess = "PARTICIPANT_ACCESS";

    /// <summary>
    /// Регистрирует все endpoint'ы группы Participants.
    /// </summary>
    public static void MapParticipantEndpoints(this WebApplication app)
    {
        var participants = app.MapGroup("/api/participants")
            .RequireAuthorization()
            .WithTags("Participants");

        // GET: список участников текущего ЮЛ
        participants.MapGet("/", async (
            Guid? legalEntityId,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var leId = legalEntityId;
            if (leId is null || leId == Guid.Empty)
            {
                var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
                leId = workplace?.LastSelectedLegalEntityId;
            }
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(Array.Empty<object>());

            var items = await ctx.BoardParticipants
                .Where(p => p.LegalEntityId == leId.Value)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            return Results.Ok(items.Select(MapParticipantToDto));
        });

        // GET: один участник по ID
        participants.MapGet("/{id}", async (Guid id, IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var p = await ctx.BoardParticipants.FindAsync(id);
            if (p is null) return Results.NotFound();
            return Results.Ok(MapParticipantToDto(p));
        });

        // POST: добавление участника
        participants.MapPost("/", async (
            HttpContext http,
            BoardParticipantDto dto,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
                var leId = workplace!.LastSelectedLegalEntityId!.Value;

                var maxSort = await ctx.BoardParticipants
                    .Where(p => p.LegalEntityId == leId)
                    .MaxAsync(p => (int?)p.SortOrder) ?? 0;

                var entity = MapDtoToEntity(dto, leId);
                entity.Id = Guid.NewGuid();
                entity.SortOrder = maxSort + 1;
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                ctx.BoardParticipants.Add(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Добавлен участник: {Name}, доля={Share}%, ЮЛ={LeId}",
                    ClientIpHelper.GetClientIp(http), entity.FullName ?? entity.CompanyName, entity.SharePercent, leId);

                return Results.Ok(MapParticipantToDto(entity));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка добавления участника: {Error}", UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // PUT: обновление участника
        participants.MapPut("/{id}", async (
            Guid id,
            HttpContext http,
            BoardParticipantDto dto,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var entity = await ctx.BoardParticipants.FindAsync(id);
                if (entity is null) return Results.NotFound();

                entity.ParticipantType = dto.ParticipantType ?? entity.ParticipantType;
                entity.FullName = dto.FullName;
                entity.PassportSeries = dto.PassportSeries;
                entity.PassportNumber = dto.PassportNumber;
                entity.PassportIssuedBy = dto.PassportIssuedBy;
                entity.PassportIssueDate = dto.PassportIssueDate;
                entity.PassportDepartmentCode = dto.PassportDepartmentCode;
                entity.PassportRegistrationAddress = dto.PassportRegistrationAddress;
                entity.PersonInn = dto.PersonInn;
                entity.Citizenship = dto.Citizenship;
                entity.CompanyName = dto.CompanyName;
                entity.CompanyInn = dto.CompanyInn;
                entity.CompanyOgrn = dto.CompanyOgrn;
                entity.CompanyKpp = dto.CompanyKpp;
                entity.CompanyAddress = dto.CompanyAddress;
                entity.Ogrnip = dto.Ogrnip;
                entity.SharePercent = dto.SharePercent;
                entity.ShareAmount = dto.ShareAmount;
                entity.PaymentInfo = dto.PaymentInfo;
                entity.ShareRegistrationInfo = dto.ShareRegistrationInfo;
                entity.EntryDate = dto.EntryDate;
                entity.ExitDate = dto.ExitDate;
                entity.IsActive = dto.IsActive ?? true;
                entity.UpdatedAt = DateTime.UtcNow;

                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Обновлён участник: {Name}, id={Id}",
                    ClientIpHelper.GetClientIp(http), entity.FullName ?? entity.CompanyName, id);

                return Results.Ok(MapParticipantToDto(entity));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка обновления участника id={Id}: {Error}", id, UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // DELETE: удаление участника
        participants.MapDelete("/{id}", async (
            Guid id,
            HttpContext http,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var entity = await ctx.BoardParticipants.FindAsync(id);
                if (entity is null) return Results.NotFound();

                ctx.BoardParticipants.Remove(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Удалён участник: {Name}, id={Id}",
                    ClientIpHelper.GetClientIp(http), entity.FullName ?? entity.CompanyName, id);

                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка удаления участника id={Id}: {Error}", id, UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // POST: импорт участников из СПАРК
        participants.MapPost("/import-from-spark", async (
            HttpContext http,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
                var leId = workplace!.LastSelectedLegalEntityId!.Value;

                var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == leId);
                if (le is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не найдено" });

                var sparkFounders = await ctx.ExtSparkFounders
                    .Where(f => f.Inn == le.Inn)
                    .ToListAsync();

                if (sparkFounders.Count == 0)
                    return Results.Ok(new { imported = 0, message = "Нет данных СПАРК для данного ЮЛ" });

                // Удаляем существующих участников этого ЮЛ
                var existing = await ctx.BoardParticipants
                    .Where(p => p.LegalEntityId == leId)
                    .ToListAsync();
                ctx.BoardParticipants.RemoveRange(existing);

                var maxSort = 0;
                var imported = new List<object>();

                foreach (var f in sparkFounders.OrderByDescending(f => f.SharePercent))
                {
                    var entity = new BoardParticipant
                    {
                        Id = Guid.NewGuid(),
                        LegalEntityId = leId,
                        ParticipantType = !string.IsNullOrEmpty(f.Name) ? "FL" : (f.IsEntrepreneur ? "IP" : "FL"),
                        FullName = f.FullName,
                        PersonInn = f.PersonInn,
                        Citizenship = f.Citizenship,
                        CompanyName = f.Name,
                        CompanyInn = f.FounderInn,
                        CompanyOgrn = f.FounderOgrn,
                        Ogrnip = f.Ogrnip,
                        SharePercent = f.SharePercent,
                        ShareAmount = f.ShareAmount,
                        EntryDate = f.EntryDate.HasValue ? DateOnly.FromDateTime(f.EntryDate.Value) : null,
                        ExitDate = f.ExitDate.HasValue ? DateOnly.FromDateTime(f.ExitDate.Value) : null,
                        IsActive = !f.ExitDate.HasValue,
                        SortOrder = ++maxSort,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    ctx.BoardParticipants.Add(entity);
                    imported.Add(MapParticipantToDto(entity));
                }

                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Импорт из СПАРК: {Count} участников, ЮЛ={Inn}",
                    ClientIpHelper.GetClientIp(http), imported.Count, le.Inn);

                return Results.Ok(new { imported = imported.Count, participants = imported });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка импорта из СПАРК: {Error}", UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // ── Treasury Shares API ──────────────────────────────────────────────

        var treasuryShares = app.MapGroup("/api/treasury-shares")
            .RequireAuthorization()
            .WithTags("Treasury Shares");

        // GET: список казначейских долей
        treasuryShares.MapGet("/", async (
            Guid? legalEntityId,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var leId = legalEntityId;
            if (leId is null || leId == Guid.Empty)
            {
                var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
                leId = workplace?.LastSelectedLegalEntityId;
            }
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(Array.Empty<object>());

            var items = await ctx.BoardTreasuryShares
                .Where(t => t.LegalEntityId == leId.Value)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            return Results.Ok(items.Select(MapTreasuryToDto));
        });

        // POST: добавление казначейской доли
        treasuryShares.MapPost("/", async (
            HttpContext http,
            BoardTreasuryShareDto dto,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
                var leId = workplace!.LastSelectedLegalEntityId!.Value;

                var maxSort = await ctx.BoardTreasuryShares
                    .Where(t => t.LegalEntityId == leId)
                    .MaxAsync(t => (int?)t.SortOrder) ?? 0;

                var entity = new BoardTreasuryShare
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = leId,
                    SharePercent = dto.SharePercent,
                    ShareAmount = dto.ShareAmount,
                    AcquiredDate = dto.AcquiredDate,
                    AcquisitionBasis = dto.AcquisitionBasis,
                    SortOrder = maxSort + 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                ctx.BoardTreasuryShares.Add(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Добавлена казначейская доля: {Share}%, id={Id}",
                    ClientIpHelper.GetClientIp(http), entity.SharePercent, entity.Id);

                return Results.Ok(MapTreasuryToDto(entity));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка добавления казначейской доли: {Error}", UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // PUT: обновление казначейской доли
        treasuryShares.MapPut("/{id}", async (
            Guid id,
            HttpContext http,
            BoardTreasuryShareDto dto,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var entity = await ctx.BoardTreasuryShares.FindAsync(id);
                if (entity is null) return Results.NotFound();

                entity.SharePercent = dto.SharePercent;
                entity.ShareAmount = dto.ShareAmount;
                entity.AcquiredDate = dto.AcquiredDate;
                entity.AcquisitionBasis = dto.AcquisitionBasis;
                entity.UpdatedAt = DateTime.UtcNow;

                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Обновлена казначейская доля: {Share}%, id={Id}",
                    ClientIpHelper.GetClientIp(http), entity.SharePercent, id);

                return Results.Ok(MapTreasuryToDto(entity));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка обновления казначейской доли id={Id}: {Error}", id, UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // DELETE: удаление казначейской доли
        treasuryShares.MapDelete("/{id}", async (
            Guid id,
            HttpContext http,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var entity = await ctx.BoardTreasuryShares.FindAsync(id);
                if (entity is null) return Results.NotFound();

                ctx.BoardTreasuryShares.Remove(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Удалена казначейская доля: {Share}%, id={Id}",
                    ClientIpHelper.GetClientIp(http), entity.SharePercent, id);

                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка удаления казначейской доли id={Id}: {Error}", id, UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // ── Registry Uploads API ─────────────────────────────────────────────

        var registryUploads = app.MapGroup("/api/registry-uploads")
            .RequireAuthorization()
            .WithTags("Registry Uploads");

        // GET: список актов загрузки реестра
        registryUploads.MapGet("/", async (
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
            var leId = workplace?.LastSelectedLegalEntityId;
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(Array.Empty<object>());

            var items = await ctx.BoardRegistryUploads
                .Where(u => u.LegalEntityId == leId.Value)
                .OrderByDescending(u => u.UploadedAt)
                .ToListAsync();

            return Results.Ok(items.Select(MapRegistryUploadToDto));
        });

        // POST: загрузка XML-файла реестра
        registryUploads.MapPost("/upload-xml", async (
            HttpContext http,
            IFormFile file,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            IFileStorage fileStorage) =>
        {
            var logger = loggerFactory.CreateLogger("Participants.RegistryUpload");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
                var leId = workplace!.LastSelectedLegalEntityId!.Value;

                // Сохраняем файл через IFileStorage
                await using var stream = file.OpenReadStream();
                var storageKey = await fileStorage.SaveAsync(stream, file.FileName, file.ContentType);

                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = Guid.TryParse(userIdStr, out var uid) ? uid : null;

                var entity = new BoardRegistryUpload
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = leId,
                    XmlOriginalName = file.FileName,
                    Status = "uploaded",
                    UploadedBy = userId,
                    UploadedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Создаём запись в files для ссылки
                var fileEntry = new FileEntry
                {
                    Id = Guid.NewGuid(),
                    OriginalName = file.FileName,
                    ContentType = file.ContentType,
                    SizeBytes = file.Length,
                    StorageProvider = "LOCAL",
                    StorageKeyOrPath = storageKey,
                    IsUploaded = true,
                    Extension = System.IO.Path.GetExtension(file.FileName)?.TrimStart('.')
                };
                ctx.Files.Add(fileEntry);
                entity.XmlFileId = fileEntry.Id;

                ctx.BoardRegistryUploads.Add(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Загружен XML реестра: {FileName}, id={Id}",
                    ClientIpHelper.GetClientIp(http), file.FileName, entity.Id);

                return Results.Ok(MapRegistryUploadToDto(entity));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка загрузки XML реестра: {Error}", UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // POST: загрузка файла подписи к акту
        registryUploads.MapPost("/{id}/upload-signature", async (
            Guid id,
            HttpContext http,
            IFormFile file,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            IFileStorage fileStorage) =>
        {
            var logger = loggerFactory.CreateLogger("Participants.RegistryUpload");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var entity = await ctx.BoardRegistryUploads.FindAsync(id);
                if (entity is null) return Results.NotFound();

                await using var stream = file.OpenReadStream();
                var storageKey = await fileStorage.SaveAsync(stream, file.FileName, file.ContentType);

                var fileEntry = new FileEntry
                {
                    Id = Guid.NewGuid(),
                    OriginalName = file.FileName,
                    ContentType = file.ContentType,
                    SizeBytes = file.Length,
                    StorageProvider = "LOCAL",
                    StorageKeyOrPath = storageKey,
                    IsUploaded = true,
                    Extension = System.IO.Path.GetExtension(file.FileName)?.TrimStart('.')
                };
                ctx.Files.Add(fileEntry);

                entity.SignatureFileId = fileEntry.Id;
                entity.SignatureOriginalName = file.FileName;
                entity.UpdatedAt = DateTime.UtcNow;

                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Загружена подпись к акту {Id}: {FileName}",
                    ClientIpHelper.GetClientIp(http), id, file.FileName);

                return Results.Ok(MapRegistryUploadToDto(entity));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка загрузки подписи к акту id={Id}: {Error}", id, UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // DELETE: удаление акта загрузки
        registryUploads.MapDelete("/{id}", async (
            Guid id,
            HttpContext http,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants.RegistryUpload");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var entity = await ctx.BoardRegistryUploads.FindAsync(id);
                if (entity is null) return Results.NotFound();

                ctx.BoardRegistryUploads.Remove(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Удалён акт загрузки реестра: id={Id}",
                    ClientIpHelper.GetClientIp(http), id);

                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка удаления акта загрузки id={Id}: {Error}", id, UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // ── Participant Changes API ─────────────────────────────────────────

        var participantChanges = app.MapGroup("/api/participant-changes")
            .RequireAuthorization()
            .WithTags("Participant Changes");

        // GET: список информирований об изменении сведений
        participantChanges.MapGet("/", async (
            Guid? participantId,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
            var leId = workplace?.LastSelectedLegalEntityId;
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(Array.Empty<object>());

            var query = ctx.BoardParticipantChanges
                .Where(c => c.LegalEntityId == leId.Value);

            if (participantId.HasValue)
                query = query.Where(c => c.ParticipantId == participantId.Value);

            var items = await query
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return Results.Ok(items.Select(MapParticipantChangeToDto));
        });

        // POST: создание записи об изменении сведений
        participantChanges.MapPost("/", async (
            HttpContext http,
            BoardParticipantChangeDto dto,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants.Change");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateParticipantAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
                var leId = workplace!.LastSelectedLegalEntityId!.Value;

                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = Guid.TryParse(userIdStr, out var uid) ? uid : null;

                var entity = new BoardParticipantChange
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = leId,
                    ParticipantId = dto.ParticipantId,
                    ParticipantType = dto.ParticipantType ?? "FL",
                    FullName = dto.FullName,
                    PassportSeries = dto.PassportSeries,
                    PassportNumber = dto.PassportNumber,
                    PassportIssuedBy = dto.PassportIssuedBy,
                    PassportIssueDate = dto.PassportIssueDate,
                    PassportDepartmentCode = dto.PassportDepartmentCode,
                    PassportRegistrationAddress = dto.PassportRegistrationAddress,
                    PersonInn = dto.PersonInn,
                    Citizenship = dto.Citizenship,
                    CompanyName = dto.CompanyName,
                    CompanyInn = dto.CompanyInn,
                    CompanyOgrn = dto.CompanyOgrn,
                    CompanyKpp = dto.CompanyKpp,
                    CompanyAddress = dto.CompanyAddress,
                    Ogrnip = dto.Ogrnip,
                    SharePercent = dto.SharePercent,
                    ShareAmount = dto.ShareAmount,
                    DocumentFileId = dto.DocumentFileId,
                    DocumentOriginalName = dto.DocumentOriginalName,
                    SubmittedBy = userId,
                    SubmittedAt = DateTime.UtcNow,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                ctx.BoardParticipantChanges.Add(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Создано информирование об изменении сведений: participant={ParticipantId}, id={Id}",
                    ClientIpHelper.GetClientIp(http), dto.ParticipantId, entity.Id);

                return Results.Ok(MapParticipantChangeToDto(entity));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка создания информирования: {Error}", UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // POST: загрузка документа-подтверждения
        participantChanges.MapPost("/upload-document", async (
            HttpContext http,
            IFormFile file,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            IFileStorage fileStorage) =>
        {
            var logger = loggerFactory.CreateLogger("Participants.Change");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateParticipantAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                await using var stream = file.OpenReadStream();
                var storageKey = await fileStorage.SaveAsync(stream, file.FileName, file.ContentType);

                var fileEntry = new FileEntry
                {
                    Id = Guid.NewGuid(),
                    OriginalName = file.FileName,
                    ContentType = file.ContentType,
                    SizeBytes = file.Length,
                    StorageProvider = "LOCAL",
                    StorageKeyOrPath = storageKey,
                    IsUploaded = true,
                    Extension = System.IO.Path.GetExtension(file.FileName)?.TrimStart('.')
                };
                ctx.Files.Add(fileEntry);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Загружен документ к информированию: {FileName}, fileId={FileId}",
                    ClientIpHelper.GetClientIp(http), file.FileName, fileEntry.Id);

                return Results.Ok(new { fileId = fileEntry.Id, originalName = file.FileName });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка загрузки документа: {Error}", UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // PUT: рассмотрение заявки (утвердить/отклонить)
        participantChanges.MapPut("/{id}/review", async (
            Guid id,
            HttpContext http,
            ReviewDto dto,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants.Change");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var entity = await ctx.BoardParticipantChanges.FindAsync(id);
                if (entity is null) return Results.NotFound();

                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = Guid.TryParse(userIdStr, out var uid) ? uid : null;

                entity.Status = dto.Status;
                entity.ReviewComment = dto.Comment;
                entity.ReviewedBy = userId;
                entity.ReviewedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Рассмотрено информирование {Id}: статус={Status}",
                    ClientIpHelper.GetClientIp(http), id, dto.Status);

                return Results.Ok(MapParticipantChangeToDto(entity));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка рассмотрения информирования id={Id}: {Error}", id, UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // DELETE: удаление записи
        participantChanges.MapDelete("/{id}", async (
            Guid id,
            HttpContext http,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Participants.Change");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit);
                if (llcCheck is not null) return llcCheck;

                var entity = await ctx.BoardParticipantChanges.FindAsync(id);
                if (entity is null) return Results.NotFound();

                ctx.BoardParticipantChanges.Remove(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Удалено информирование об изменении сведений: id={Id}",
                    ClientIpHelper.GetClientIp(http), id);

                return Results.Ok();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка удаления информирования id={Id}: {Error}", id, UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });
    }

    /// <summary>
    /// Проверка прав и функционального доступа: ЮЛ выбрано, является ООО (ОКОПФ 12300).
    /// Результат (разрешён/запрещён) логируется в аудит с логином и ФИО пользователя.
    /// </summary>
    private static async Task<IResult?> ValidateAccessAsync(
        FiduciaDbContext ctx,
        HttpContext http,
        ISecurityAuditService audit)
    {
        var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
        var leId = workplace?.LastSelectedLegalEntityId;
        if (leId is null || leId == Guid.Empty)
            return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId.Value);

        if (le is null)
            return Results.BadRequest(new { error = "Юридическое лицо не найдено" });

        var clientIp = ClientIpHelper.GetClientIp(http);
        var (login, fullName) = await GetUserInfoAsync(ctx, http);

        if (le.RefOkopf?.Code != LlcOkopfCode)
        {
            await audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь {login} ({fullName}), ЮЛ «{le.Name}» (ОКОПФ {le.RefOkopf?.Code}) не является ООО",
                entityName: "LegalEntity", entityId: le.Id);
            return Results.Forbid();
        }

        await audit.LogEventAsync(AuditActionAccess, clientIp,
            $"Доступ разрешён: пользователь {login} ({fullName}), ЮЛ «{le.Name}» (ООО), реестр участников",
            entityName: "LegalEntity", entityId: le.Id);

        return null;
    }

    /// <summary>
    /// Проверка прав участника: ЮЛ является ООО + пользователь имеет роль PARTICIPANT.
    /// </summary>
    private static async Task<IResult?> ValidateParticipantAccessAsync(
        FiduciaDbContext ctx,
        HttpContext http,
        ISecurityAuditService audit)
    {
        var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
        var leId = workplace?.LastSelectedLegalEntityId;
        if (leId is null || leId == Guid.Empty)
            return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId.Value);

        if (le is null)
            return Results.BadRequest(new { error = "Юридическое лицо не найдено" });

        var clientIp = ClientIpHelper.GetClientIp(http);
        var (login, fullName) = await GetUserInfoAsync(ctx, http);

        if (le.RefOkopf?.Code != LlcOkopfCode)
        {
            await audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь {login} ({fullName}), ЮЛ «{le.Name}» (ОКОПФ {le.RefOkopf?.Code}) не является ООО",
                entityName: "LegalEntity", entityId: le.Id);
            return Results.Forbid();
        }

        // Проверка роли PARTICIPANT
        var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            await audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь не аутентифицирован, информирование об изменении сведений",
                entityName: "LegalEntity", entityId: le.Id);
            return Results.Forbid();
        }

        var hasParticipantRole = await ctx.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.Role != null && ur.Role.Code == "PARTICIPANT");

        if (!hasParticipantRole)
        {
            await audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь {login} ({fullName}) не имеет роль PARTICIPANT, информирование об изменении сведений",
                entityName: "LegalEntity", entityId: le.Id);
            return Results.Forbid();
        }

        await audit.LogEventAsync(AuditActionAccess, clientIp,
            $"Доступ разрешён: пользователь {login} ({fullName}), роль PARTICIPANT, ЮЛ «{le.Name}» (ООО)",
            entityName: "LegalEntity", entityId: le.Id);

        return null;
    }

    /// <summary>
    /// Извлекает логин (Email) и ФИО текущего пользователя из JWT + БД.
    /// </summary>
    private static async Task<(string login, string fullName)> GetUserInfoAsync(
        FiduciaDbContext ctx,
        HttpContext http)
    {
        var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return ("anonymous", "Неизвестный пользователь");

        var user = await ctx.Users.FindAsync(userId);
        if (user is null)
            return ("unknown", "Пользователь не найден");

        var login = user.Email;
        var fullName = string.IsNullOrWhiteSpace(user.MiddleName)
            ? $"{user.LastName} {user.FirstName}"
            : $"{user.LastName} {user.FirstName} {user.MiddleName}";

        return (login, fullName);
    }

    /// <summary>
    /// Извлекает самое глубокое сообщение из цепочки InnerException.
    /// EF Core оборачивает реальную ошибку БД в «An error occurred while saving…».
    /// </summary>
    private static string UnwrapException(Exception ex)
    {
        var inner = ex;
        while (inner.InnerException != null)
            inner = inner.InnerException;
        return inner.Message;
    }

    private static object MapParticipantToDto(BoardParticipant p) => new
    {
        p.Id,
        p.LegalEntityId,
        p.ParticipantType,
        p.FullName,
        p.PassportSeries,
        p.PassportNumber,
        p.PassportIssuedBy,
        PassportIssueDate = p.PassportIssueDate?.ToString("dd.MM.yyyy"),
        p.PassportDepartmentCode,
        p.PassportRegistrationAddress,
        p.PersonInn,
        p.Citizenship,
        p.CompanyName,
        p.CompanyInn,
        p.CompanyOgrn,
        p.CompanyKpp,
        p.CompanyAddress,
        p.Ogrnip,
        p.SharePercent,
        p.ShareAmount,
        p.PaymentInfo,
        p.ShareRegistrationInfo,
        EntryDate = p.EntryDate?.ToString("dd.MM.yyyy"),
        ExitDate = p.ExitDate?.ToString("dd.MM.yyyy"),
        p.IsActive,
        p.SortOrder
    };

    private static BoardParticipant MapDtoToEntity(BoardParticipantDto dto, Guid legalEntityId) => new()
    {
        LegalEntityId = legalEntityId,
        ParticipantType = dto.ParticipantType ?? "FL",
        FullName = dto.FullName,
        PassportSeries = dto.PassportSeries,
        PassportNumber = dto.PassportNumber,
        PassportIssuedBy = dto.PassportIssuedBy,
        PassportIssueDate = dto.PassportIssueDate,
        PassportDepartmentCode = dto.PassportDepartmentCode,
        PassportRegistrationAddress = dto.PassportRegistrationAddress,
        PersonInn = dto.PersonInn,
        Citizenship = dto.Citizenship,
        CompanyName = dto.CompanyName,
        CompanyInn = dto.CompanyInn,
        CompanyOgrn = dto.CompanyOgrn,
        CompanyKpp = dto.CompanyKpp,
        CompanyAddress = dto.CompanyAddress,
        Ogrnip = dto.Ogrnip,
        SharePercent = dto.SharePercent,
        ShareAmount = dto.ShareAmount,
        PaymentInfo = dto.PaymentInfo,
        ShareRegistrationInfo = dto.ShareRegistrationInfo,
        EntryDate = dto.EntryDate,
        ExitDate = dto.ExitDate,
        IsActive = dto.IsActive ?? true
    };

    private static object MapTreasuryToDto(BoardTreasuryShare t) => new
    {
        t.Id,
        t.LegalEntityId,
        t.SharePercent,
        t.ShareAmount,
        AcquiredDate = t.AcquiredDate?.ToString("dd.MM.yyyy"),
        t.AcquisitionBasis,
        t.SortOrder
    };

    private static object MapRegistryUploadToDto(BoardRegistryUpload u) => new
    {
        u.Id,
        u.LegalEntityId,
        u.XmlFileId,
        u.SignatureFileId,
        u.XmlOriginalName,
        u.SignatureOriginalName,
        u.Status,
        u.ParticipantCount,
        UploadedAt = u.UploadedAt.ToString("dd.MM.yyyy HH:mm"),
        u.UploadedBy
    };

    private static object MapParticipantChangeToDto(BoardParticipantChange c) => new
    {
        c.Id,
        c.LegalEntityId,
        c.ParticipantId,
        c.ParticipantType,
        c.FullName,
        c.PassportSeries,
        c.PassportNumber,
        c.PassportIssuedBy,
        PassportIssueDate = c.PassportIssueDate?.ToString("dd.MM.yyyy"),
        c.PassportDepartmentCode,
        c.PassportRegistrationAddress,
        c.PersonInn,
        c.Citizenship,
        c.CompanyName,
        c.CompanyInn,
        c.CompanyOgrn,
        c.CompanyKpp,
        c.CompanyAddress,
        c.Ogrnip,
        c.SharePercent,
        c.ShareAmount,
        c.DocumentFileId,
        c.DocumentOriginalName,
        SubmittedAt = c.SubmittedAt.ToString("dd.MM.yyyy HH:mm"),
        c.SubmittedBy,
        c.Status,
        c.ReviewComment,
        c.ReviewedBy,
        ReviewedAt = c.ReviewedAt?.ToString("dd.MM.yyyy HH:mm")
    };

    /// <summary>DTO для участника общества.</summary>
    public record BoardParticipantDto
    {
        public string? ParticipantType { get; init; }
        public string? FullName { get; init; }
        public string? PassportSeries { get; init; }
        public string? PassportNumber { get; init; }
        public string? PassportIssuedBy { get; init; }
        public DateOnly? PassportIssueDate { get; init; }
        public string? PassportDepartmentCode { get; init; }
        public string? PassportRegistrationAddress { get; init; }
        public string? PersonInn { get; init; }
        public string? Citizenship { get; init; }
        public string? CompanyName { get; init; }
        public string? CompanyInn { get; init; }
        public string? CompanyOgrn { get; init; }
        public string? CompanyKpp { get; init; }
        public string? CompanyAddress { get; init; }
        public string? Ogrnip { get; init; }
        public decimal? SharePercent { get; init; }
        public decimal? ShareAmount { get; init; }
        public string? PaymentInfo { get; init; }
        public string? ShareRegistrationInfo { get; init; }
        public DateOnly? EntryDate { get; init; }
        public DateOnly? ExitDate { get; init; }
        public bool? IsActive { get; init; }
    }

    /// <summary>DTO для казначейской доли.</summary>
    public record BoardTreasuryShareDto
    {
        public decimal? SharePercent { get; init; }
        public decimal? ShareAmount { get; init; }
        public DateOnly? AcquiredDate { get; init; }
        public string? AcquisitionBasis { get; init; }
    }

    /// <summary>DTO для информирования об изменении сведений участника.</summary>
    public record BoardParticipantChangeDto
    {
        public Guid ParticipantId { get; init; }
        public string? ParticipantType { get; init; }
        public string? FullName { get; init; }
        public string? PassportSeries { get; init; }
        public string? PassportNumber { get; init; }
        public string? PassportIssuedBy { get; init; }
        public DateOnly? PassportIssueDate { get; init; }
        public string? PassportDepartmentCode { get; init; }
        public string? PassportRegistrationAddress { get; init; }
        public string? PersonInn { get; init; }
        public string? Citizenship { get; init; }
        public string? CompanyName { get; init; }
        public string? CompanyInn { get; init; }
        public string? CompanyOgrn { get; init; }
        public string? CompanyKpp { get; init; }
        public string? CompanyAddress { get; init; }
        public string? Ogrnip { get; init; }
        public decimal? SharePercent { get; init; }
        public decimal? ShareAmount { get; init; }
        public Guid? DocumentFileId { get; init; }
        public string? DocumentOriginalName { get; init; }
    }

    /// <summary>DTO для рассмотрения заявки.</summary>
    public record ReviewDto
    {
        public string Status { get; init; } = "pending";
        public string? Comment { get; init; }
    }
}
