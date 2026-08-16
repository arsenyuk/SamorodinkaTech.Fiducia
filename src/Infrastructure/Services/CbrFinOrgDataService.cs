using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Сервис кэширования данных ЦБ РФ (FinOrg) в БД.
/// TTL кэша настраивается через CbrFinOrgOptions:CacheTtlHours (по умолчанию 12 часов).
/// При устаревании — автоматический рефреш через API.
/// </summary>
public class CbrFinOrgDataService : ICbrFinOrgDataService
{
    private readonly ICbrFinOrgClient _cbrClient;
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly ILogger<CbrFinOrgDataService> _logger;
    private readonly TimeSpan _cacheTtl;

    public CbrFinOrgDataService(
        ICbrFinOrgClient cbrClient,
        IDbContextFactory<FiduciaDbContext> dbFactory,
        ILogger<CbrFinOrgDataService> logger,
        IOptions<CbrFinOrgOptions> options)
    {
        _cbrClient = cbrClient ?? throw new ArgumentNullException(nameof(cbrClient));
        _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheTtl = TimeSpan.FromHours(options?.Value?.CacheTtlHours ?? CbrFinOrgOptions.DefaultCacheTtlHours);
    }

    /// <inheritdoc />
    public async Task<CbrFinOrgOrganization?> GetOrganizationByInnAsync(
        long inn,
        CancellationToken cancellationToken = default)
    {
        var innStr = inn.ToString();

        // 1. Проверить кэш
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var cached = await ctx.ExtCbrFinOrgOrganizations
            .FirstOrDefaultAsync(o => o.Inn == innStr, cancellationToken);

        if (cached is not null && DateTime.UtcNow - cached.FetchedAt < _cacheTtl)
        {
            _logger.LogDebug("Кэш ЦБ РФ актуален для ИНН={Inn} (fetched_at={FetchedAt:O})",
                inn, cached.FetchedAt);

            var cachedLicenses = await ctx.ExtCbrFinOrgLicenses
                .Where(l => l.OrganizationInn == innStr)
                .ToListAsync(cancellationToken);

            return MapToDto(cached, cachedLicenses);
        }

        // 2. Кэш устарел или отсутствует — вызвать API
        _logger.LogDebug("Кэш ЦБ РФ устарел или отсутствует для ИНН={Inn}, запрос API", inn);

        var org = await _cbrClient.GetOrganizationByInnAsync(inn, cancellationToken);
        if (org is null)
        {
            _logger.LogDebug("ЦБ РФ: организация с ИНН={Inn} не найдена", inn);
            return null;
        }

        // 3. Сохранить/обновить в БД
        await UpsertOrganizationAsync(ctx, org, innStr, cancellationToken);
        await ReplaceLicensesAsync(ctx, innStr, org.Licenses, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Данные ЦБ РФ для ИНН={Inn} сохранены в кэш ({LicenseCount} лицензий)",
            inn, org.Licenses.Count);

        return org;
    }

    // ── Внутренние методы ─────────────────────────────────────────

