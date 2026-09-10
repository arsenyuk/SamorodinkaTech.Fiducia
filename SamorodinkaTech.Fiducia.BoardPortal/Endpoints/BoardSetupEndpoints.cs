using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Infrastructure.Common;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для первичного ввода состава СД (Board Portal).
/// Доступно для ООО с нестандартным уставом, имеющим Совет директоров.
/// </summary>
public static class BoardSetupEndpoints
{
    private const string LlcOkopfCode = "12300";

    /// <summary>
    /// Регистрирует все endpoint'ы группы BoardSetup.
    /// </summary>
    public static void MapBoardSetupEndpoints(this WebApplication app)
    {
        var boardSetup = app.MapGroup("/api/v1/board-setup")
            .RequireAuthorization()
            .WithTags("Board Setup");

        // GET: загрузка текущего состава СД + настроек
        boardSetup.MapGet("/", async (
            Guid? legalEntityId,
            HttpContext http,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var leId = legalEntityId;
            if (leId is null || leId == Guid.Empty)
                leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
            if (leId is null || leId == Guid.Empty)
                return Results.BadRequest(new { error = "Юридическое лицо не определено" });

            var boardSettings = await ctx.LegalEntityBoardSettings.FirstOrDefaultAsync();
            var charter = await ctx.LegalEntityCharters.FirstOrDefaultAsync(c => c.LegalEntityId == leId.Value);

            var roles = await ctx.BoardParticipantRoles
                .Include(r => r.Participant)
                .Include(r => r.Role)
                .Where(r => r.Participant != null && r.Participant.LegalEntityId == leId.Value && r.Participant.IsActive)
                .ToListAsync();

            var result = new
            {
                legalEntityId = leId.Value,
                deputyChairProvided = boardSettings?.DeputyChairProvided ?? false,
                secretaryProvided = boardSettings?.SecretaryProvided ?? false,
                secretarySignsProtocols = boardSettings?.SecretarySignsProtocols ?? false,
                hasBoardOfDirectors = charter?.HasBoardOfDirectors ?? false,
                assignments = roles.Select(r => new
                {
                    participantId = r.ParticipantId,
                    fullName = r.Participant?.Person?.FullName,
                    personInn = r.Participant?.Person?.Inn,
                    roleCode = r.Role?.Code,
                    roleName = r.Role?.Name,
                    ecosystemBound = r.Participant?.EcosystemParticipantId.HasValue ?? false,
                    assignedAt = r.AssignedAt
                })
            };

            return Results.Ok(result);
        });

        // GET: поиск участников/сотрудников ЮЛ для добавления в СД
        boardSetup.MapGet("/search-participants", async (
            string? query,
            HttpContext http,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http);
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(Array.Empty<object>());

            var q = ctx.BoardParticipants
                .Where(p => p.LegalEntityId == leId.Value && p.IsActive && p.ParticipantType == "FL");

            if (!string.IsNullOrWhiteSpace(query))
            {
                var search = query.Trim().ToLower();
                q = q.Where(p =>
                    (p.Person != null && p.Person.FullName.ToLower().Contains(search)) ||
                    (p.Person != null && p.Person.Inn != null && p.Person.Inn.Contains(search)) ||
                    ctx.IdentityDocuments.Any(d =>
                        d.PersonId == p.PersonId &&
                        ((d.Series != null && d.Series.Contains(search)) ||
                         (d.Number != null && d.Number.Contains(search)))));
            }

            var items = await q
                .OrderBy(p => p.Person != null ? p.Person.FullName : null)
                .Take(50)
                .Include(p => p.Person)
                .ToListAsync();

            return Results.Ok(items.Select(p =>
            {
                var primaryDoc = ctx.IdentityDocuments.FirstOrDefault(x => x.PersonId == p.PersonId && x.IsActive);
                return new
                {
                    participantId = p.Id,
                    fullName = p.Person?.FullName,
                    personInn = p.Person?.Inn,
                    dulTypeId = primaryDoc?.DulTypeId,
                    passportSeries = primaryDoc?.Series,
                    passportNumber = primaryDoc?.Number,
                    snils = p.Person?.Snils,
                    ecosystemBound = p.EcosystemParticipantId.HasValue
                };
            }));
        });

