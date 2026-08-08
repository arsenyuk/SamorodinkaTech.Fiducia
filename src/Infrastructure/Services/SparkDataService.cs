using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Spark;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация ISparkDataService: загрузка из СПАРК API, сохранение в кэш (ext_spark_*) и чтение кэша.
/// </summary>
public class SparkDataService : ISparkDataService
{
    private readonly ISparkApiClient _sparkApi;
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly ILogger<SparkDataService> _logger;

    public SparkDataService(
        ISparkApiClient sparkApi,
        IDbContextFactory<FiduciaDbContext> dbFactory,
        ILogger<SparkDataService> logger)
    {
        _sparkApi = sparkApi;
        _dbFactory = dbFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SparkCachedView> LoadCachedAsync(
        string inn,
        bool isLlc,
        CancellationToken cancellationToken = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var view = new SparkCachedView();

        var cachedManager = await ctx.ExtSparkManagers
            .FirstOrDefaultAsync(x => x.Inn == inn, cancellationToken);
        if (cachedManager is not null)
        {
            view = view with
            {
                ManagerName = cachedManager.FullName,
                ManagerPosition = cachedManager.Position,
                ManagerInn = cachedManager.PersonInn,
                ManagerStartDate = cachedManager.StartDate
            };
        }

        var cachedCompany = await ctx.ExtSparkCompanies
            .FirstOrDefaultAsync(x => x.Inn == inn, cancellationToken);
        if (cachedCompany is not null)
        {
            view = view with
            {
                CompanyFullName = cachedCompany.FullName,
                CompanyShortName = cachedCompany.ShortName,
                CompanyOgrn = cachedCompany.Ogrn,
                CompanyOkopfName = cachedCompany.OkopfName,
                CompanyAddress = cachedCompany.LegalAddress,
                CompanyStatus = cachedCompany.Status,
                CompanyRegDate = cachedCompany.RegistrationDate
            };
        }

        if (isLlc)
        {
            view = view with
            {
                Founders = await ctx.ExtSparkFounders
                    .Where(x => x.Inn == inn)
                    .Select(x => MapToDto(x))
                    .ToListAsync(cancellationToken)
            };
        }

        return view;
    }

    /// <inheritdoc />
    public async Task<SparkFetchResult> FetchAndSaveAsync(
        string inn,
        bool isLlc,
        Guid legalEntityId,
        CancellationToken cancellationToken = default)
    {
        var company = await _sparkApi.GetCompanyByInnAsync(inn, cancellationToken);
        var manager = await _sparkApi.GetManagerAsync(inn, cancellationToken);

        List<SparkFounder> founders = new();
        string? warning = null;

        if (isLlc)
        {
            try
            {
                founders = await _sparkApi.GetFoundersAsync(inn, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Endpoint учредителей СПАРК вернул ошибку (ИНН={Inn}): {Error}", inn, ex.Message);
                warning = ex.Message;
            }
        }

        await using var ctx = await _dbFactory.CreateDbContextAsync(cancellationToken);

        string? okopfCode = null;

        if (company is not null)
        {
            UpsertCompany(ctx, inn, company);
            okopfCode = await UpdateOkopfIfFound(ctx, company, legalEntityId, cancellationToken);
        }

        if (manager is not null)
            UpsertManager(ctx, inn, manager);
        else
            await FillManagerFromCache(ctx, inn, cancellationToken);

        if (founders.Count > 0)
            ReplaceFounders(ctx, inn, founders);

        await ctx.SaveChangesAsync(cancellationToken);

        return new SparkFetchResult
        {
            Company = company,
            Manager = manager,
            Founders = founders,
            Warning = warning,
            OkopfCode = okopfCode
        };
    }

    // ── Приватные методы сохранения ───────────────────────────────

    private static void UpsertCompany(FiduciaDbContext ctx, string inn, SparkCompany company)
    {
        var existing = ctx.ExtSparkCompanies.FirstOrDefault(x => x.Inn == inn);
        if (existing is null)
        {
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
                Status = company.Status,
                RegistrationDate = company.RegistrationDate,
                FetchedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.Ogrn = company.Ogrn;
            existing.FullName = company.FullName;
            existing.ShortName = company.ShortName;
            existing.OkopfCode = company.OkopfCode;
            existing.OkopfName = company.OkopfName;
            existing.LegalAddress = company.LegalAddress;
            existing.Status = company.Status;
            existing.RegistrationDate = company.RegistrationDate;
            existing.FetchedAt = DateTime.UtcNow;
        }
    }

    private static void UpsertManager(FiduciaDbContext ctx, string inn, SparkManager manager)
    {
        var existing = ctx.ExtSparkManagers.FirstOrDefault(x => x.Inn == inn);
        if (existing is null)
        {
            ctx.ExtSparkManagers.Add(new ExtSparkManager
            {
                Id = Guid.NewGuid(),
                Inn = inn,
                FullName = manager.FullName,
                Position = manager.Position,
                PersonInn = manager.Inn,
                StartDate = manager.ActualDate,
                FetchedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.FullName = manager.FullName;
            existing.Position = manager.Position;
            existing.PersonInn = manager.Inn;
            existing.StartDate = manager.ActualDate;
            existing.FetchedAt = DateTime.UtcNow;
        }
    }

    private static void ReplaceFounders(FiduciaDbContext ctx, string inn, List<SparkFounder> founders)
    {
        var existing = ctx.ExtSparkFounders.Where(x => x.Inn == inn).ToList();
        ctx.ExtSparkFounders.RemoveRange(existing);

        foreach (var f in founders)
        {
            ctx.ExtSparkFounders.Add(new ExtSparkFounder
            {
                Id = Guid.NewGuid(),
                Inn = inn,
                Name = f.Name,
                FounderInn = f.Inn,
                FounderOgrn = f.Ogrn,
                Country = f.Country,
                IsForeign = f.IsForeign,
                FullName = f.FullName,
                PersonInn = f.PersonInn,
                Citizenship = f.Citizenship,
                HeadOfOther = f.HeadOfOther,
                FounderOfOther = f.FounderOfOther,
                IsEntrepreneur = f.IsEntrepreneur,
                Ogrnip = f.Ogrnip,
                ShareAmount = f.ShareAmount,
                SharePercent = f.SharePercent,
                EntryDate = f.EntryDate,
                ExitDate = f.ExitDate,
                FetchedAt = DateTime.UtcNow
            });
        }
    }

    private async Task FillManagerFromCache(FiduciaDbContext ctx, string inn, CancellationToken ct)
    {
        var cached = await ctx.ExtSparkManagers
            .FirstOrDefaultAsync(x => x.Inn == inn, ct);
        if (cached is null)
            return;

        // Менеджер не найден в API — используем кэш как fallback.
        // Данные уже в БД, дополнительных действий не требуется.
    }

    private async Task<string?> UpdateOkopfIfFound(
        FiduciaDbContext ctx,
        SparkCompany company,
        Guid legalEntityId,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(company.OkopfCode))
            return null;

        var normalizedCode = new string(company.OkopfCode.Where(char.IsDigit).ToArray());
        var okopf = await ctx.RefOkopf.FirstOrDefaultAsync(o => o.Code == normalizedCode, ct);
        if (okopf is null)
        {
            _logger.LogWarning("Код ОКОПФ из СПАРК не найден в справочнике: {OkopfCode}", company.OkopfCode);
            return null;
        }

        var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == legalEntityId, ct);
        if (le is not null)
            le.OkopfId = okopf.Id;

        return normalizedCode;
    }

    // ── Маппинг ───────────────────────────────────────────────────

    private static SparkFounder MapToDto(ExtSparkFounder x)
    {
        return new SparkFounder
        {
            Name = x.Name,
            Inn = x.FounderInn,
            Ogrn = x.FounderOgrn,
            Country = x.Country,
            IsForeign = x.IsForeign,
            FullName = x.FullName,
            PersonInn = x.PersonInn,
            Citizenship = x.Citizenship,
            HeadOfOther = x.HeadOfOther,
            FounderOfOther = x.FounderOfOther,
            IsEntrepreneur = x.IsEntrepreneur,
            Ogrnip = x.Ogrnip,
            ShareAmount = x.ShareAmount,
            SharePercent = x.SharePercent,
            EntryDate = x.EntryDate,
            ExitDate = x.ExitDate
        };
    }
}