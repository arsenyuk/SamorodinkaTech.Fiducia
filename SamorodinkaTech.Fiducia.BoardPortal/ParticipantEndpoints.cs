using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для реестра участников общества (Board Portal).
/// Доступно только для ООО (ОКОПФ 12300). Все попытки доступа логируются в аудит.
/// </summary>
public static class ParticipantEndpoints
{
    private const string LlcOkopfCode = "12300";
    private const string AuditActionMutation = "PARTICIPANT_MUTATION";
    private const string AuditActionDenied = "PARTICIPANT_ACCESS_DENIED";

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
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var llcCheck = await CheckIsLlcAsync(ctx, http, audit);
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

            await audit.LogEventAsync(AuditActionMutation, GetClientIp(http),
                $"Добавлен участник: {entity.FullName ?? entity.CompanyName}, доля={entity.SharePercent}%",
                entityName: "BoardParticipant", entityId: entity.Id);

            return Results.Ok(MapParticipantToDto(entity));
        });

        // PUT: обновление участника
        participants.MapPut("/{id}", async (
            Guid id,
            HttpContext http,
            BoardParticipantDto dto,
            ISecurityAuditService audit,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var llcCheck = await CheckIsLlcAsync(ctx, http, audit);
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

            await audit.LogEventAsync(AuditActionMutation, GetClientIp(http),
                $"Обновлён участник: {entity.FullName ?? entity.CompanyName}, id={id}",
                entityName: "BoardParticipant", entityId: id);

            return Results.Ok(MapParticipantToDto(entity));
        });

        // DELETE: удаление участника
        participants.MapDelete("/{id}", async (
            Guid id,
            HttpContext http,
            ISecurityAuditService audit,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var llcCheck = await CheckIsLlcAsync(ctx, http, audit);
            if (llcCheck is not null) return llcCheck;

            var entity = await ctx.BoardParticipants.FindAsync(id);
            if (entity is null) return Results.NotFound();

            ctx.BoardParticipants.Remove(entity);
            await ctx.SaveChangesAsync();

            await audit.LogEventAsync(AuditActionMutation, GetClientIp(http),
                $"Удалён участник: {entity.FullName ?? entity.CompanyName}, id={id}",
                entityName: "BoardParticipant", entityId: id);

            return Results.Ok();
        });

        // POST: импорт участников из СПАРК
        participants.MapPost("/import-from-spark", async (
            HttpContext http,
            ISecurityAuditService audit,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var llcCheck = await CheckIsLlcAsync(ctx, http, audit);
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

            await audit.LogEventAsync(AuditActionMutation, GetClientIp(http),
                $"Импорт из СПАРК: {imported.Count} участников, ЮЛ={le.Inn}",
                entityName: "BoardParticipant");

            return Results.Ok(new { imported = imported.Count, participants = imported });
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
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var llcCheck = await CheckIsLlcAsync(ctx, http, audit);
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

            await audit.LogEventAsync(AuditActionMutation, GetClientIp(http),
                $"Добавлена казначейская доля: {entity.SharePercent}%, id={entity.Id}",
                entityName: "BoardTreasuryShare", entityId: entity.Id);

            return Results.Ok(MapTreasuryToDto(entity));
        });

        // PUT: обновление казначейской доли
        treasuryShares.MapPut("/{id}", async (
            Guid id,
            HttpContext http,
            BoardTreasuryShareDto dto,
            ISecurityAuditService audit,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var llcCheck = await CheckIsLlcAsync(ctx, http, audit);
            if (llcCheck is not null) return llcCheck;

            var entity = await ctx.BoardTreasuryShares.FindAsync(id);
            if (entity is null) return Results.NotFound();

            entity.SharePercent = dto.SharePercent;
            entity.ShareAmount = dto.ShareAmount;
            entity.AcquiredDate = dto.AcquiredDate;
            entity.AcquisitionBasis = dto.AcquisitionBasis;
            entity.UpdatedAt = DateTime.UtcNow;

            await ctx.SaveChangesAsync();

            await audit.LogEventAsync(AuditActionMutation, GetClientIp(http),
                $"Обновлена казначейская доля: {entity.SharePercent}%, id={id}",
                entityName: "BoardTreasuryShare", entityId: id);

            return Results.Ok(MapTreasuryToDto(entity));
        });

        // DELETE: удаление казначейской доли
        treasuryShares.MapDelete("/{id}", async (
            Guid id,
            HttpContext http,
            ISecurityAuditService audit,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var llcCheck = await CheckIsLlcAsync(ctx, http, audit);
            if (llcCheck is not null) return llcCheck;

            var entity = await ctx.BoardTreasuryShares.FindAsync(id);
            if (entity is null) return Results.NotFound();

            ctx.BoardTreasuryShares.Remove(entity);
            await ctx.SaveChangesAsync();

            await audit.LogEventAsync(AuditActionMutation, GetClientIp(http),
                $"Удалена казначейская доля: {entity.SharePercent}%, id={id}",
                entityName: "BoardTreasuryShare", entityId: id);

            return Results.Ok();
        });
    }

    /// <summary>
    /// Проверяет, является ли текущее ЮЛ ООО (ОКОПФ 12300).
    /// Если нет — логирует попытку доступа и возвращает Forbid.
    /// </summary>
    private static async Task<IResult?> CheckIsLlcAsync(
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

        if (le.RefOkopf?.Code != LlcOkopfCode)
        {
            await audit.LogEventAsync(AuditActionDenied, GetClientIp(http),
                $"Доступ к реестру участников запрещён: ЮЛ «{le.Name}» (ОКОПФ {le.RefOkopf?.Code}) не является ООО",
                entityName: "LegalEntity", entityId: le.Id);
            return Results.Forbid();
        }

        return null;
    }

    private static string GetClientIp(HttpContext http) =>
        http.Connection.RemoteIpAddress?.ToString() ?? "unknown";

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
}
