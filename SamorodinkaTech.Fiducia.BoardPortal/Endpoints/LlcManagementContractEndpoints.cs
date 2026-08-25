using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Validation;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для договоров ООО с управляющими ИП (ст. 42 14-ФЗ).
/// </summary>
public static class LlcManagementContractEndpoints
{
    /// <summary>
    /// Регистрирует все endpoint'ы группы LLC Management Contracts.
    /// </summary>
    public static void MapLlcManagementContractEndpoints(this WebApplication app)
    {
        var contracts = app.MapGroup("/api/llc-management-contracts")
            .RequireAuthorization()
            .WithTags("LLC Management Contracts");

        // GET: поиск ИП в СПАРК по ИНН/ОГРНИП
        contracts.MapGet("/search-spark", async (
            string query,
            [Microsoft.AspNetCore.Mvc.FromServices] ISparkApiClient? sparkApi,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("LlcMgmtContracts.SearchSpark");
            if (string.IsNullOrWhiteSpace(query))
                return Results.BadRequest(new { error = "Введите ИНН или ОГРНИП" });

            query = query.Trim();

            // Определяем тип ввода и валидируем
            if (query.Length == InnIpValidator.InnIpLength)
            {
                var (isValid, error) = InnIpValidator.Validate(query);
                if (!isValid)
                    return Results.BadRequest(new { error });
            }
            else if (query.Length == OgrnipValidator.OgrnipLength)
            {
                var (isValid, error) = OgrnipValidator.Validate(query);
                if (!isValid)
                    return Results.BadRequest(new { error });
            }
            else
            {
                return Results.BadRequest(new { error = "ИНН ИП должен содержать 12 цифр, ОГРНИП — 15 цифр" });
            }

            await using var ctx = await dbFactory.CreateDbContextAsync();

            // Если введён ИНН — ищем по ИНН
            if (query.Length == InnIpValidator.InnIpLength)
            {
                // Проверяем кэш ext_spark_company
                var cached = await ctx.ExtSparkCompanies.FirstOrDefaultAsync(c => c.Inn == query);
                if (cached is not null)
                {
                    return Results.Ok(new
                    {
                        inn = cached.Inn,
                        fullName = cached.FullName,
                        shortName = cached.ShortName,
                        ogrn = cached.Ogrn,
                        status = cached.Status,
                        isActing = cached.Status == "Действующее"
                    });
                }

                // Ищем в СПАРК
                if (sparkApi is null)
                    return Results.Ok(new { warning = "ИП не найден в кэше. СПАРК API не настроен." });

                try
                {
                    var company = await sparkApi.GetCompanyByInnAsync(query);
                    if (company is null)
                        return Results.Ok(new { warning = "ИП не найден в СПАРК" });

                    return Results.Ok(new
                    {
                        inn = company.Inn,
                        fullName = company.FullName,
                        shortName = company.ShortName,
                        ogrn = company.Ogrn,
                        status = company.Status,
                        isActing = company.IsActing
                    });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Ошибка СПАРК для ИНН {Inn}", query);
                    return Results.Ok(new { warning = $"Ошибка СПАРК: {ex.Message}" });
                }
            }

            // Если введён ОГРНИП — ищем по кэшу (СПАРК API не поддерживает поиск по ОГРНИП напрямую)
            var cachedByOgrnip = await ctx.ExtSparkCompanies.FirstOrDefaultAsync(c => c.Ogrn == query);
            if (cachedByOgrnip is not null)
            {
                return Results.Ok(new
                {
                    inn = cachedByOgrnip.Inn,
                    fullName = cachedByOgrnip.FullName,
                    shortName = cachedByOgrnip.ShortName,
                    ogrn = cachedByOgrnip.Ogrn,
                    status = cachedByOgrnip.Status,
                    isActing = cachedByOgrnip.Status == "Действующее"
                });
            }

            return Results.Ok(new { warning = "ИП с указанным ОГРНИП не найден в кэше. Введите ИНН для поиска в СПАРК." });
        });

        // GET: список договоров текущего ЮЛ
        contracts.MapGet("/", async (
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
            var leId = workplace?.LastSelectedLegalEntityId;
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(Array.Empty<object>());

            var items = await ctx.LlcManagementContracts
                .Where(c => c.LegalEntityId == leId.Value)
                .OrderByDescending(c => c.IsActive)
                .ThenBy(c => c.ManagerFullName)
                .ToListAsync();

            return Results.Ok(items.Select(c => new
            {
                c.Id,
                c.ManagerFullName,
                c.ManagerInn,
                c.ManagerOgrnip,
                c.ContractNumber,
                ContractDate = c.ContractDate?.ToString("dd.MM.yyyy"),
                ContractValidFrom = c.ContractValidFrom.ToString("dd.MM.yyyy"),
                ContractValidTo = c.ContractValidTo?.ToString("dd.MM.yyyy"),
                c.IsIndefinite,
                c.ContractDocumentId,
                c.IsActive
            }));
        });

        // GET: один договор по ID
        contracts.MapGet("/{id}", async (Guid id, IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var c = await ctx.LlcManagementContracts.FindAsync(id);
            if (c is null) return Results.NotFound();

            return Results.Ok(new
            {
                c.Id,
                c.ManagerFullName,
                c.ManagerInn,
                c.ManagerOgrnip,
                c.ContractNumber,
                ContractDate = c.ContractDate?.ToString("dd.MM.yyyy"),
                ContractValidFrom = c.ContractValidFrom.ToString("dd.MM.yyyy"),
                ContractValidTo = c.ContractValidTo?.ToString("dd.MM.yyyy"),
                c.IsIndefinite,
                c.ContractDocumentId,
                c.IsActive
            });
        });

        // POST: создать договор
        contracts.MapPost("/", async (
            HttpContext http,
            IFormFileCollection formFiles,
            IApplicationDbContext db,
            IChunkedUploadService uploadService,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("LlcMgmtContracts.Create");
            if (!HasRole(http.User, "LAWYER") && !HasRole(http.User, "CEO"))
                return Results.Forbid();

            await using var ctx = ((FiduciaDbContext)db);
            var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
            var leId = workplace?.LastSelectedLegalEntityId;
            if (leId is null || leId == Guid.Empty)
                return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });

            var form = await http.Request.ReadFormAsync();
            var managerFullName = form["managerFullName"].FirstOrDefault();
            var managerInn = form["managerInn"].FirstOrDefault();
            var managerOgrnip = form["managerOgrnip"].FirstOrDefault();
            var contractNumber = form["contractNumber"].FirstOrDefault();
            var contractDateStr = form["contractDate"].FirstOrDefault();
            var contractValidFromStr = form["contractValidFrom"].FirstOrDefault();
            var contractValidToStr = form["contractValidTo"].FirstOrDefault();
            var isIndefiniteStr = form["isIndefinite"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(managerFullName))
                return Results.BadRequest(new { error = "ФИО управляющего обязательно" });

            var (innValid, innError) = InnIpValidator.Validate(managerInn);
            if (!innValid)
                return Results.BadRequest(new { error = innError });

            if (!string.IsNullOrWhiteSpace(managerOgrnip))
            {
                var (ogrnipValid, ogrnipError) = OgrnipValidator.Validate(managerOgrnip);
                if (!ogrnipValid)
                    return Results.BadRequest(new { error = ogrnipError });
            }

            if (!DateOnly.TryParse(contractValidFromStr, out var validFrom))
                return Results.BadRequest(new { error = "Дата начала действия обязательна" });

            DateOnly? validTo = null;
            bool isIndefinite = isIndefiniteStr != "false";
            if (!isIndefinite && DateOnly.TryParse(contractValidToStr, out var vt))
                validTo = vt;

            DateOnly? contractDate = null;
            if (DateOnly.TryParse(contractDateStr, out var cd))
                contractDate = cd;

            var entity = new LlcManagementContract
            {
                Id = Guid.NewGuid(),
                LegalEntityId = leId.Value,
                ManagerFullName = managerFullName.Trim(),
                ManagerInn = managerInn!.Trim(),
                ManagerOgrnip = managerOgrnip?.Trim(),
                ContractNumber = contractNumber?.Trim(),
                ContractDate = contractDate,
                ContractValidFrom = validFrom,
                ContractValidTo = validTo,
                IsIndefinite = isIndefinite,
                CreatedBy = Guid.TryParse(
                    http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    out var uid) ? uid : null
            };

            // Загружаем файл договора
            var file = form.Files.FirstOrDefault(f => f.Name == "file");
            if (file is not null && file.Length > 0)
            {
                var uploadId = await uploadService.InitiateUploadAsync(file.FileName, file.ContentType, file.Length);
                await using var stream = file.OpenReadStream();
                await uploadService.UploadChunkAsync(uploadId, 0, stream);
                var fileEntry = await uploadService.CompleteUploadAsync(uploadId);
                entity.ContractDocumentId = fileEntry.Id;
            }

            ctx.LlcManagementContracts.Add(entity);
            await ctx.SaveChangesAsync();
            return Results.Ok(new { entity.Id });
        });

        // PUT: обновить договор
        contracts.MapPut("/{id}", async (
            Guid id,
            HttpContext http,
            IApplicationDbContext db,
            IChunkedUploadService uploadService,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("LlcMgmtContracts.Update");
            if (!HasRole(http.User, "LAWYER") && !HasRole(http.User, "CEO"))
                return Results.Forbid();

            await using var ctx = ((FiduciaDbContext)db);
            var entity = await ctx.LlcManagementContracts.FindAsync(id);
            if (entity is null) return Results.NotFound();

            var form = await http.Request.ReadFormAsync();
            var managerFullName = form["managerFullName"].FirstOrDefault();
            var managerInn = form["managerInn"].FirstOrDefault();
            var managerOgrnip = form["managerOgrnip"].FirstOrDefault();
            var contractNumber = form["contractNumber"].FirstOrDefault();
            var contractDateStr = form["contractDate"].FirstOrDefault();
            var contractValidFromStr = form["contractValidFrom"].FirstOrDefault();
            var contractValidToStr = form["contractValidTo"].FirstOrDefault();
            var isIndefiniteStr = form["isIndefinite"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(managerFullName))
                entity.ManagerFullName = managerFullName.Trim();

            if (!string.IsNullOrWhiteSpace(managerInn))
            {
                var (innValid, innError) = InnIpValidator.Validate(managerInn);
                if (!innValid) return Results.BadRequest(new { error = innError });
                entity.ManagerInn = managerInn.Trim();
            }

            if (!string.IsNullOrWhiteSpace(managerOgrnip))
            {
                var (ogrnipValid, ogrnipError) = OgrnipValidator.Validate(managerOgrnip);
                if (!ogrnipValid) return Results.BadRequest(new { error = ogrnipError });
                entity.ManagerOgrnip = managerOgrnip.Trim();
            }

            entity.ContractNumber = contractNumber?.Trim();
            if (DateOnly.TryParse(contractDateStr, out var cd))
                entity.ContractDate = cd;

            if (DateOnly.TryParse(contractValidFromStr, out var vf))
                entity.ContractValidFrom = vf;

            entity.IsIndefinite = isIndefiniteStr != "false";
            if (!entity.IsIndefinite && DateOnly.TryParse(contractValidToStr, out var vt))
                entity.ContractValidTo = vt;
            else if (entity.IsIndefinite)
                entity.ContractValidTo = null;

            // Загружаем новый файл договора
            var file = form.Files.FirstOrDefault(f => f.Name == "file");
            if (file is not null && file.Length > 0)
            {
                var uploadId = await uploadService.InitiateUploadAsync(file.FileName, file.ContentType, file.Length);
                await using var stream = file.OpenReadStream();
                await uploadService.UploadChunkAsync(uploadId, 0, stream);
                var fileEntry = await uploadService.CompleteUploadAsync(uploadId);
                entity.ContractDocumentId = fileEntry.Id;
            }

            await ctx.SaveChangesAsync();
            return Results.Ok();
        });

        // DELETE: деактивация (soft delete)
        contracts.MapDelete("/{id}", async (Guid id, HttpContext http, IApplicationDbContext db) =>
        {
            if (!HasRole(http.User, "LAWYER") && !HasRole(http.User, "CEO"))
                return Results.Forbid();

            await using var ctx = ((FiduciaDbContext)db);
            var entity = await ctx.LlcManagementContracts.FindAsync(id);
            if (entity is null) return Results.NotFound();

            entity.IsActive = false;
            await ctx.SaveChangesAsync();
            return Results.Ok();
        });
    }

    private static bool HasRole(System.Security.Claims.ClaimsPrincipal user, string role)
    {
        var roleClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
        return roleClaim.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(r => r.Trim().Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}
