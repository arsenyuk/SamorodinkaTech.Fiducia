using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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
            HttpContext http,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var leId = legalEntityId;
            if (leId is null || leId == Guid.Empty)
            {
                var (login, _) = await GetUserInfoAsync(ctx, http);
                if (!string.IsNullOrEmpty(login))
                {
                    var participant = await ctx.EcosystemParticipants
                        .FirstOrDefaultAsync(ep => ep.Login == login);
                    if (participant != null)
                        leId = participant.LegalEntityId;
                }
            }
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(Array.Empty<object>());

            var items = await ctx.BoardParticipants
                .Include(p => p.EcosystemParticipant)
                .Where(p => p.LegalEntityId == leId.Value)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            return Results.Ok(items.Select(p => MapParticipantToDto(p)));
        });

        // GET: поиск EcosystemParticipant по ФИО (для привязки BoardParticipant)
        participants.MapGet("/eco-search", async (
            string name,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            HttpContext http) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
            if (leId is null) return Results.Ok(Array.Empty<object>());

            var search = name.Trim().ToLower();
            var results = await ctx.EcosystemParticipants
                .Where(ep => ep.LegalEntityId == leId.Value && ep.IsActive
                    && (ep.LastName + " " + ep.FirstName + " " + (ep.MiddleName ?? "")).ToLower().Contains(search))
                .Select(ep => new { ep.Id, ep.Login, FullName = ep.LastName + " " + ep.FirstName + " " + (ep.MiddleName ?? "") })
                .ToListAsync();

            return Results.Ok(results);
        });

        // GET: текущий участник (по пользователю)
        participants.MapGet("/current", async (
            IDbContextFactory<FiduciaDbContext> dbFactory,
            HttpContext http) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();

            var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Ok(new { Id = (Guid?)null, FullName = (string?)null, SharePercent = (decimal?)null });

            var user = await ctx.Users.FindAsync(userId);
            if (user is null)
                return Results.Ok(new { Id = (Guid?)null, FullName = (string?)null, SharePercent = (decimal?)null });

            var ecoParticipant = await ctx.EcosystemParticipants.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (ecoParticipant is null)
                return Results.Ok(new { Id = (Guid?)null, FullName = (string?)null, SharePercent = (decimal?)null });

            var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
            if (leId is null)
                return Results.Ok(new { Id = (Guid?)null, FullName = (string?)null, SharePercent = (decimal?)null });

            var participant = await ctx.BoardParticipants
                .FirstOrDefaultAsync(p => p.LegalEntityId == leId.Value && p.EcosystemParticipantId == ecoParticipant.Id && p.IsActive);

            if (participant is null)
                return Results.Ok(new { Id = (Guid?)null, FullName = (string?)null, SharePercent = (decimal?)null });

            return Results.Ok(new { participant.Id, FullName = participant.Person?.FullName, participant.SharePercent });
        });

        // GET: один участник по ID
        participants.MapGet("/{id}", async (Guid id, IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var p = await ctx.BoardParticipants.Include(x => x.EcosystemParticipant).Include(x => x.Person).FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return Results.NotFound();
            return Results.Ok(MapParticipantToDto(p));
        });

        // POST: добавление участника
        participants.MapPost("/", async (
            HttpContext http,
            BoardParticipantDto dto,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            IServiceProvider serviceProvider) =>
        {
            var logger = loggerFactory.CreateLogger("Participants");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
                if (llcCheck is not null) return llcCheck;

                var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http) ?? Guid.Empty;

                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = Guid.TryParse(userIdStr, out var uid) ? uid : null;

                var maxSort = await ctx.BoardParticipants
                    .Where(p => p.LegalEntityId == leId)
                    .MaxAsync(p => (int?)p.SortOrder) ?? 0;

                var entity = MapDtoToEntity(dto, leId);

                // ── Создание Person при наличии FullName для ФЛ ──────
                if (entity.ParticipantType == "FL" && !string.IsNullOrWhiteSpace(dto.FullName))
                {
                    var (lastName, firstName, middleName) = SplitFullName(dto.FullName);
                    var person = new Person
                    {
                        Id = Guid.NewGuid(),
                        LastName = lastName,
                        FirstName = firstName,
                        MiddleName = middleName,
                        Inn = dto.PersonInn,
                        Citizenship = dto.Citizenship,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };
                    ctx.Persons.Add(person);
                    entity.PersonId = person.Id;

                    // Привязываемся к существующему EcosystemParticipant или создаём новый
                    if (!entity.EcosystemParticipantId.HasValue)
                    {
                        var ecoParticipant = new EcosystemParticipant
                        {
                            Id = Guid.NewGuid(),
                            LegalEntityId = leId,
                            LastName = lastName,
                            FirstName = firstName,
                            MiddleName = middleName,
                            Login = string.Empty,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId
                        };
                        ctx.EcosystemParticipants.Add(ecoParticipant);
                        entity.EcosystemParticipantId = ecoParticipant.Id;
                    }
                }

                // ── Дедупликация ──────────────────────────────────────
                var dedupError = await CheckDuplicateParticipantAsync(ctx, leId, entity, null);
                if (dedupError is not null)
                {
                    logger.LogWarning("Дедупликация: {Error}", dedupError);
                    return Results.BadRequest(new { error = dedupError });
                }
                entity.Id = Guid.NewGuid();
                entity.SortOrder = maxSort + 1;
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                ctx.BoardParticipants.Add(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Добавлен участник: {Name}, доля={Share}%, ЮЛ={LeId}",
                    ClientIpHelper.GetClientIp(http), entity.Person?.FullName ?? entity.CompanyName, entity.SharePercent, leId);

                // ЕДИН: автоматическая привязка MasterId при наличии ПДн
                _ = TriggerEdinBindingAsync(serviceProvider, logger, ctx, entity);

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
            IDbContextFactory<FiduciaDbContext> dbFactory,
            IServiceProvider serviceProvider) =>
        {
            var logger = loggerFactory.CreateLogger("Participants");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
                if (llcCheck is not null) return llcCheck;

                var entity = await ctx.BoardParticipants.FindAsync(id);
                if (entity is null) return Results.NotFound();

                entity.ParticipantType = dto.ParticipantType ?? entity.ParticipantType;
                entity.CompanyName = dto.CompanyName;
                entity.CompanyInn = dto.CompanyInn;
                entity.CompanyOgrn = dto.CompanyOgrn;
                entity.CompanyKpp = dto.CompanyKpp;
                entity.CompanyAddress = dto.CompanyAddress;
                entity.SharePercent = dto.SharePercent;
                entity.ShareAmount = dto.ShareAmount;
                entity.PaymentInfo = dto.PaymentInfo;
                entity.ShareRegistrationInfo = dto.ShareRegistrationInfo;
                entity.EntryDate = dto.EntryDate;
                entity.ExitDate = dto.ExitDate;
                entity.IsActive = dto.IsActive ?? true;
                entity.UpdatedAt = DateTime.UtcNow;

                // ── Дедупликация (при обновлении) ────────────────────
                var dedupError = await CheckDuplicateParticipantAsync(ctx, entity.LegalEntityId, entity, id);
                if (dedupError is not null)
                {
                    logger.LogWarning("Дедупликация при обновлении {Id}: {Error}", id, dedupError);
                    return Results.BadRequest(new { error = dedupError });
                }

                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Обновлён участник: {Name}, id={Id}",
                    ClientIpHelper.GetClientIp(http), entity.Person?.FullName ?? entity.CompanyName, id);

                // ЕДИН: автоматическая привязка MasterId при наличии ПДн
                _ = TriggerEdinBindingAsync(serviceProvider, logger, ctx, entity);

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
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
                if (llcCheck is not null) return llcCheck;

                var entity = await ctx.BoardParticipants.FindAsync(id);
                if (entity is null) return Results.NotFound();

                // Если участник был ГД — удаляем роль CEO и деактивируем участника экосистемы
                if (entity.IsGeneralDirector && entity.EcosystemParticipantId.HasValue)
                {
                    var ep = await ctx.EcosystemParticipants
                        .FirstOrDefaultAsync(x => x.Id == entity.EcosystemParticipantId.Value);

                    if (ep?.UserId.HasValue == true)
                    {
                        var ceoRole = await ctx.Roles.FirstOrDefaultAsync(r => r.Code == "CEO");
                        if (ceoRole is not null)
                        {
                            var ceoUserRole = await ctx.UserRoles
                                .FirstOrDefaultAsync(ur => ur.UserId == ep.UserId.Value
                                                       && ur.RoleId == ceoRole.Id);
                            if (ceoUserRole is not null)
                                ctx.UserRoles.Remove(ceoUserRole);
                        }

                        // Проверяем, остались ли роли у пользователя
                        var remainingRoles = await ctx.UserRoles
                            .CountAsync(ur => ur.UserId == ep.UserId.Value);

                        // Если CEO была единственной ролью — деактивируем участника экосистемы
                        if (remainingRoles <= 1) // <= 1 потому что CEO ещё не удалена на момент подсчёта
                        {
                            ep.IsActive = false;
                            logger.LogInformation("Участник экосистемы {EPId} деактивирован — нет оставшихся ролей", ep.Id);
                        }
                    }
                }

                ctx.BoardParticipants.Remove(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Удалён участник: {Name}, id={Id}",
                    ClientIpHelper.GetClientIp(http), entity.Person?.FullName ?? entity.CompanyName, id);

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
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
                if (llcCheck is not null) return llcCheck;

                var (login, _) = await GetUserInfoAsync(ctx, http);
                var participant = await ctx.EcosystemParticipants
                    .FirstOrDefaultAsync(ep => ep.Login == login);
                var leId = participant?.LegalEntityId ?? Guid.Empty;

                var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == leId);
                if (le is null)
                {
                    logger.LogWarning("Импорт из СПАРК: юридическое лицо не найдено для Id={LeId}", leId);
                    return Results.BadRequest(new { error = "Юридическое лицо не найдено" });
                }

                var sparkFounders = await ctx.ExtSparkFounders
                    .Where(f => f.Inn == le.Inn)
                    .ToListAsync();

                if (sparkFounders.Count == 0)
                    return Results.Ok(new { imported = 0, message = "Нет данных СПАРК для данного ЮЛ" });

                // Удаляем существующих участников этого ЮЛ
                var existing = await ctx.BoardParticipants
                    .Where(p => p.LegalEntityId == leId)
                    .ToListAsync();

                // Очищаем роли CEO у участников-ГД перед удалением
                var gdParticipants = existing.Where(p => p.IsGeneralDirector && p.EcosystemParticipantId.HasValue).ToList();
                if (gdParticipants.Count > 0)
                {
                    var ceoRole = await ctx.Roles.FirstOrDefaultAsync(r => r.Code == "CEO");
                    if (ceoRole is not null)
                    {
                        foreach (var gd in gdParticipants)
                        {
                            var ep = await ctx.EcosystemParticipants
                                .FirstOrDefaultAsync(x => x.Id == gd.EcosystemParticipantId!.Value);
                            if (ep?.UserId.HasValue == true)
                            {
                                var ceoUserRole = await ctx.UserRoles
                                    .FirstOrDefaultAsync(ur => ur.UserId == ep.UserId.Value
                                                           && ur.RoleId == ceoRole.Id);
                                if (ceoUserRole is not null)
                                    ctx.UserRoles.Remove(ceoUserRole);

                                var remainingRoles = await ctx.UserRoles
                                    .CountAsync(ur => ur.UserId == ep.UserId.Value);
                                if (remainingRoles <= 1)
                                    ep.IsActive = false;
                            }
                        }
                    }
                }

                ctx.BoardParticipants.RemoveRange(existing);

                var maxSort = 0;
                var imported = new List<object>();

                foreach (var f in sparkFounders.OrderByDescending(f => f.SharePercent))
                {
                    var isFl = !string.IsNullOrEmpty(f.Name) && !f.IsEntrepreneur;
                    var isIp = f.IsEntrepreneur;

                    Guid? personId = null;
                    if (isFl || isIp)
                    {
                        var nameParts = (f.FullName ?? f.Name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var person = new Person
                        {
                            Id = Guid.NewGuid(),
                            LastName = nameParts.ElementAtOrDefault(0) ?? "",
                            FirstName = nameParts.ElementAtOrDefault(1) ?? "",
                            MiddleName = nameParts.ElementAtOrDefault(2),
                            Inn = f.PersonInn ?? (isIp ? f.Ogrnip : null),
                            Citizenship = f.Citizenship,
                            Ogrnip = isIp ? f.Ogrnip : null,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        ctx.Persons.Add(person);
                        personId = person.Id;
                    }

                    var entity = new BoardParticipant
                    {
                        Id = Guid.NewGuid(),
                        LegalEntityId = leId,
                        ParticipantType = isFl ? "FL" : (isIp ? "IP" : "UL"),
                        PersonId = personId,
                        CompanyName = f.Name,
                        CompanyInn = f.FounderInn,
                        CompanyOgrn = f.FounderOgrn,
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
            HttpContext http,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var leId = legalEntityId;
            if (leId is null || leId == Guid.Empty)
            {
                leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
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
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
                if (llcCheck is not null) return llcCheck;

                var (login, _) = await GetUserInfoAsync(ctx, http);
                var participant = await ctx.EcosystemParticipants
                    .FirstOrDefaultAsync(ep => ep.Login == login);
                var leId = participant?.LegalEntityId ?? Guid.Empty;

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
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
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
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
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
            HttpContext http,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(Array.Empty<object>());

            var items = await ctx.BoardRegistryUploads
                .Where(u => u.LegalEntityId == leId.Value)
                .OrderByDescending(u => u.UploadedAt)
                .ToListAsync();

            return Results.Ok(items.Select(MapRegistryUploadToDto));
        });

        // POST: загрузка XML-файла реестра + отсоединённая подпись (оба обязательны)
        registryUploads.MapPost("/upload-xml", async (
            HttpContext http,
            IFormFile file,
            IFormFile signature,
            ISecurityAuditService audit,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            IFileStorage fileStorage) =>
        {
            var logger = loggerFactory.CreateLogger("Participants.RegistryUpload");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
                if (llcCheck is not null) return llcCheck;

                // Валидация XML
                var xmlExt = System.IO.Path.GetExtension(file.FileName)?.ToLowerInvariant();
                if (xmlExt != ".xml")
                    return Results.BadRequest(new { error = "XML-файл должен иметь расширение .xml" });

                // Валидация подписи
                var sigExt = System.IO.Path.GetExtension(signature.FileName)?.ToLowerInvariant();
                if (sigExt != ".sig" && sigExt != ".p7s")
                    return Results.BadRequest(new { error = "Файл подписи должен иметь расширение .sig или .p7s" });

                var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не определено" });

                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = Guid.TryParse(userIdStr, out var uid) ? uid : null;

                // Сохраняем XML
                await using var xmlStream = file.OpenReadStream();
                var xmlStorageKey = await fileStorage.SaveAsync(xmlStream, file.FileName, file.ContentType);

                var xmlFileEntry = new FileEntry
                {
                    Id = Guid.NewGuid(),
                    OriginalName = file.FileName,
                    ContentType = file.ContentType,
                    SizeBytes = file.Length,
                    StorageProvider = "LOCAL",
                    StorageKeyOrPath = xmlStorageKey,
                    IsUploaded = true,
                    Extension = xmlExt?.TrimStart('.')
                };
                ctx.Files.Add(xmlFileEntry);

                // Сохраняем подпись
                await using var sigStream = signature.OpenReadStream();
                var sigStorageKey = await fileStorage.SaveAsync(sigStream, signature.FileName, signature.ContentType);

                var sigFileEntry = new FileEntry
                {
                    Id = Guid.NewGuid(),
                    OriginalName = signature.FileName,
                    ContentType = signature.ContentType,
                    SizeBytes = signature.Length,
                    StorageProvider = "LOCAL",
                    StorageKeyOrPath = sigStorageKey,
                    IsUploaded = true,
                    Extension = sigExt?.TrimStart('.')
                };
                ctx.Files.Add(sigFileEntry);

                var entity = new BoardRegistryUpload
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = leId.Value,
                    XmlFileId = xmlFileEntry.Id,
                    XmlOriginalName = file.FileName,
                    SignatureFileId = sigFileEntry.Id,
                    SignatureOriginalName = signature.FileName,
                    Status = "uploaded",
                    UploadedBy = userId,
                    UploadedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                ctx.BoardRegistryUploads.Add(entity);
                await ctx.SaveChangesAsync();

                logger.LogInformation("[{Ip}] Загружен XML реестра: {FileName} + подпись: {SigName}, id={Id}",
                    ClientIpHelper.GetClientIp(http), file.FileName, signature.FileName, entity.Id);

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
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
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
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
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
            HttpContext http,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
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
                var llcCheck = await ValidateParticipantAccessAsync(ctx, http, audit, logger);
                if (llcCheck is not null) return llcCheck;

                var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
                if (leId is null)
                    return Results.BadRequest(new { error = "Юридическое лицо не определено" });

                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = Guid.TryParse(userIdStr, out var uid) ? uid : null;

                var entity = new BoardParticipantChange
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = leId.Value,
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
                    Source = dto.Source ?? "electronic",
                    Date = dto.Date,
                    PaperDocNumber = dto.PaperDocNumber,
                    Comment = dto.Comment,
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
                var llcCheck = await ValidateParticipantAccessAsync(ctx, http, audit, logger);
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
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
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
                var llcCheck = await ValidateAccessAsync(ctx, http, audit, logger);
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
        ISecurityAuditService audit,
        ILogger logger)
    {
        var (login, _) = await GetUserInfoAsync(ctx, http);
        if (string.IsNullOrEmpty(login))
        {
            logger.LogWarning("Не удалось определить пользователя");
            return Results.BadRequest(new { error = "Не удалось определить пользователя" });
        }

        var participant = await ctx.EcosystemParticipants
            .FirstOrDefaultAsync(ep => ep.Login == login);

        if (participant is null)
        {
            logger.LogWarning("Пользователь {Login} не привязан к ЮЛ", login);
            return Results.BadRequest(new { error = "Пользователь не привязан к юридическому лицу" });
        }

        var leId = participant.LegalEntityId;

        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId);

        if (le is null)
        {
            logger.LogWarning("Юридическое лицо не найдено для Id={LeId}", leId);
            return Results.BadRequest(new { error = "Юридическое лицо не найдено" });
        }

        var clientIp = ClientIpHelper.GetClientIp(http);
        var (_, fullName) = await GetUserInfoAsync(ctx, http);

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
        ISecurityAuditService audit,
        ILogger logger)
    {
        var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
        if (leId is null || leId == Guid.Empty)
        {
            logger.LogWarning("Юридическое лицо не выбрано (participant access)");
            return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });
        }

        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == leId.Value);

        if (le is null)
        {
            logger.LogWarning("Юридическое лицо не найдено для Id={LeId} (participant access)", leId);
            return Results.BadRequest(new { error = "Юридическое лицо не найдено" });
        }

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

        var login = user.Login;
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

    /// <summary>
    /// Fire-and-forget: привязка ЕДИН MasterId к пользователю после сохранения участника.
    /// Выполняется асинхронно, не блокирует ответ API.
    /// </summary>
    private static async Task TriggerEdinBindingAsync(
        IServiceProvider serviceProvider,
        ILogger logger,
        FiduciaDbContext ctx,
        BoardParticipant entity)
    {
        logger.LogInformation("ЕДИН: TriggerEdinBinding вызван для участника {Name}, EcosystemParticipantId={EcoId}",
            entity.Person?.FullName, entity.EcosystemParticipantId?.ToString() ?? "NULL");
        try
        {
            var bindingService = serviceProvider.GetService<IEdinBindingService>();
            if (bindingService is null)
            {
                logger.LogWarning("ЕДИН: IEdinBindingService не зарегистрирован");
                return;
            }

            if (entity.ParticipantType != "FL")
            {
                logger.LogWarning("ЕДИН: пропуск — тип участника {Type}", entity.ParticipantType);
                return;
            }

            if (string.IsNullOrWhiteSpace(entity.Person?.FullName))
            {
                logger.LogWarning("ЕДИН: пропуск — ФИО пустое");
                return;
            }

            if (!entity.EcosystemParticipantId.HasValue)
            {
                logger.LogWarning("ЕДИН: пропуск — EcosystemParticipantId не установлен");
                return;
            }

            // Сохраняем данные ДО dispose контекста
            var ecoId = entity.EcosystemParticipantId.Value;
            var lastName = entity.Person?.LastName ?? "";
            var firstName = entity.Person?.FirstName ?? "";
            var middleName = entity.Person?.MiddleName;
            var personInn = entity.Person?.Inn;
            var personId = entity.PersonId;

            // Ищем паспорт через новый контекст
            string? dulType = null;
            string? passportSeries = null;
            string? passportNumber = null;
            var dbFactory = serviceProvider.GetRequiredService<IDbContextFactory<FiduciaDbContext>>();
            await using var freshCtx = await dbFactory.CreateDbContextAsync();
            if (personId.HasValue)
            {
                var primaryDoc = await freshCtx.IdentityDocuments
                    .Include(x => x.DulType)
                    .FirstOrDefaultAsync(x => x.PersonId == personId.Value && x.IsActive);
                dulType = primaryDoc?.DulType?.Code;
                passportSeries = primaryDoc?.Series;
                passportNumber = primaryDoc?.Number;
            }

            logger.LogDebug("ЕДИН: запуск binding для EcosystemParticipant={EcoId}, ФИО={LastName} {FirstName}", ecoId, lastName, firstName);

            // Создаём новый scope для Task.Run, чтобы контекст БД не был disposed
            var scope = serviceProvider.CreateScope();
            var scopedBindingService = scope.ServiceProvider.GetRequiredService<IEdinBindingService>();

            _ = Task.Run(async () =>
            {
                try
                {
                    await scopedBindingService.ResolveAndBindAsync(
                        ecoId, lastName, firstName, middleName,
                        personInn, null,
                        dulType, passportSeries, passportNumber);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "ЕДИН: ошибка привязки для участника {Name}", entity.Person?.FullName);
                }
                finally
                {
                    scope.Dispose();
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "ЕДИН: ошибка запуска привязки для участника {Name}", entity.Person?.FullName);
        }
    }

    /// <summary>Разбивает ФИО на фамилию, имя и отчество.</summary>
    private static (string LastName, string FirstName, string? MiddleName) SplitFullName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            >= 3 => (parts[0], parts[1], parts[2]),
            2 => (parts[0], parts[1], null),
            1 => (parts[0], parts[0], null),
            _ => ("", "", null)
        };
    }

    private static object MapParticipantToDto(BoardParticipant p, IdentityDocument? primaryDoc = null)
    {
        return new
        {
            p.Id,
            p.LegalEntityId,
            p.EcosystemParticipantId,
            p.ParticipantType,
            p.PersonId,
            FullName = p.Person?.FullName,
            MpiMasterId = p.EcosystemParticipant?.MpiMasterId,
            DulTypeId = primaryDoc?.DulTypeId,
            PassportSeries = primaryDoc?.Series,
            PassportNumber = primaryDoc?.Number,
            PassportIssuedBy = primaryDoc?.IssuedBy,
            PassportIssueDate = primaryDoc?.IssueDate?.ToString("dd.MM.yyyy"),
            PassportDepartmentCode = primaryDoc?.DepartmentCode,
            PassportRegistrationAddress = primaryDoc?.RegistrationAddress,
            PersonInn = p.Person?.Inn,
            Citizenship = p.Person?.Citizenship,
            p.CompanyName,
            p.CompanyInn,
            p.CompanyOgrn,
            p.CompanyKpp,
            p.CompanyAddress,
            Ogrnip = p.Person?.Ogrnip,
            p.SharePercent,
            p.ShareAmount,
            p.PaymentInfo,
            p.ShareRegistrationInfo,
            EntryDate = p.EntryDate?.ToString("dd.MM.yyyy"),
            ExitDate = p.ExitDate?.ToString("dd.MM.yyyy"),
            p.IsActive,
            p.SortOrder
        };
    }

    private static BoardParticipant MapDtoToEntity(BoardParticipantDto dto, Guid legalEntityId) => new()
    {
        LegalEntityId = legalEntityId,
        EcosystemParticipantId = dto.EcosystemParticipantId,
        ParticipantType = dto.ParticipantType ?? "FL",
        CompanyName = dto.CompanyName,
        CompanyInn = dto.CompanyInn,
        CompanyOgrn = dto.CompanyOgrn,
        CompanyKpp = dto.CompanyKpp,
        CompanyAddress = dto.CompanyAddress,
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
        c.Source,
        c.Date,
        c.PaperDocNumber,
        c.Comment,
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
        public Guid? EcosystemParticipantId { get; init; }
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
        public string? Source { get; init; }
        public string? Date { get; init; }
        public string? PaperDocNumber { get; init; }
        public string? Comment { get; init; }
    }

    /// <summary>DTO для рассмотрения заявки.</summary>
    public record ReviewDto
    {
        public string Status { get; init; } = "pending";
        public string? Comment { get; init; }
    }

    /// <summary>
    /// Проверка дубликата участника по ДУЛ/INN в пределах одного ЮЛ.
    /// Возвращает сообщение об ошибке или null если дубликатов нет.
    /// </summary>
    private static async Task<string?> CheckDuplicateParticipantAsync(
        FiduciaDbContext ctx,
        Guid legalEntityId,
        BoardParticipant entity,
        Guid? excludeId)
    {
        var query = ctx.BoardParticipants
            .Where(p => p.LegalEntityId == legalEntityId
                     && p.IsActive
                     && p.Id != (excludeId ?? Guid.Empty));

        // Проверка по поисковому ключу ДУЛ (нормализованная строка)
        var personId = entity.PersonId;
        if (personId.HasValue)
        {
            var primaryDoc = await ctx.IdentityDocuments
                .FirstOrDefaultAsync(x => x.PersonId == personId.Value && x.IsActive);
            if (primaryDoc is not null)
            {
                var searchKey = ComputeDulSearchKey(primaryDoc);
                if (!string.IsNullOrEmpty(searchKey))
                {
                    var existingDoc = await ctx.IdentityDocuments
                        .AnyAsync(d => d.PersonId != personId.Value
                            && d.IsActive
                            && d.Series == primaryDoc.Series
                            && d.Number == primaryDoc.Number
                            && d.DulTypeId == primaryDoc.DulTypeId);
                    if (existingDoc)
                        return "Участник с таким документом уже добавлен";
                }
            }
        }

        // Проверка по ИНН (ФЛ)
        var personInn = entity.Person?.Inn;
        if (entity.ParticipantType == "FL" && !string.IsNullOrEmpty(personInn))
        {
            if (await query.AnyAsync(p => p.Person != null && p.Person.Inn == personInn))
                return "Участник с таким ИНН уже добавлен";
        }

        // Проверка по ИНН (ЮЛ)
        if (entity.ParticipantType == "UL" && !string.IsNullOrEmpty(entity.CompanyInn))
        {
            if (await query.AnyAsync(p => p.ParticipantType == "UL" && p.CompanyInn == entity.CompanyInn))
                return "Юридическое лицо с таким ИНН уже добавлено";
        }

        return null;
    }

    /// <summary>
    /// Вычисляет поисковый ключ ДУЛ для дедупликации.
    /// Формат: DulTypeId|normalized_series|normalized_number
    /// </summary>
    private static string? ComputeDulSearchKey(IdentityDocument doc)
    {
        if (string.IsNullOrEmpty(doc.Number))
            return null;

        var series = NormalizeDulField(doc.Series ?? "");
        var number = NormalizeDulField(doc.Number);
        return $"{doc.DulTypeId}|{series}|{number}";
    }

    /// <summary>Нормализация поля ДУЛ: удаление пробелов/дефисов, верхний регистр.</summary>
    private static string NormalizeDulField(string value) =>
        value.Replace(" ", "").Replace("-", "").ToUpperInvariant();
}
