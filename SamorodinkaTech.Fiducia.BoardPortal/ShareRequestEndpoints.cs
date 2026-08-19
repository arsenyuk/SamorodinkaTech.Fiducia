using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для запросов участника ООО в общество (Board Portal).
/// Типы: PREEMPTIVE_LIST, NOTARIAL_OFFER, EXIT_APPLICATION, MANDATORY_BUYBACK.
/// </summary>
public static class ShareRequestEndpoints
{
    private const string LlcOkopfCode = "12300";

    public static void MapShareRequestEndpoints(this WebApplication app)
    {
        var shareRequests = app.MapGroup("/api/share-requests")
            .RequireAuthorization()
            .WithTags("Share Requests");

        // GET: список запросов текущего участника
        shareRequests.MapGet("/", async (
            string? type,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            HttpContext http) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var (leId, participantId, error) = await ResolveParticipantAsync(ctx, http);
            if (error is not null) return error;

            var query = ctx.ShareRequests
                .Where(r => r.LegalEntityId == leId && r.ParticipantId == participantId);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(r => r.RequestType == type);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Results.Ok(items.Select(MapToDto));
        });

        // GET: один запрос по ID
        shareRequests.MapGet("/{id}", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            HttpContext http) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var (leId, participantId, error) = await ResolveParticipantAsync(ctx, http);
            if (error is not null) return error;

            var item = await ctx.ShareRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId && r.ParticipantId == participantId);

            return item is null
                ? Results.NotFound()
                : Results.Ok(MapToDto(item));
        });

        // POST: создать запрос
        shareRequests.MapPost("/", async (
            ShareRequestCreateDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            HttpContext http) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var (leId, participantId, error) = await ResolveParticipantAsync(ctx, http);
            if (error is not null) return error;

            // Проверка допустимых типов
            var allowedTypes = new[] { "PREEMPTIVE_LIST", "NOTARIAL_OFFER", "EXIT_APPLICATION", "MANDATORY_BUYBACK" };
            if (!allowedTypes.Contains(dto.RequestType))
                return Results.BadRequest(new { error = $"Недопустимый тип запроса: {dto.RequestType}" });

            // Проверка флагов устава
            var charter = await ctx.LegalEntityCharters.FindAsync(leId);
            if (charter is not null)
            {
                if (dto.RequestType == "PREEMPTIVE_LIST" && !charter.PreemptiveRight)
                    return Results.BadRequest(new { error = "Преимущественное право не действует" });

                if (dto.RequestType == "EXIT_APPLICATION" && !charter.ExitAllowed)
                    return Results.BadRequest(new { error = "Выход из ООО не предусмотрен уставом" });
            }

            var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var createdBy = Guid.TryParse(userIdStr, out var uid) ? uid : Guid.Empty;

            var entity = new ShareRequest
            {
                Id = Guid.NewGuid(),
                LegalEntityId = leId,
                ParticipantId = participantId,
                RequestType = dto.RequestType,
                Status = "pending",
                Payload = dto.Payload,
                CreatedBy = createdBy
            };

            ctx.ShareRequests.Add(entity);
            await ctx.SaveChangesAsync();

            return Results.Ok(MapToDto(entity));
        });

        // POST: завершить запрос (администрация)
        shareRequests.MapPost("/{id}/complete", async (
            Guid id,
            ShareRequestCompleteDto dto,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            HttpContext http) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var (leId, _, error) = await ResolveParticipantAsync(ctx, http);
            if (error is not null) return error;

            var item = await ctx.ShareRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId);

            if (item is null) return Results.NotFound();

            item.Status = dto.Status ?? "completed";
            item.CompletedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(dto.Payload))
                item.Payload = dto.Payload;

            await ctx.SaveChangesAsync();
            return Results.Ok(MapToDto(item));
        });

        // GET: результат запроса (PREEMPTIVE_LIST — список участников)
        shareRequests.MapGet("/{id}/result", async (
            Guid id,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            HttpContext http) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var (leId, participantId, error) = await ResolveParticipantAsync(ctx, http);
            if (error is not null) return error;

            var item = await ctx.ShareRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.LegalEntityId == leId && r.ParticipantId == participantId);

            if (item is null) return Results.NotFound();
            if (item.RequestType != "PREEMPTIVE_LIST")
                return Results.BadRequest(new { error = "Результат доступен только для запроса списка участников" });
            if (item.Status != "completed")
                return Results.BadRequest(new { error = "Запрос ещё не завершён" });

            // Возвращаем список участников из БД (кроме самого запросившего)
            var participants = await ctx.BoardParticipants
                .Where(p => p.LegalEntityId == leId && p.IsActive && p.Id != participantId)
                .Select(p => new
                {
                    ParticipantId = p.Id,
                    p.FullName,
                    p.CompanyName,
                    p.ParticipantType,
                    p.SharePercent,
                    p.ShareAmount
                })
                .ToListAsync();

            return Results.Ok(participants);
        });

        // GET: текущий устав ООО (для UI — определение доступных типов запросов)
        app.MapGet("/api/charter/current", async (
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
            var leId = workplace?.LastSelectedLegalEntityId;
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(new { preemptiveRight = true, exitAllowed = false, transferToThirdPartiesWithoutConsent = false });

            var charter = await ctx.LegalEntityCharters.FindAsync(leId.Value);
            if (charter is null)
                return Results.Ok(new { preemptiveRight = true, exitAllowed = false, transferToThirdPartiesWithoutConsent = false });

            return Results.Ok(new
            {
                preemptiveRight = charter.PreemptiveRight,
                exitAllowed = charter.ExitAllowed,
                transferToThirdPartiesWithoutConsent = charter.TransferToThirdPartiesWithoutConsent
            });
        }).RequireAuthorization();
    }

    private static async Task<(Guid leId, Guid participantId, IResult? error)> ResolveParticipantAsync(
        FiduciaDbContext ctx, HttpContext http)
    {
        var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
        var leId = workplace?.LastSelectedLegalEntityId;
        if (leId is null || leId == Guid.Empty)
            return (Guid.Empty, Guid.Empty, Results.BadRequest(new { error = "Юридическое лицо не выбрано" }));

        var userIdStr = http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return (Guid.Empty, Guid.Empty, Results.Forbid());

        // Найти участника по user_id через связь person → participant
        var user = await ctx.Users.FindAsync(userId);
        if (user?.PersonId is null)
            return (Guid.Empty, Guid.Empty, Results.BadRequest(new { error = "Пользователь не привязан к участнику" }));

        var participant = await ctx.BoardParticipants
            .FirstOrDefaultAsync(p => p.LegalEntityId == leId.Value && p.Id == user.PersonId);

        if (participant is null)
            return (Guid.Empty, Guid.Empty, Results.BadRequest(new { error = "Участник не найден в реестре" }));

        return (leId.Value, participant.Id, null);
    }

    private static object MapToDto(ShareRequest r) => new
    {
        r.Id,
        r.LegalEntityId,
        r.ParticipantId,
        r.RequestType,
        r.Status,
        r.Payload,
        r.CreatedAt,
        r.CompletedAt
    };
}

public record ShareRequestCreateDto(string RequestType, string? Payload);
public record ShareRequestCompleteDto(string? Status, string? Payload);
