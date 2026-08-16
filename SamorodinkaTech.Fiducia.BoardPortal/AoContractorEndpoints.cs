using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.FileStorage;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для контрагентов АО (Регистратор / Информационное агентство).
/// </summary>
public static class AoContractorEndpoints
{
    private const int MinInnLength = 10;
    private const int CbrVidIdRegistrar = 4;
    private const int CbrVidIdInfoAgency = 52;
    private const string CbrFoTypeInfoAgency = "IA";

    /// <summary>
    /// Регистрирует все endpoint'ы группы AO Contractors.
    /// </summary>
    public static void MapAoContractorEndpoints(this WebApplication app)
    {
        // ── AO Contractors API (Регистратор / Информационное агентство) ──────────

        var aoContractors = app.MapGroup("/api/ao-contractors")
            .RequireAuthorization()
            .WithTags("AO Contractors");

        // GET: поиск компании по ИНН — сначала кэш ext_spark_company, затем СПАРК API
        aoContractors.MapGet("/search-spark", async (
            string inn,
            [Microsoft.AspNetCore.Mvc.FromServices] ISparkApiClient? sparkApi,
            [Microsoft.AspNetCore.Mvc.FromServices] IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            if (string.IsNullOrWhiteSpace(inn) || inn.Length < MinInnLength)
                return Results.BadRequest(new { error = "ИНН должен содержать минимум 10 цифр" });

            await using var ctx = await dbFactory.CreateDbContextAsync();

            // 1. Проверяем кэш ext_spark_company
            var cached = await ctx.ExtSparkCompanies.FirstOrDefaultAsync(c => c.Inn == inn);
            if (cached is not null)
            {
                return Results.Ok(new
                {
                    inn = cached.Inn,
                    fullName = cached.FullName,
                    shortName = cached.ShortName,
                    okopfCode = cached.OkopfCode,
                    okopfName = cached.OkopfName,
                    status = cached.Status,
                    address = cached.LegalAddress
                });
            }

            // 2. Проверяем реестр ЦБ (регистраторы PT, информационные агентства IA)
            var cbrOrg = await ctx.ExtCbrFinOrgOrganizations.FirstOrDefaultAsync(o => o.Inn == inn);
            if (cbrOrg is not null)
            {
                return Results.Ok(new
                {
                    inn = cbrOrg.Inn,
                    fullName = cbrOrg.FullName,
                    shortName = cbrOrg.ShortName,
                    okopfCode = (string?)null,
                    okopfName = cbrOrg.FoTypes,
                    status = cbrOrg.Status,
                    address = cbrOrg.Address
                });
            }

            // 3. Кэша нет — вызываем СПАРК API (если настроен)
            if (sparkApi is null)
                return Results.Ok(new { warning = "Компания не найдена в кэше. СПАРК API не настроен." });

            try
            {
                var company = await sparkApi.GetCompanyByInnAsync(inn);
                if (company is null)
                    return Results.Ok(new { warning = "Компания не найдена в СПАРК" });

                // Сохраняем в кэш
                ctx.ExtSparkCompanies.Add(new ExtSparkCompany
                {
                    Id = Guid.NewGuid(),
                    Inn = company.Inn,
                    Ogrn = company.Ogrn,
                    FullName = company.FullName,
                    ShortName = company.ShortName,
                    OkopfCode = company.OkopfCode,
                    OkopfName = company.OkopfName,
                    LegalAddress = company.LegalAddress,
                    Status = company.IsActing ? "Действующее" : "Не действующее",
                    RegistrationDate = company.RegistrationDate,
                    FetchedAt = DateTime.UtcNow
                });
                await ctx.SaveChangesAsync();

                return Results.Ok(new
                {
                    inn = company.Inn,
                    fullName = company.FullName,
                    shortName = company.ShortName,
                    okopfCode = company.OkopfCode,
                    okopfName = company.OkopfName,
                    status = company.IsActing ? "Действующее" : "Не действующее",
                    address = company.LegalAddress
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { warning = $"Ошибка СПАРК: {ex.Message}", results = Array.Empty<object>() });
            }
        });

        // GET: проверка статуса организации в ЦБ РФ
        aoContractors.MapGet("/check-cbr", async (
            string inn,
            [Microsoft.AspNetCore.Mvc.FromServices] ICbrFinOrgDataService? cbrDataService) =>
        {
            if (string.IsNullOrWhiteSpace(inn) || inn.Length < MinInnLength)
                return Results.BadRequest(new { error = "ИНН должен содержать минимум 10 цифр" });

            if (cbrDataService is null)
                return Results.Ok(new { warning = "ЦБ РФ сервис не настроен" });

            try
            {
                if (!long.TryParse(inn, out var innLong))
                    return Results.BadRequest(new { error = "Невалидный ИНН" });

                var org = await cbrDataService.GetOrganizationByInnAsync(innLong);
                if (org is null)
                    return Results.Ok(new { warning = "Организация не найдена в реестре ЦБ РФ" });

                return Results.Ok(new
                {
                    inn = org.Inn,
                    name = org.Name,
                    shortName = org.ShortName,
                    status = org.Status,
                    foTypes = org.FoTypes,
                    region = org.Region,
                    regNumber = org.RegNumber,
                    licenses = org.Licenses.Select(l => new
                    {
                        l.VidId,
                        l.ActivityName,
                        l.Number,
                        l.Name,
                        StartDate = l.StartDate?.ToString("dd.MM.yyyy"),
                        EndDate = l.EndDate?.ToString("dd.MM.yyyy")
                    })
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new { warning = $"Ошибка ЦБ РФ: {ex.Message}" });
            }
        });

        // GET: диагностика контрагента по ИНН (СПАРК + ЦБР)
        aoContractors.MapGet("/diagnose", async (
            string inn,
            [Microsoft.AspNetCore.Mvc.FromServices] ISparkApiClient? sparkApi,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            [Microsoft.AspNetCore.Mvc.FromServices] ILoggerFactory loggerFactory) =>
        {
            if (string.IsNullOrWhiteSpace(inn) || inn.Length < MinInnLength)
                return Results.BadRequest(new { error = "ИНН должен содержать минимум 10 цифр" });

            var logger = loggerFactory.CreateLogger("AoContractors.Diagnose");

            await using var ctx = await dbFactory.CreateDbContextAsync();

            // 1. Ищем в кэше СПАРК
            var spark = await ctx.ExtSparkCompanies.FirstOrDefaultAsync(c => c.Inn == inn);

            // 2. Ищем в кэше ЦБР
            var cbrOrg = await ctx.ExtCbrFinOrgOrganizations.FirstOrDefaultAsync(o => o.Inn == inn);
            var cbrLicenses = await ctx.ExtCbrFinOrgLicenses
                .Where(l => l.OrganizationInn == inn)
                .ToListAsync();

            // 3. Если нет в кэше — вызываем СПАРК API
            if (spark is null && cbrOrg is null && sparkApi is not null)
            {
                try
                {
                    var company = await sparkApi.GetCompanyByInnAsync(inn);
                    if (company is not null)
                    {
                        spark = new ExtSparkCompany
                        {
                            Id = Guid.NewGuid(),
                            Inn = company.Inn,
                            Ogrn = company.Ogrn,
                            FullName = company.FullName,
                            ShortName = company.ShortName,
                            OkopfCode = company.OkopfCode,
                            OkopfName = company.OkopfName,
                            LegalAddress = company.LegalAddress,
                            Status = company.IsActing ? "Действующее" : "Не действующее",
                            RegistrationDate = company.RegistrationDate,
                            FetchedAt = DateTime.UtcNow
                        };
                        ctx.ExtSparkCompanies.Add(spark);
                        await ctx.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "СПАРК недоступен для ИНН={Inn}, продолжаем с кэшем", inn);
                }
            }

            // 4. Определяем тип по лицензиям ЦБ
            var isRegistrar = cbrLicenses.Any(l => l.VidId == CbrVidIdRegistrar);
            var isInfoAgency = cbrOrg?.FoTypes?.Contains(CbrFoTypeInfoAgency) == true ||
                               cbrLicenses.Any(l => l.VidId == CbrVidIdInfoAgency);

            // 5. Собираем предупреждения
            var warnings = new List<string>();
            if (spark is null && cbrOrg is null)
                warnings.Add("Компания не найдена ни в СПАРК, ни в реестре ЦБ РФ");
            else if (cbrOrg is null)
                warnings.Add("Организация не найдена в реестре ЦБ РФ (нет данных о лицензиях)");
            else if (!isRegistrar && !isInfoAgency)
                warnings.Add("Организация не является регистратором (VidID=4) и не является информационным агентством (FoType=IA / VidID=52)");

            return Results.Ok(new
            {
                inn,
                companyName = spark?.ShortName ?? spark?.FullName ?? cbrOrg?.ShortName ?? cbrOrg?.FullName,
                fullName = spark?.FullName ?? cbrOrg?.FullName,
                status = spark?.Status ?? cbrOrg?.Status,
                okopfCode = spark?.OkopfCode,
                okopfName = spark?.OkopfName,
                isRegistrar,
                isInfoAgency,
                cbrStatus = cbrOrg?.Status,
                licenses = cbrLicenses.Select(l => new
                {
                    l.VidId,
                    l.ActivityName,
                    l.Number,
                    l.Name,
                    StartDate = l.StartDate?.ToString("dd.MM.yyyy"),
                    EndDate = l.EndDate?.ToString("dd.MM.yyyy")
                }),
                warnings
            });
        });

        // GET: один контрагент по ID
        aoContractors.MapGet("/{id}", async (Guid id, IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var c = await ctx.AoContractors.FindAsync(id);
            if (c is null) return Results.NotFound();

            var org = await ctx.ExtCbrFinOrgOrganizations.FirstOrDefaultAsync(o => o.Inn == c.ContractorInn);
            var licenses = await ctx.ExtCbrFinOrgLicenses.Where(l => l.OrganizationInn == c.ContractorInn).ToListAsync();

            return Results.Ok(new
            {
                c.Id,
                c.ContractorInn,
                c.ContractorName,
                ContractorType = c.ContractorType.ToString(),
                c.ContractNumber,
                ContractDate = c.ContractDate?.ToString("dd.MM.yyyy"),
                ContractValidFrom = c.ContractValidFrom?.ToString("dd.MM.yyyy"),
                ContractValidTo = c.ContractValidTo?.ToString("dd.MM.yyyy"),
                c.IsIndefinite,
                c.ContractDocumentId,
                c.RegistryPreparationDays,
                RegistryPreparationUnit = c.RegistryPreparationUnit?.ToString(),
                c.DividendRegistryPreparationDays,
                DividendRegistryPreparationUnit = c.DividendRegistryPreparationUnit?.ToString(),
                c.RegistryRulesUrl,
                c.RegistryRulesDocumentId,
                c.IsActive,
                CbrStatus = org?.Status,
                Licenses = licenses.Select(l => new { l.VidId, l.ActivityName, l.Number, l.Name, StartDate = l.StartDate?.ToString("dd.MM.yyyy"), EndDate = l.EndDate?.ToString("dd.MM.yyyy") })
            });
        });

        // GET: список контрагентов текущего ЮЛ по типу
        aoContractors.MapGet("/", async (
            string? contractorType,
            IApplicationDbContext db,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
            var leId = workplace?.LastSelectedLegalEntityId;
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(Array.Empty<object>());

            var query = ctx.AoContractors
                .Where(c => c.LegalEntityId == leId.Value);

            if (!string.IsNullOrEmpty(contractorType) &&
                Enum.TryParse<SamorodinkaTech.Fiducia.Domain.Enums.AoContractorType>(contractorType, true, out var ct))
            {
                query = query.Where(c => c.ContractorType == ct);
            }

            var contractors = await query
                .OrderByDescending(c => c.IsActive)
                .ThenBy(c => c.ContractorName)
                .ToListAsync();

            // Подтягиваем данные ЦБ для каждого контрагента
            var inns = contractors.Select(c => c.ContractorInn).Distinct().ToList();
            var cbrOrgs = await ctx.ExtCbrFinOrgOrganizations
                .Where(o => inns.Contains(o.Inn))
                .ToDictionaryAsync(o => o.Inn);

            var cbrLicenses = await ctx.ExtCbrFinOrgLicenses
                .Where(l => inns.Contains(l.OrganizationInn))
                .ToListAsync();

            var result = contractors.Select(c =>
            {
                cbrOrgs.TryGetValue(c.ContractorInn, out var org);
                var licenses = cbrLicenses.Where(l => l.OrganizationInn == c.ContractorInn).ToList();
                return new
                {
                    c.Id,
                    c.ContractorInn,
                    c.ContractorName,
                    ContractorType = c.ContractorType.ToString(),
                    c.ContractNumber,
                    ContractDate = c.ContractDate?.ToString("dd.MM.yyyy"),
                    ContractValidFrom = c.ContractValidFrom?.ToString("dd.MM.yyyy"),
                    ContractValidTo = c.ContractValidTo?.ToString("dd.MM.yyyy"),
                    c.IsIndefinite,
                    c.ContractDocumentId,
                    c.RegistryPreparationDays,
                    RegistryPreparationUnit = c.RegistryPreparationUnit?.ToString(),
                    c.DividendRegistryPreparationDays,
                    DividendRegistryPreparationUnit = c.DividendRegistryPreparationUnit?.ToString(),
                    c.RegistryRulesUrl,
                    c.RegistryRulesDocumentId,
                    c.IsActive,
                    CbrStatus = org?.Status,
                    CbrFetchedAt = org?.FetchedAt,
                    Licenses = licenses.Select(l => new
                    {
                        l.VidId,
                        l.ActivityName,
                        l.Number,
                        l.Name,
                        StartDate = l.StartDate?.ToString("dd.MM.yyyy"),
                        EndDate = l.EndDate?.ToString("dd.MM.yyyy")
                    })
                };
            });

            return Results.Ok(result);
        });

        // POST: создание нового контрагента/договора
        aoContractors.MapPost("/", async (HttpContext http, IApplicationDbContext db) =>
        {
            if (!HasRole(http.User, "LAWYER"))
                return Results.Forbid();

            var form = await http.Request.ReadFormAsync();
            var contractorInn = form["contractorInn"].FirstOrDefault();
            var contractorName = form["contractorName"].FirstOrDefault();
            var contractorTypeStr = form["contractorType"].FirstOrDefault();
            var contractNumber = form["contractNumber"].FirstOrDefault();
            var contractDateStr = form["contractDate"].FirstOrDefault();
            var contractValidFromStr = form["contractValidFrom"].FirstOrDefault();
            var contractValidToStr = form["contractValidTo"].FirstOrDefault();
            var isIndefiniteStr = form["isIndefinite"].FirstOrDefault();
            var registryPrepDaysStr = form["registryPreparationDays"].FirstOrDefault();
            var registryPrepUnitStr = form["registryPreparationUnit"].FirstOrDefault();
            var dividendRegPrepDaysStr = form["dividendRegistryPreparationDays"].FirstOrDefault();
            var dividendRegPrepUnitStr = form["dividendRegistryPreparationUnit"].FirstOrDefault();
            var registryRulesUrl = form["registryRulesUrl"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(contractorInn) || string.IsNullOrWhiteSpace(contractorName) ||
                string.IsNullOrWhiteSpace(contractorTypeStr))
                return Results.BadRequest(new { error = "contractorInn, contractorName, contractorType are required" });

            if (!Enum.TryParse<SamorodinkaTech.Fiducia.Domain.Enums.AoContractorType>(contractorTypeStr, true, out var contractorType))
                return Results.BadRequest(new { error = "Invalid contractorType" });

            // Получаем текущее ЮЛ
            await using var ctx = ((FiduciaDbContext)db);
            var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
            var leId = workplace?.LastSelectedLegalEntityId;
            if (leId is null || leId == Guid.Empty)
                return Results.BadRequest(new { error = "No legal entity selected" });

            // Обработка загруженных файлов
            var uploadService = http.RequestServices.GetRequiredService<IChunkedUploadService>();

            Guid? documentId = null;
            var contractFile = form.Files.FirstOrDefault(f => f.Name == "file");
            if (contractFile is not null && contractFile.Length > 0)
            {
                var uploadId = await uploadService.InitiateUploadAsync(contractFile.FileName, contractFile.ContentType, contractFile.Length);
                await using var stream = contractFile.OpenReadStream();
                await uploadService.UploadChunkAsync(uploadId, 0, stream);
                var fileEntry = await uploadService.CompleteUploadAsync(uploadId);
                documentId = fileEntry.Id;
            }

            Guid? rulesDocId = null;
            var rulesFile = form.Files.FirstOrDefault(f => f.Name == "rulesFile");
            if (rulesFile is not null && rulesFile.Length > 0)
            {
                var uploadId = await uploadService.InitiateUploadAsync(rulesFile.FileName, rulesFile.ContentType, rulesFile.Length);
                await using var stream = rulesFile.OpenReadStream();
                await uploadService.UploadChunkAsync(uploadId, 0, stream);
                var fileEntry = await uploadService.CompleteUploadAsync(uploadId);
                rulesDocId = fileEntry.Id;
            }

            var contractor = new AoContractor
            {
                Id = Guid.NewGuid(),
                LegalEntityId = leId.Value,
                ContractorInn = contractorInn,
                ContractorName = contractorName,
                ContractorType = contractorType,
                ContractNumber = contractNumber,
                ContractDate = DateOnly.TryParse(contractDateStr, out var cd) ? cd : null,
                ContractValidFrom = DateOnly.TryParse(contractValidFromStr, out var cvf) ? cvf : null,
                ContractValidTo = DateOnly.TryParse(contractValidToStr, out var cvt) ? cvt : null,
                IsIndefinite = !bool.TryParse(isIndefiniteStr, out var ii) || ii,
                ContractDocumentId = documentId,
                RegistryPreparationDays = int.TryParse(registryPrepDaysStr, out var rpd) ? rpd : null,
                RegistryPreparationUnit = Enum.TryParse<SamorodinkaTech.Fiducia.Domain.Enums.MeasurementUnit>(registryPrepUnitStr, true, out var rpu) ? rpu : null,
                DividendRegistryPreparationDays = int.TryParse(dividendRegPrepDaysStr, out var drpd) ? drpd : null,
                DividendRegistryPreparationUnit = Enum.TryParse<SamorodinkaTech.Fiducia.Domain.Enums.MeasurementUnit>(dividendRegPrepUnitStr, true, out var drpu) ? drpu : null,
                RegistryRulesUrl = registryRulesUrl,
                RegistryRulesDocumentId = rulesDocId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = null // TODO: извлечь из JWT
            };

            ctx.AoContractors.Add(contractor);
            await ctx.SaveChangesAsync();

            return Results.Ok(new { contractor.Id });
        });

        // PUT: редактирование договора
        aoContractors.MapPut("/{id}", async (Guid id, HttpContext http, IApplicationDbContext db) =>
        {
            if (!HasRole(http.User, "LAWYER"))
                return Results.Forbid();

            var form = await http.Request.ReadFormAsync();
            await using var ctx = ((FiduciaDbContext)db);
            var contractor = await ctx.AoContractors.FindAsync(id);
            if (contractor is null) return Results.NotFound();

            var contractNumber = form["contractNumber"].FirstOrDefault();
            var contractDateStr = form["contractDate"].FirstOrDefault();
            var contractValidFromStr = form["contractValidFrom"].FirstOrDefault();
            var contractValidToStr = form["contractValidTo"].FirstOrDefault();
            var isIndefiniteStr = form["isIndefinite"].FirstOrDefault();
            var registryPrepDaysStr = form["registryPreparationDays"].FirstOrDefault();
            var registryPrepUnitStr = form["registryPreparationUnit"].FirstOrDefault();
            var dividendRegPrepDaysStr = form["dividendRegistryPreparationDays"].FirstOrDefault();
            var dividendRegPrepUnitStr = form["dividendRegistryPreparationUnit"].FirstOrDefault();
            var registryRulesUrl = form["registryRulesUrl"].FirstOrDefault();

            contractor.ContractNumber = contractNumber;
            contractor.ContractDate = DateOnly.TryParse(contractDateStr, out var cd) ? cd : null;
            contractor.ContractValidFrom = DateOnly.TryParse(contractValidFromStr, out var cvf) ? cvf : null;
            contractor.ContractValidTo = DateOnly.TryParse(contractValidToStr, out var cvt) ? cvt : null;
            contractor.IsIndefinite = !bool.TryParse(isIndefiniteStr, out var ii) || ii;
            contractor.RegistryPreparationDays = int.TryParse(registryPrepDaysStr, out var rpd) ? rpd : null;
            contractor.RegistryPreparationUnit = Enum.TryParse<SamorodinkaTech.Fiducia.Domain.Enums.MeasurementUnit>(registryPrepUnitStr, true, out var rpu) ? rpu : null;
            contractor.DividendRegistryPreparationDays = int.TryParse(dividendRegPrepDaysStr, out var drpd) ? drpd : null;
            contractor.DividendRegistryPreparationUnit = Enum.TryParse<SamorodinkaTech.Fiducia.Domain.Enums.MeasurementUnit>(dividendRegPrepUnitStr, true, out var drpu) ? drpu : null;
            contractor.RegistryRulesUrl = registryRulesUrl;

            // Обработка загруженных файлов (замена)
            var uploadService = http.RequestServices.GetRequiredService<IChunkedUploadService>();

            var contractFile = form.Files.FirstOrDefault(f => f.Name == "file");
            if (contractFile is not null && contractFile.Length > 0)
            {
                var uploadId = await uploadService.InitiateUploadAsync(contractFile.FileName, contractFile.ContentType, contractFile.Length);
                await using var stream = contractFile.OpenReadStream();
                await uploadService.UploadChunkAsync(uploadId, 0, stream);
                var fileEntry = await uploadService.CompleteUploadAsync(uploadId);
                contractor.ContractDocumentId = fileEntry.Id;
            }

            var rulesFile = form.Files.FirstOrDefault(f => f.Name == "rulesFile");
            if (rulesFile is not null && rulesFile.Length > 0)
            {
                var uploadId = await uploadService.InitiateUploadAsync(rulesFile.FileName, rulesFile.ContentType, rulesFile.Length);
                await using var stream = rulesFile.OpenReadStream();
                await uploadService.UploadChunkAsync(uploadId, 0, stream);
                var fileEntry = await uploadService.CompleteUploadAsync(uploadId);
                contractor.RegistryRulesDocumentId = fileEntry.Id;
            }

            await ctx.SaveChangesAsync();
            return Results.Ok();
        });

        // DELETE: деактивация (soft delete)
        aoContractors.MapDelete("/{id}", async (Guid id, HttpContext http, IApplicationDbContext db) =>
        {
            if (!HasRole(http.User, "LAWYER"))
                return Results.Forbid();

            await using var ctx = ((FiduciaDbContext)db);
            var contractor = await ctx.AoContractors.FindAsync(id);
            if (contractor is null) return Results.NotFound();

            contractor.IsActive = false;
            await ctx.SaveChangesAsync();
            return Results.Ok();
        });
    }

    /// <summary>
    /// Проверяет, содержит ли пользователь указанную роль (включая составные роли через запятую).
    /// </summary>
    private static bool HasRole(System.Security.Claims.ClaimsPrincipal user, string role)
    {
        var roleClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
        return roleClaim.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(r => r.Trim().Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}