        // POST: валидация назначений
        boardSetup.MapPost("/validate", async (
            HttpContext http,
            BoardSetupValidateRequest request,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var leId = request.LegalEntityId;
            if (leId == Guid.Empty)
                leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http) ?? Guid.Empty;
            if (leId == Guid.Empty)
                return Results.BadRequest(new { error = "Юридическое лицо не определено" });

            var errors = await BoardSetupValidator.ValidateAsync(request.Assignments, ctx, leId);

            return Results.Ok(new
            {
                valid = errors.Count == 0,
                errors
            });
        });

        // POST: сохранение состава СД
        boardSetup.MapPost("/save", async (
            HttpContext http,
            BoardSetupSaveRequest request,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("BoardSetup");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();
                var leId = request.LegalEntityId;
                if (leId == Guid.Empty)
                    leId = await LegalEntityHelper.GetLegalEntityIdAsync(ctx, http) ?? Guid.Empty;
                if (leId == Guid.Empty)
                    return Results.BadRequest(new { error = "Юридическое лицо не определено" });

                // Валидация
                var errors = await BoardSetupValidator.ValidateAsync(request.Assignments, ctx, leId);
                if (errors.Count > 0)
                    return Results.BadRequest(new { error = "Ошибки валидации", errors });

                var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = Guid.TryParse(userIdStr, out var uid) ? uid : null;

                // Удаляем существующие роли участников этого ЮЛ
                var existingRoles = await ctx.BoardParticipantRoles
                    .Include(r => r.Participant)
                    .Where(r => r.Participant != null && r.Participant.LegalEntityId == leId)
                    .ToListAsync();

                ctx.BoardParticipantRoles.RemoveRange(existingRoles);

                // Создаём новые назначения
                foreach (var assignment in request.Assignments)
                {
                    var role = await ctx.BoardRoles.FirstOrDefaultAsync(r => r.Code == assignment.RoleCode);
                    if (role is null) continue;

                    var participantRole = new BoardParticipantRole
                    {
                        Id = Guid.NewGuid(),
                        ParticipantId = assignment.ParticipantId,
                        RoleId = role.Id,
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = userId
                    };

                    ctx.BoardParticipantRoles.Add(participantRole);

                    logger.LogInformation("Назначена роль {RoleCode} участнику {ParticipantId}, ЮЛ={LeId}",
                        assignment.RoleCode, assignment.ParticipantId, leId);
                }

                await ctx.SaveChangesAsync();

                return Results.Ok(new { success = true, rolesCreated = request.Assignments.Count });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка сохранения состава СД: {Error}", UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });

        // POST: удаление роли участника из СД
        boardSetup.MapPost("/remove-role", async (
            HttpContext http,
            BoardSetupRemoveRoleRequest request,
            ILoggerFactory loggerFactory,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            var logger = loggerFactory.CreateLogger("BoardSetup");
            try
            {
                await using var ctx = await dbFactory.CreateDbContextAsync();

                var role = await ctx.BoardRoles.FirstOrDefaultAsync(r => r.Code == request.RoleCode);
                if (role is null)
                    return Results.BadRequest(new { error = $"Неизвестная роль: {request.RoleCode}" });

                var existing = await ctx.BoardParticipantRoles
                    .FirstOrDefaultAsync(r => r.ParticipantId == request.ParticipantId && r.RoleId == role.Id);

                if (existing is null)
                    return Results.NotFound();

                ctx.BoardParticipantRoles.Remove(existing);
                await ctx.SaveChangesAsync();

                logger.LogInformation("Удалена роль {RoleCode} у участника {ParticipantId}", request.RoleCode, request.ParticipantId);

                return Results.Ok(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Ошибка удаления роли: {Error}", UnwrapException(ex));
                return Results.BadRequest(new { error = UnwrapException(ex) });
            }
        });
    }

    private static string UnwrapException(Exception ex)
    {
        var current = ex;
        while (current.InnerException != null)
            current = current.InnerException;
        return current.Message;
    }
}

/// <summary>DTO для валидации.</summary>
public class BoardSetupValidateRequest
{
    public Guid LegalEntityId { get; set; }
    public List<BoardSetupAssignment> Assignments { get; set; } = new();
}

/// <summary>DTO для сохранения.</summary>
public class BoardSetupSaveRequest
{
    public Guid LegalEntityId { get; set; }
    public List<BoardSetupAssignment> Assignments { get; set; } = new();
}

/// <summary>DTO назначения роли.</summary>
public class BoardSetupAssignment
{
    public Guid ParticipantId { get; set; }
    public string RoleCode { get; set; } = default!;
}

/// <summary>DTO для удаления роли.</summary>
public class BoardSetupRemoveRoleRequest
{
    public Guid ParticipantId { get; set; }
    public string RoleCode { get; set; } = default!;
}