    private static async Task UpsertOrganizationAsync(
        FiduciaDbContext ctx,
        CbrFinOrgOrganization org,
        string inn,
        CancellationToken ct)
    {
        var existing = await ctx.ExtCbrFinOrgOrganizations
            .FirstOrDefaultAsync(o => o.Inn == inn, ct);

        if (existing is null)
        {
            ctx.ExtCbrFinOrgOrganizations.Add(new ExtCbrFinOrgOrganization
            {
                Id = Guid.NewGuid(),
                Inn = inn,
                CbrId = org.Id,
                Ogrn = org.Ogrn?.ToString(),
                FullName = org.Name,
                ShortName = org.ShortName,
                EngName = org.EngName,
                Address = org.Address,
                Phones = org.Phones,
                Email = org.Email,
                Okato = org.Okato,
                Region = org.Region,
                FoTypes = org.FoTypes.Count > 0 ? string.Join(",", org.FoTypes) : null,
                Status = org.Status,
                IsSroMember = org.IsSroMember,
                IsRss = org.IsRss,
                IsNpo = org.IsNpo,
                IsAsv = org.IsAsv,
                RegNumber = org.RegNumber,
                Bic = org.Bic,
                BankStatus = org.BankStatus,
                RegistrationDate = org.RegistrationDate,
                HasBranches = org.HasBranches,
                FundValue = org.Fund?.FundValue,
                WebSites = org.WebSites.Count > 0 ? string.Join(",", org.WebSites) : null,
                Error = org.Error,
                FetchedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.CbrId = org.Id;
            existing.Ogrn = org.Ogrn?.ToString();
            existing.FullName = org.Name;
            existing.ShortName = org.ShortName;
            existing.EngName = org.EngName;
            existing.Address = org.Address;
            existing.Phones = org.Phones;
            existing.Email = org.Email;
            existing.Okato = org.Okato;
            existing.Region = org.Region;
            existing.FoTypes = org.FoTypes.Count > 0 ? string.Join(",", org.FoTypes) : null;
            existing.Status = org.Status;
            existing.IsSroMember = org.IsSroMember;
            existing.IsRss = org.IsRss;
            existing.IsNpo = org.IsNpo;
            existing.IsAsv = org.IsAsv;
            existing.RegNumber = org.RegNumber;
            existing.Bic = org.Bic;
            existing.BankStatus = org.BankStatus;
            existing.RegistrationDate = org.RegistrationDate;
            existing.HasBranches = org.HasBranches;
            existing.FundValue = org.Fund?.FundValue;
            existing.WebSites = org.WebSites.Count > 0 ? string.Join(",", org.WebSites) : null;
            existing.Error = org.Error;
            existing.FetchedAt = DateTime.UtcNow;
        }
    }

    private static async Task ReplaceLicensesAsync(
        FiduciaDbContext ctx,
        string inn,
        List<CbrFinOrgLicense> licenses,
        CancellationToken ct)
    {
        // Удалить все существующие лицензии для данного ИНН
        var existing = await ctx.ExtCbrFinOrgLicenses
            .Where(l => l.OrganizationInn == inn)
            .ToListAsync(ct);

        ctx.ExtCbrFinOrgLicenses.RemoveRange(existing);

        // Вставить свежие
        var now = DateTime.UtcNow;
        foreach (var lic in licenses)
        {
            ctx.ExtCbrFinOrgLicenses.Add(new ExtCbrFinOrgLicense
            {
                Id = Guid.NewGuid(),
                OrganizationInn = inn,
                VidId = lic.VidId,
                ActivityName = lic.ActivityName,
                Number = lic.Number,
                Name = lic.Name,
                StartDate = lic.StartDate,
                EndDate = lic.EndDate,
                FetchedAt = now
            });
        }
    }

    private static CbrFinOrgOrganization MapToDto(
        ExtCbrFinOrgOrganization entity,
        List<ExtCbrFinOrgLicense> licenses)
    {
        return new CbrFinOrgOrganization
        {
            Id = entity.CbrId ?? 0,
            Ogrn = long.TryParse(entity.Ogrn, out var ogrn) ? ogrn : null,
            Inn = entity.Inn,
            Name = entity.FullName,
            ShortName = entity.ShortName,
            EngName = entity.EngName,
            Address = entity.Address,
            Phones = entity.Phones,
            Email = entity.Email,
            Okato = entity.Okato,
            Region = entity.Region,
            FoTypes = string.IsNullOrEmpty(entity.FoTypes)
                ? new List<string>()
                : entity.FoTypes.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            Status = entity.Status,
            IsSroMember = entity.IsSroMember,
            IsRss = entity.IsRss,
            IsNpo = entity.IsNpo,
            IsAsv = entity.IsAsv,
            RegNumber = entity.RegNumber,
            Bic = entity.Bic,
            BankStatus = entity.BankStatus,
            RegistrationDate = entity.RegistrationDate,
            HasBranches = entity.HasBranches,
            Fund = entity.FundValue.HasValue
                ? new CbrFinOrgFundInfo { FundValue = entity.FundValue.Value }
                : null,
            Licenses = licenses.Select(l => new CbrFinOrgLicense
            {
                VidId = l.VidId,
                ActivityName = l.ActivityName,
                Number = l.Number,
                Name = l.Name,
                StartDate = l.StartDate,
                EndDate = l.EndDate
            }).ToList(),
            WebSites = string.IsNullOrEmpty(entity.WebSites)
                ? new List<string>()
                : entity.WebSites.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            Error = entity.Error
        };
    }
}
