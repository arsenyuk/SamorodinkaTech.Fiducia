using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Validation;
using SamorodinkaTech.Fiducia.Infrastructure.FileStorage;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Minimal API endpoints для договоров (единая таблица contracts).
/// Типы: REGISTRAR, INFO_AGENCY, MANAGEMENT_IP, MANAGEMENT_UL.
/// </summary>
public static class ContractEndpoints
{
    private const int MinInnLength = 10;
    private const int InnIpLength = 12;
    private const int OgrnipLength = 15;
    private const int InnUlLength = 10;
    private const int OgrnLength = 13;
    private const int CbrVidIdRegistrar = 4;
    private const int CbrVidIdInfoAgency = 52;
    private const string CbrFoTypeInfoAgency = "IA";

    /// <summary>
    /// Регистрирует все endpoint'ы группы Contracts.
    /// </summary>
    public static void MapContractEndpoints(this WebApplication app)
    {
        var contracts = app.MapGroup("/api/contracts")
            .RequireAuthorization()
            .WithTags("Contracts");

        // GET: поиск контрагента в СПАРК — для всех типов договоров
        contracts.MapGet("/search-spark", async (
            string query,
            string? contractType,
            [Microsoft.AspNetCore.Mvc.FromServices] ISparkApiClient? sparkApi,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Contracts.SearchSpark");
            if (string.IsNullOrWhiteSpace(query))
                return Results.BadRequest(new { error = "Введите ИНН или ОГРН" });

            query = query.Trim();

            // Определяем тип ввода по длине
            if (query.Length == InnIpLength)
            {
                // ИНН ИП (12 цифр) — для MANAGEMENT_IP
                var (isValid, error) = InnIpValidator.Validate(query);
                if (!isValid)
                    return Results.BadRequest(new { error });

                return await SearchIpByInnAsync(query, sparkApi, dbFactory, logger);
            }
            else if (query.Length == OgrnipLength)
            {
                // ОГРНИП (15 цифр) — для MANAGEMENT_IP
                var (isValid, error) = OgrnipValidator.Validate(query);
                if (!isValid)
                    return Results.BadRequest(new { error });

                return await SearchByOgrnipAsync(query, dbFactory, logger);
            }
            else if (query.Length == InnUlLength)
            {
                // ИНН ЮЛ (10 цифр) — для REGISTRAR, INFO_AGENCY, MANAGEMENT_UL
                return await SearchUlByInnAsync(query, sparkApi, dbFactory, logger);
            }
            else if (query.Length == OgrnLength)
            {
                // ОГРН (13 цифр) — для MANAGEMENT_UL
                return await SearchByOgrnAsync(query, dbFactory, logger);
            }
            else
            {
                return Results.BadRequest(new { error = "ИНН ИП: 12 цифр, ОГРНИП: 15 цифр, ИНН ЮЛ: 10 цифр, ОГРН: 13 цифр" });
            }
        });

        // GET: проверка статуса организации в ЦБ РФ (для REGISTRAR/INFO_AGENCY)
        contracts.MapGet("/check-cbr", async (
            string inn,
            [Microsoft.AspNetCore.Mvc.FromServices] ICbrFinOrgDataService? cbrDataService,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Contracts.CheckCbr");
            if (string.IsNullOrWhiteSpace(inn) || inn.Length < MinInnLength)
            {
                logger.LogWarning("Проверка ЦБ: ИНН не содержит минимум 10 цифр: {Inn}", inn);
                return Results.BadRequest(new { error = "ИНН должен содержать минимум 10 цифр" });
            }

            if (cbrDataService is null)
                return Results.Ok(new { warning = "ЦБ РФ сервис не настроен" });

            try
            {
                if (!long.TryParse(inn, out var innLong))
                {
                    logger.LogWarning("Проверка ЦБ: невалидный ИНН: {Inn}", inn);
                    return Results.BadRequest(new { error = "Невалидный ИНН" });
                }

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

        // GET: диагностика контрагента по ИНН (СПАРК + ЦБР) — для REGISTRAR/INFO_AGENCY
        contracts.MapGet("/diagnose", async (
            string inn,
            [Microsoft.AspNetCore.Mvc.FromServices] ISparkApiClient? sparkApi,
            IDbContextFactory<FiduciaDbContext> dbFactory,
            ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Contracts.Diagnose");
            if (string.IsNullOrWhiteSpace(inn) || inn.Length < MinInnLength)
            {
                logger.LogWarning("Диагностика контрагента: ИНН не содержит минимум 10 цифр: {Inn}", inn);
                return Results.BadRequest(new { error = "ИНН должен содержать минимум 10 цифр" });
            }

            await using var ctx = await dbFactory.CreateDbContextAsync();

            var spark = await ctx.ExtSparkCompanies.FirstOrDefaultAsync(c => c.Inn == inn);
            var cbrOrg = await ctx.ExtCbrFinOrgOrganizations.FirstOrDefaultAsync(o => o.Inn == inn);
            var cbrLicenses = await ctx.ExtCbrFinOrgLicenses
                .Where(l => l.OrganizationInn == inn)
                .ToListAsync();

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

            var isRegistrar = cbrLicenses.Any(l => l.VidId == CbrVidIdRegistrar);
            var isInfoAgency = cbrOrg?.FoTypes?.Contains(CbrFoTypeInfoAgency) == true ||
                               cbrLicenses.Any(l => l.VidId == CbrVidIdInfoAgency);

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
                webSites = cbrOrg?.WebSites,
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

        // GET: список договоров текущего ЮЛ
        contracts.MapGet("/", async (
            string? contractType,
            IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
            var leId = workplace?.LastSelectedLegalEntityId;
            if (leId is null || leId == Guid.Empty)
                return Results.Ok(Array.Empty<object>());

            var query = ctx.Contracts
                .Where(c => c.LegalEntityId == leId.Value);

            if (!string.IsNullOrEmpty(contractType) &&
                Enum.TryParse<ContractType>(contractType, true, out var ct))
            {
                query = query.Where(c => c.ContractType == ct);
            }

            var items = await query
                .OrderByDescending(c => c.IsActive)
                .ThenBy(c => c.CounterpartyName)
                .ToListAsync();

            // Подтягиваем данные ЦБ для REGISTRAR/INFO_AGENCY
            var aoInns = items
                .Where(c => c.ContractType == ContractType.REGISTRAR || c.ContractType == ContractType.INFO_AGENCY)
                .Select(c => c.CounterpartyInn)
                .Distinct()
                .ToList();
            var cbrOrgs = await ctx.ExtCbrFinOrgOrganizations
                .Where(o => aoInns.Contains(o.Inn))
                .ToDictionaryAsync(o => o.Inn);
            var cbrLicenses = await ctx.ExtCbrFinOrgLicenses
                .Where(l => aoInns.Contains(l.OrganizationInn))
                .ToListAsync();

            // Подтягиваем данные ЮЛ-управляющих для MANAGEMENT_UL
            var ulLeIds = items
                .Where(c => c.ContractType == ContractType.MANAGEMENT_UL && c.ManagerLegalEntityId.HasValue)
                .Select(c => c.ManagerLegalEntityId!.Value)
                .Distinct()
                .ToList();
            var ulEntities = await ctx.LegalEntities
                .Where(le => ulLeIds.Contains(le.Id))
                .ToDictionaryAsync(le => le.Id);

            var result = items.Select(c =>
            {
                cbrOrgs.TryGetValue(c.CounterpartyInn, out var cbrOrg);
                var licenses = cbrLicenses.Where(l => l.OrganizationInn == c.CounterpartyInn).ToList();

                string? managerFullName = null;
                string? managerInn = null;
                if (c.ContractType == ContractType.MANAGEMENT_UL && c.ManagerLegalEntityId.HasValue &&
                    ulEntities.TryGetValue(c.ManagerLegalEntityId.Value, out var ul))
                {
                    managerFullName = ul.ShortName ?? ul.Name;
                    managerInn = ul.Inn;
                }

                return new
                {
                    c.Id,
                    ContractType = c.ContractType.ToString(),
                    c.CounterpartyName,
                    c.CounterpartyInn,
                    c.ContractNumber,
                    ContractDate = c.ContractDate?.ToString("dd.MM.yyyy"),
                    ContractValidFrom = c.ContractValidFrom?.ToString("dd.MM.yyyy"),
                    ContractValidTo = c.ContractValidTo?.ToString("dd.MM.yyyy"),
                    c.IsIndefinite,
                    c.ContractDocumentId,
                    c.RegistryPreparationDays,
                    RegistryPreparationUnit = c.RegistryPreparationUnit?.Code,
                    c.DividendRegistryPreparationDays,
                    DividendRegistryPreparationUnit = c.DividendRegistryPreparationUnit?.Code,
                    c.RegistryRulesUrl,
                    c.RegistryRulesDocumentId,
                    c.ManagerOgrnip,
                    c.ManagerLegalEntityId,
                    ManagerFullName = managerFullName,
                    ManagerInn = managerInn,
                    c.IsActive,
                    CbrStatus = cbrOrg?.Status,
                    CbrFetchedAt = cbrOrg?.FetchedAt,
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

        // GET: один договор по ID
        contracts.MapGet("/{id}", async (Guid id, IDbContextFactory<FiduciaDbContext> dbFactory) =>
        {
            await using var ctx = await dbFactory.CreateDbContextAsync();
            var c = await ctx.Contracts.FindAsync(id);
            if (c is null) return Results.NotFound();

            string? managerFullName = null;
            string? managerInn = null;
            if (c.ContractType == ContractType.MANAGEMENT_UL && c.ManagerLegalEntityId.HasValue)
            {
                var ul = await ctx.LegalEntities.FindAsync(c.ManagerLegalEntityId.Value);
                managerFullName = ul?.ShortName ?? ul?.Name;
                managerInn = ul?.Inn;
            }

            return Results.Ok(new
            {
                c.Id,
                ContractType = c.ContractType.ToString(),
                c.CounterpartyName,
                c.CounterpartyInn,
                c.ContractNumber,
                ContractDate = c.ContractDate?.ToString("dd.MM.yyyy"),
                ContractValidFrom = c.ContractValidFrom?.ToString("dd.MM.yyyy"),
                ContractValidTo = c.ContractValidTo?.ToString("dd.MM.yyyy"),
                c.IsIndefinite,
                c.ContractDocumentId,
                c.RegistryPreparationDays,
                RegistryPreparationUnit = c.RegistryPreparationUnit?.Code,
                c.DividendRegistryPreparationDays,
                DividendRegistryPreparationUnit = c.DividendRegistryPreparationUnit?.Code,
                c.RegistryRulesUrl,
                c.RegistryRulesDocumentId,
                c.ManagerOgrnip,
                c.ManagerLegalEntityId,
                ManagerFullName = managerFullName,
                ManagerInn = managerInn,
                c.IsActive
            });
        });

        // POST: создание договора (все типы)
        contracts.MapPost("/", async (HttpContext http, IApplicationDbContext db,
            IChunkedUploadService uploadService, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Contracts.Create");
            var form = await http.Request.ReadFormAsync();

            var contractTypeStr = form["contractType"].FirstOrDefault();
            var counterpartyInn = form["counterpartyInn"].FirstOrDefault();
            var counterpartyName = form["counterpartyName"].FirstOrDefault();
            var contractNumber = form["contractNumber"].FirstOrDefault();
            var contractDateStr = form["contractDate"].FirstOrDefault();
            var contractValidFromStr = form["contractValidFrom"].FirstOrDefault();
            var contractValidToStr = form["contractValidTo"].FirstOrDefault();
            var isIndefiniteStr = form["isIndefinite"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(contractTypeStr) ||
                string.IsNullOrWhiteSpace(counterpartyInn) ||
                string.IsNullOrWhiteSpace(counterpartyName))
            {
                logger.LogWarning("Создание договора: обязательные поля отсутствуют");
                return Results.BadRequest(new { error = "contractType, counterpartyInn, counterpartyName are required" });
            }

            if (!Enum.TryParse<ContractType>(contractTypeStr, true, out var contractType))
            {
                logger.LogWarning("Создание договора: невалидный contractType={ContractType}", contractTypeStr);
                return Results.BadRequest(new { error = "Invalid contractType" });
            }

            // Роль: LAWYER для всех, CEO только для MANAGEMENT
            if (!HasRole(http.User, "LAWYER") &&
                !(HasRole(http.User, "CEO") && (contractType == ContractType.MANAGEMENT_IP || contractType == ContractType.MANAGEMENT_UL)))
                return Results.Forbid();

            await using var ctx = ((FiduciaDbContext)db);
            var workplace = await ctx.CurrentWorkplaces.FirstOrDefaultAsync();
            var leId = workplace?.LastSelectedLegalEntityId;
            if (leId is null || leId == Guid.Empty)
            {
                logger.LogWarning("Создание договора: юридическое лицо не выбрано");
                return Results.BadRequest(new { error = "Юридическое лицо не выбрано" });
            }

            // Загрузка файла договора
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

            // Загрузка файла правил реестра (REGISTRAR)
            Guid? rulesDocId = null;
            if (contractType == ContractType.REGISTRAR)
            {
                var rulesFile = form.Files.FirstOrDefault(f => f.Name == "rulesFile");
                if (rulesFile is not null && rulesFile.Length > 0)
                {
                    var uploadId = await uploadService.InitiateUploadAsync(rulesFile.FileName, rulesFile.ContentType, rulesFile.Length);
                    await using var stream = rulesFile.OpenReadStream();
                    await uploadService.UploadChunkAsync(uploadId, 0, stream);
                    var fileEntry = await uploadService.CompleteUploadAsync(uploadId);
                    rulesDocId = fileEntry.Id;
                }
            }

            // ОГРНИП для MANAGEMENT_IP
            string? managerOgrnip = null;
            if (contractType == ContractType.MANAGEMENT_IP)
            {
                managerOgrnip = form["managerOgrnip"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(managerOgrnip))
                {
                    var (ogrnipValid, ogrnipError) = OgrnipValidator.Validate(managerOgrnip);
                    if (!ogrnipValid)
                        return Results.BadRequest(new { error = ogrnipError });
                }
            }

            // Ссылка на ЮЛ для MANAGEMENT_UL
            Guid? managerLegalEntityId = null;
            if (contractType == ContractType.MANAGEMENT_UL)
            {
                var leStr = form["managerLegalEntityId"].FirstOrDefault();
                if (Guid.TryParse(leStr, out var leGuid))
                    managerLegalEntityId = leGuid;
            }

            var entity = new Contract
            {
                Id = Guid.NewGuid(),
                LegalEntityId = leId.Value,
                ContractType = contractType,
                CounterpartyName = counterpartyName.Trim(),
                CounterpartyInn = counterpartyInn.Trim(),
                ContractNumber = contractNumber?.Trim(),
                ContractDate = DateOnly.TryParse(contractDateStr, out var cd) ? cd : null,
                ContractValidFrom = DateOnly.TryParse(contractValidFromStr, out var cvf) ? cvf : null,
                ContractValidTo = DateOnly.TryParse(contractValidToStr, out var cvt) ? cvt : null,
                IsIndefinite = !bool.TryParse(isIndefiniteStr, out var ii) || ii,
                ContractDocumentId = documentId,
                ManagerOgrnip = managerOgrnip?.Trim(),
                ManagerLegalEntityId = managerLegalEntityId,
                RegistryPreparationDays = int.TryParse(form["registryPreparationDays"].FirstOrDefault(), out var rpd) ? rpd : null,
                RegistryPreparationUnitId = !string.IsNullOrEmpty(form["registryPreparationUnit"].FirstOrDefault())
                    ? (await ctx.RefMeasurementUnits.FirstOrDefaultAsync(x => x.Code == form["registryPreparationUnit"].FirstOrDefault()))?.Id
                    : null,
                DividendRegistryPreparationDays = int.TryParse(form["dividendRegistryPreparationDays"].FirstOrDefault(), out var drpd) ? drpd : null,
                DividendRegistryPreparationUnitId = !string.IsNullOrEmpty(form["dividendRegistryPreparationUnit"].FirstOrDefault())
                    ? (await ctx.RefMeasurementUnits.FirstOrDefaultAsync(x => x.Code == form["dividendRegistryPreparationUnit"].FirstOrDefault()))?.Id
                    : null,
                RegistryRulesUrl = form["registryRulesUrl"].FirstOrDefault(),
                RegistryRulesDocumentId = rulesDocId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(
                    http.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    out var uid) ? uid : null
            };

            ctx.Contracts.Add(entity);
            await ctx.SaveChangesAsync();

            return Results.Ok(new { entity.Id });
        });

        // PUT: обновление договора
        contracts.MapPut("/{id}", async (Guid id, HttpContext http, IApplicationDbContext db,
            IChunkedUploadService uploadService, ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("Contracts.Update");
            if (!HasRole(http.User, "LAWYER") &&
                !(HasRole(http.User, "CEO")))
                return Results.Forbid();

            var form = await http.Request.ReadFormAsync();
            await using var ctx = ((FiduciaDbContext)db);
            var entity = await ctx.Contracts.FindAsync(id);
            if (entity is null) return Results.NotFound();

            var contractNumber = form["contractNumber"].FirstOrDefault();
            var contractDateStr = form["contractDate"].FirstOrDefault();
            var contractValidFromStr = form["contractValidFrom"].FirstOrDefault();
            var contractValidToStr = form["contractValidTo"].FirstOrDefault();
            var isIndefiniteStr = form["isIndefinite"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(contractNumber))
                entity.ContractNumber = contractNumber.Trim();

            if (DateOnly.TryParse(contractDateStr, out var cd))
                entity.ContractDate = cd;

            if (DateOnly.TryParse(contractValidFromStr, out var vf))
                entity.ContractValidFrom = vf;

            entity.IsIndefinite = !bool.TryParse(isIndefiniteStr, out var ii) || ii;
            if (!entity.IsIndefinite && DateOnly.TryParse(contractValidToStr, out var vt))
                entity.ContractValidTo = vt;
            else if (entity.IsIndefinite)
                entity.ContractValidTo = null;

            // REGISTRAR: специфичные поля
            if (entity.ContractType == ContractType.REGISTRAR)
            {
                entity.RegistryPreparationDays = int.TryParse(form["registryPreparationDays"].FirstOrDefault(), out var rpd) ? rpd : null;
                entity.RegistryPreparationUnitId = !string.IsNullOrEmpty(form["registryPreparationUnit"].FirstOrDefault())
                    ? (await ctx.RefMeasurementUnits.FirstOrDefaultAsync(x => x.Code == form["registryPreparationUnit"].FirstOrDefault()))?.Id
                    : null;
                entity.DividendRegistryPreparationDays = int.TryParse(form["dividendRegistryPreparationDays"].FirstOrDefault(), out var drpd) ? drpd : null;
                entity.DividendRegistryPreparationUnitId = !string.IsNullOrEmpty(form["dividendRegistryPreparationUnit"].FirstOrDefault())
                    ? (await ctx.RefMeasurementUnits.FirstOrDefaultAsync(x => x.Code == form["dividendRegistryPreparationUnit"].FirstOrDefault()))?.Id
                    : null;
                entity.RegistryRulesUrl = form["registryRulesUrl"].FirstOrDefault();
            }

            // MANAGEMENT_IP: ОГРНИП
            if (entity.ContractType == ContractType.MANAGEMENT_IP)
            {
                var ogrnip = form["managerOgrnip"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(ogrnip))
                {
                    var (ogrnipValid, ogrnipError) = OgrnipValidator.Validate(ogrnip);
                    if (!ogrnipValid)
                        return Results.BadRequest(new { error = ogrnipError });
                    entity.ManagerOgrnip = ogrnip.Trim();
                }
            }

            // MANAGEMENT_UL: ссылка на ЮЛ
            if (entity.ContractType == ContractType.MANAGEMENT_UL)
            {
                var leStr = form["managerLegalEntityId"].FirstOrDefault();
                if (Guid.TryParse(leStr, out var leGuid))
                    entity.ManagerLegalEntityId = leGuid;
            }

            // Загрузка файлов
            var contractFile = form.Files.FirstOrDefault(f => f.Name == "file");
            if (contractFile is not null && contractFile.Length > 0)
            {
                var uploadId = await uploadService.InitiateUploadAsync(contractFile.FileName, contractFile.ContentType, contractFile.Length);
                await using var stream = contractFile.OpenReadStream();
                await uploadService.UploadChunkAsync(uploadId, 0, stream);
                var fileEntry = await uploadService.CompleteUploadAsync(uploadId);
                entity.ContractDocumentId = fileEntry.Id;
            }

            if (entity.ContractType == ContractType.REGISTRAR)
            {
                var rulesFile = form.Files.FirstOrDefault(f => f.Name == "rulesFile");
                if (rulesFile is not null && rulesFile.Length > 0)
                {
                    var uploadId = await uploadService.InitiateUploadAsync(rulesFile.FileName, rulesFile.ContentType, rulesFile.Length);
                    await using var stream = rulesFile.OpenReadStream();
                    await uploadService.UploadChunkAsync(uploadId, 0, stream);
                    var fileEntry = await uploadService.CompleteUploadAsync(uploadId);
                    entity.RegistryRulesDocumentId = fileEntry.Id;
                }
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
            var entity = await ctx.Contracts.FindAsync(id);
            if (entity is null) return Results.NotFound();

            entity.IsActive = false;
            await ctx.SaveChangesAsync();
            return Results.Ok();
        });
    }

    // ═════════════════════════════════════════════════════════════════════
    // Поиск в СПАРК — вспомогательные методы
    // ═════════════════════════════════════════════════════════════════════

    private static async Task<IResult> SearchIpByInnAsync(
        string inn, ISparkApiClient? sparkApi,
        IDbContextFactory<FiduciaDbContext> dbFactory, ILogger logger)
    {
        await using var ctx = await dbFactory.CreateDbContextAsync();

        var cached = await ctx.ExtSparkCompanies.FirstOrDefaultAsync(c => c.Inn == inn);
        if (cached is not null)
            return Results.Ok(MapSparkCache(cached));

        if (sparkApi is null)
            return Results.Ok(new { warning = "ИП не найден в кэше. СПАРК API не настроен." });

        try
        {
            var company = await sparkApi.GetCompanyByInnAsync(inn);
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
            logger.LogWarning(ex, "Ошибка СПАРК для ИНН ИП {Inn}", inn);
            return Results.Ok(new { warning = $"Ошибка СПАРК: {ex.Message}" });
        }
    }

    private static async Task<IResult> SearchByOgrnipAsync(
        string ogrnip, IDbContextFactory<FiduciaDbContext> dbFactory, ILogger logger)
    {
        await using var ctx = await dbFactory.CreateDbContextAsync();
        var cached = await ctx.ExtSparkCompanies.FirstOrDefaultAsync(c => c.Ogrn == ogrnip);
        if (cached is not null)
            return Results.Ok(MapSparkCache(cached));

        return Results.Ok(new { warning = "ИП с указанным ОГРНИП не найден в кэше. Введите ИНН для поиска в СПАРК." });
    }

    private static async Task<IResult> SearchUlByInnAsync(
        string inn, ISparkApiClient? sparkApi,
        IDbContextFactory<FiduciaDbContext> dbFactory, ILogger logger)
    {
        await using var ctx = await dbFactory.CreateDbContextAsync();

        // 1. Кэш СПАРК
        var cached = await ctx.ExtSparkCompanies.FirstOrDefaultAsync(c => c.Inn == inn);
        if (cached is not null)
            return Results.Ok(MapSparkCache(cached));

        // 2. Реестр ЦБ
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

        // 3. СПАРК API
        if (sparkApi is null)
            return Results.Ok(new { warning = "Компания не найдена в кэше. СПАРК API не настроен." });

        try
        {
            var company = await sparkApi.GetCompanyByInnAsync(inn);
            if (company is null)
                return Results.Ok(new { warning = "Компания не найдена в СПАРК" });

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
            logger.LogWarning(ex, "Ошибка СПАРК для ИНН ЮЛ {Inn}", inn);
            return Results.Ok(new { warning = $"Ошибка СПАРК: {ex.Message}" });
        }
    }

    private static async Task<IResult> SearchByOgrnAsync(
        string ogrn, IDbContextFactory<FiduciaDbContext> dbFactory, ILogger logger)
    {
        await using var ctx = await dbFactory.CreateDbContextAsync();
        var cached = await ctx.ExtSparkCompanies.FirstOrDefaultAsync(c => c.Ogrn == ogrn);
        if (cached is not null)
            return Results.Ok(MapSparkCache(cached));

        return Results.Ok(new { warning = "Организация с указанным ОГРН не найдена в кэше. Введите ИНН для поиска в СПАРК." });
    }

    private static object MapSparkCache(ExtSparkCompany cached) => new
    {
        inn = cached.Inn,
        fullName = cached.FullName,
        shortName = cached.ShortName,
        okopfCode = cached.OkopfCode,
        okopfName = cached.OkopfName,
        status = cached.Status,
        address = cached.LegalAddress,
        ogrn = cached.Ogrn,
        isActing = cached.Status == "Действующее"
    };

    private static bool HasRole(System.Security.Claims.ClaimsPrincipal user, string role)
    {
        var roleClaim = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
        return roleClaim.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Any(r => r.Trim().Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}
