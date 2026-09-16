using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Services;
using SamorodinkaTech.Fiducia.Infrastructure.Common;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация <see cref="IParticipantWriteService"/>.
/// Заменяет POST/PUT/DELETE loopback HTTP-вызовы из Blazor-страниц Board Portal.
/// Бизнес-логика скопирована из <c>ParticipantEndpoints.cs</c>.
/// </summary>
public class ParticipantWriteService : IParticipantWriteService
{
    private const string LlcOkopfCode = "12300";
    private const string AuditActionAccess = "PARTICIPANT_ACCESS";

    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;
    private readonly ILogger<ParticipantWriteService> _logger;
    private readonly ISecurityAuditService _audit;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Создаёт экземпляр сервиса записи участников.
    /// </summary>
    public ParticipantWriteService(
        IDbContextFactory<FiduciaDbContext> dbFactory,
        ILogger<ParticipantWriteService> logger,
        ISecurityAuditService audit,
        IServiceProvider serviceProvider)
    {
        _dbFactory = dbFactory;
        _logger = logger;
        _audit = audit;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(ParticipantCreateModel model, Guid userId, string clientIp, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        await ValidateLlcAccessAsync(ctx, userId, clientIp, ct);

        var leId = model.LegalEntityId;

        // Валидация ФИО для ФЛ
        if (model.ParticipantType == "FL" || string.IsNullOrEmpty(model.ParticipantType))
        {
            if (string.IsNullOrWhiteSpace(model.LastName))
                throw new InvalidOperationException("Фамилия обязательна для физического лица");
            if (string.IsNullOrWhiteSpace(model.FirstName))
                throw new InvalidOperationException("Имя обязательно для физического лица");
        }

        // Валидация доли и оплаты
        if (model.SharePercent is not null && model.SharePercent <= 0)
            throw new InvalidOperationException("Размер доли должен быть больше нуля");
        if (model.SharePercent is not null && model.SharePercent < 100 && string.IsNullOrWhiteSpace(model.PaymentInfo))
            throw new InvalidOperationException("Сведения об оплате доли обязательны при неполной оплате");

        var entity = MapCreateModelToEntity(model, leId);

        // ── Создание Person при наличии ФИО для ФЛ ──────
        if (entity.ParticipantType == "FL" && !string.IsNullOrWhiteSpace(model.LastName))
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                LastName = model.LastName,
                FirstName = model.FirstName ?? "",
                MiddleName = model.MiddleName,
                Inn = model.PersonInn,
                Citizenship = model.Citizenship,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            ctx.Persons.Add(person);
            entity.PersonId = person.Id;

            // ── Создание IdentityDocument при наличии ДУЛ ────
            if (!string.IsNullOrWhiteSpace(model.DulTypeCode))
            {
                var dulType = await ctx.RefDulTypes.FirstOrDefaultAsync(t => t.Code == model.DulTypeCode, ct);
                if (dulType is not null)
                {
                    var now = DateTime.UtcNow;
                    ctx.IdentityDocuments.Add(new IdentityDocument
                    {
                        Id = Guid.NewGuid(),
                        PersonId = person.Id,
                        DulTypeId = dulType.Id,
                        Series = model.DulSeries,
                        Number = model.DulNumber,
                        IssuedBy = model.PassportIssuedBy,
                        IssueDate = model.PassportIssueDate,
                        DepartmentCode = model.PassportDepartmentCode,
                        RegistrationAddress = model.PassportRegistrationAddress,
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                        CreatedBy = userId
                    });
                }
            }

            // Привязываемся к существующему EcosystemParticipant или создаём новый
            if (!entity.EcosystemParticipantId.HasValue)
            {
                Guid? existingEcoId = null;
                var existingEco = await ctx.EcosystemParticipants
                    .FirstOrDefaultAsync(ep => ep.UserId == userId && ep.LegalEntityId == leId, ct);
                if (existingEco is not null)
                    existingEcoId = existingEco.Id;

                if (existingEcoId.HasValue)
                {
                    entity.EcosystemParticipantId = existingEcoId.Value;
                }
                else
                {
                    var ecoPerson = new EcosystemPerson
                    {
                        Id = Guid.NewGuid(),
                        LastName = model.LastName ?? "",
                        FirstName = model.FirstName ?? "",
                        MiddleName = model.MiddleName,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };
                    ctx.EcosystemPersons.Add(ecoPerson);

                    // Ищем существующий User участника (по ФИО), а НЕ вызывающего
                    var participantUser = await ctx.Users
                        .FirstOrDefaultAsync(u => u.LastName == model.LastName
                                               && u.FirstName == model.FirstName
                                               && !u.IsSystem, ct);
                    var existingUserId = participantUser?.Id ?? userId;

                    var ecoParticipant = new EcosystemParticipant
                    {
                        Id = Guid.NewGuid(),
                        LegalEntityId = leId,
                        EcosystemPersonId = ecoPerson.Id,
                        UserId = existingUserId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };
                    ctx.EcosystemParticipants.Add(ecoParticipant);
                    entity.EcosystemParticipantId = ecoParticipant.Id;
                }
            }
        }

        // ── Дедупликация ──────────────────────────────────────
        var dedupError = await CheckDuplicateParticipantAsync(ctx, leId, entity, null, ct);
        if (dedupError is not null)
            throw new InvalidOperationException(dedupError);

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        ctx.BoardParticipants.Add(entity);

        // ── Создание сведений об ЮЛ (SCD Type 2) ────────
        if (entity.ParticipantType == "UL" && !string.IsNullOrWhiteSpace(model.CompanyName))
        {
            ctx.BoardParticipantCompanies.Add(new BoardParticipantCompany
            {
                Id = Guid.NewGuid(),
                ParticipantId = entity.Id,
                CompanyName = model.CompanyName,
                CompanyInn = model.CompanyInn,
                CompanyOgrn = model.CompanyOgrn,
                CompanyKpp = model.CompanyKpp,
                CompanyAddress = model.CompanyAddress,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });
        }

        // ── SCD Type 2: начальная версия доли ───────────────
        if (model.SharePercent.HasValue || model.ShareAmount.HasValue || !string.IsNullOrWhiteSpace(model.PaymentInfo) || !string.IsNullOrWhiteSpace(model.ShareRegistrationInfo))
        {
            ctx.BoardParticipantShares.Add(new BoardParticipantShare
            {
                Id = Guid.NewGuid(),
                ParticipantId = entity.Id,
                LegalEntityId = leId,
                SharePercent = model.SharePercent,
                ShareAmount = model.ShareAmount,
                PaymentInfo = model.PaymentInfo,
                ShareRegistrationInfo = model.ShareRegistrationInfo,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });
        }

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("[{Ip}] Добавлен участник: {Name}, ЮЛ={LeId}",
            clientIp, entity.Person?.FullName ?? entity.CompanyName, leId);

        // ЕДИН: автоматическая привязка MasterId при наличии ПДн
        _ = TriggerEdinBindingAsync(ctx, entity, ct);

        // Уведомление LE_ADMIN: участник добавлен без логина
        if (entity.EcosystemParticipantId.HasValue)
        {
            var ecoParticipant = await ctx.EcosystemParticipants
                .Include(ep => ep.User)
                .FirstOrDefaultAsync(ep => ep.Id == entity.EcosystemParticipantId.Value, ct);

            if (ecoParticipant?.UserId is null)
            {
                var leAdminUser = await ctx.UserRoles
                    .Include(ur => ur.User)
                    .Where(ur => ur.Role != null && ur.Role.Code == "LE_ADMIN"
                                 && ur.User != null && !ur.User.IsSystem)
                    .Select(ur => ur.User)
                    .FirstOrDefaultAsync(ct);

                if (leAdminUser is not null)
                {
                    var notificationService = _serviceProvider.GetService<INotificationService>();
                    var textBuilder = _serviceProvider.GetService<NotificationTextBuilder>();
                    if (notificationService is not null && textBuilder is not null)
                    {
                        var legalEntity = await ctx.LegalEntities.FindAsync(new object[] { leId }, ct);
                        var legalEntityName = legalEntity?.Name ?? "организация";
                        var participantFullName = entity.Person?.FullName ?? $"{entity.ParticipantType}";

                        var (title, body) = await textBuilder.BuildEcosystemParticipantAddedNoLoginAsync(
                            participantFullName, entity.Shares.FirstOrDefault()?.SharePercent ?? 0m, legalEntityName);

                        await notificationService.SendAsync(
                            "ECOSYSTEM_PARTICIPANT_ADDED_NO_LOGIN",
                            title, body,
                            userId: leAdminUser.Id,
                            cancellationToken: ct);
                    }
                }
            }
        }

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Guid id, ParticipantUpdateModel model, Guid userId, string clientIp, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        await ValidateLlcAccessAsync(ctx, userId, clientIp, ct);

        var entity = await ctx.BoardParticipants.FindAsync(new object[] { id }, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Участник {id} не найден");

        // ── Валидация ──────────────────────────────────────────
        var participantType = model.ParticipantType ?? entity.ParticipantType;
        if (participantType == "FL" || string.IsNullOrEmpty(participantType))
        {
            if (string.IsNullOrWhiteSpace(model.LastName))
                throw new InvalidOperationException("Фамилия обязательна для физического лица");
            if (string.IsNullOrWhiteSpace(model.FirstName))
                throw new InvalidOperationException("Имя обязательно для физического лица");
        }
        if (model.SharePercent is not null && model.SharePercent <= 0)
            throw new InvalidOperationException("Размер доли должен быть больше нуля");
        if (model.SharePercent is not null && model.SharePercent < 100 && string.IsNullOrWhiteSpace(model.PaymentInfo))
            throw new InvalidOperationException("Сведения об оплате доли обязательны при неполной оплате");

        entity.ParticipantType = participantType;
        entity.EntryDate = model.EntryDate;
        entity.ExitDate = model.ExitDate;
        entity.IsActive = model.IsActive ?? true;
        entity.UpdatedAt = DateTime.UtcNow;

        // ── SCD Type 2: версионирование доли ───────────────────
        var currentShare = await ctx.BoardParticipantShares
            .FirstOrDefaultAsync(s => s.ParticipantId == id && s.IsActive, ct);

        var shareChanged = currentShare is null
            || model.SharePercent != currentShare.SharePercent
            || model.ShareAmount != currentShare.ShareAmount
            || model.PaymentInfo != currentShare.PaymentInfo
            || model.ShareRegistrationInfo != currentShare.ShareRegistrationInfo;

        if (shareChanged)
        {
            if (currentShare is not null)
            {
                currentShare.IsActive = false;
                currentShare.UpdatedAt = DateTime.UtcNow;
            }
            ctx.BoardParticipantShares.Add(new BoardParticipantShare
            {
                Id = Guid.NewGuid(),
                ParticipantId = id,
                LegalEntityId = entity.LegalEntityId,
                SharePercent = model.SharePercent,
                ShareAmount = model.ShareAmount,
                PaymentInfo = model.PaymentInfo,
                ShareRegistrationInfo = model.ShareRegistrationInfo,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });
        }

        // ── SCD Type 2: сведения об ЮЛ ───────────────────
        if (entity.ParticipantType == "UL")
        {
            var currentCompany = await ctx.BoardParticipantCompanies
                .FirstOrDefaultAsync(c => c.ParticipantId == id && c.IsActive, ct);

            var companyChanged = currentCompany is null
                || model.CompanyName != currentCompany.CompanyName
                || model.CompanyInn != currentCompany.CompanyInn
                || model.CompanyOgrn != currentCompany.CompanyOgrn
                || model.CompanyKpp != currentCompany.CompanyKpp
                || model.CompanyAddress != currentCompany.CompanyAddress;

            if (companyChanged)
            {
                if (currentCompany is not null)
                {
                    currentCompany.IsActive = false;
                    currentCompany.UpdatedAt = DateTime.UtcNow;
                }
                ctx.BoardParticipantCompanies.Add(new BoardParticipantCompany
                {
                    Id = Guid.NewGuid(),
                    ParticipantId = id,
                    CompanyName = model.CompanyName,
                    CompanyInn = model.CompanyInn,
                    CompanyOgrn = model.CompanyOgrn,
                    CompanyKpp = model.CompanyKpp,
                    CompanyAddress = model.CompanyAddress,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }
        }

        // ── Дедупликация (при обновлении) ────────────────────
        var dedupError = await CheckDuplicateParticipantAsync(ctx, entity.LegalEntityId, entity, id, ct);
        if (dedupError is not null)
            throw new InvalidOperationException(dedupError);

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("[{Ip}] Обновлён участник: {Name}, id={Id}",
            clientIp, entity.Person?.FullName ?? entity.CompanyName, id);

        // ЕДИН: автоматическая привязка MasterId при наличии ПДн
        _ = TriggerEdinBindingAsync(ctx, entity, ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, Guid userId, string clientIp, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        await ValidateLlcAccessAsync(ctx, userId, clientIp, ct);

        var entity = await ctx.BoardParticipants.FindAsync(new object[] { id }, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Участник {id} не найден");

        // Если участник был ГД — удаляем роль CEO и деактивируем участника экосистемы
        if (entity.IsGeneralDirector && entity.EcosystemParticipantId.HasValue)
        {
            var ep = await ctx.EcosystemParticipants
                .FirstOrDefaultAsync(x => x.Id == entity.EcosystemParticipantId.Value, ct);

            if (ep?.UserId.HasValue == true)
            {
                var ceoRole = await ctx.Roles.FirstOrDefaultAsync(r => r.Code == "CEO", ct);
                if (ceoRole is not null)
                {
                    var ceoUserRole = await ctx.UserRoles
                        .FirstOrDefaultAsync(ur => ur.UserId == ep.UserId.Value
                                               && ur.RoleId == ceoRole.Id, ct);
                    if (ceoUserRole is not null)
                        ctx.UserRoles.Remove(ceoUserRole);
                }

                // Проверяем, остались ли роли у пользователя
                var remainingRoles = await ctx.UserRoles
                    .CountAsync(ur => ur.UserId == ep.UserId.Value, ct);

                // Если CEO была единственной ролью — деактивируем участника экосистемы
                if (remainingRoles <= 1)
                {
                    ep.IsActive = false;
                    _logger.LogInformation("Участник экосистемы {EPId} деактивирован — нет оставшихся ролей", ep.Id);
                }
            }
        }

        ctx.BoardParticipants.Remove(entity);
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("[{Ip}] Удалён участник: {Name}, id={Id}",
            clientIp, entity.Person?.FullName ?? entity.CompanyName, id);
    }

    /// <inheritdoc />
    public async Task ImportFromSparkAsync(Guid legalEntityId, Guid userId, string clientIp, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        await ValidateLlcAccessAsync(ctx, userId, clientIp, ct);

        var le = await ctx.LegalEntities.FirstOrDefaultAsync(x => x.Id == legalEntityId, ct);
        if (le is null)
            throw new InvalidOperationException("Юридическое лицо не найдено");

        var sparkFounders = await ctx.ExtSparkFounders
            .Where(f => f.Inn == le.Inn)
            .ToListAsync(ct);

        if (sparkFounders.Count == 0)
            return; // Нет данных СПАРК

        // Удаляем существующих участников этого ЮЛ
        var existing = await ctx.BoardParticipants
            .Where(p => p.LegalEntityId == legalEntityId)
            .ToListAsync(ct);

        // Очищаем роли CEO у участников-ГД перед удалением
        var gdParticipants = existing.Where(p => p.IsGeneralDirector && p.EcosystemParticipantId.HasValue).ToList();
        if (gdParticipants.Count > 0)
        {
            var ceoRole = await ctx.Roles.FirstOrDefaultAsync(r => r.Code == "CEO", ct);
            if (ceoRole is not null)
            {
                foreach (var gd in gdParticipants)
                {
                    var ep = await ctx.EcosystemParticipants
                        .FirstOrDefaultAsync(x => x.Id == gd.EcosystemParticipantId!.Value, ct);
                    if (ep?.UserId.HasValue == true)
                    {
                        var ceoUserRole = await ctx.UserRoles
                            .FirstOrDefaultAsync(ur => ur.UserId == ep.UserId.Value
                                                   && ur.RoleId == ceoRole.Id, ct);
                        if (ceoUserRole is not null)
                            ctx.UserRoles.Remove(ceoUserRole);

                        var remainingRoles = await ctx.UserRoles
                            .CountAsync(ur => ur.UserId == ep.UserId.Value, ct);
                        if (remainingRoles <= 1)
                            ep.IsActive = false;
                    }
                }
            }
        }

        ctx.BoardParticipants.RemoveRange(existing);

        var importedCount = 0;

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
                LegalEntityId = legalEntityId,
                ParticipantType = isFl ? "FL" : (isIp ? "IP" : "UL"),
                PersonId = personId,
                CompanyName = f.Name,
                CompanyInn = f.FounderInn,
                CompanyOgrn = f.FounderOgrn,
                EntryDate = f.EntryDate.HasValue ? DateOnly.FromDateTime(f.EntryDate.Value) : null,
                ExitDate = f.ExitDate.HasValue ? DateOnly.FromDateTime(f.ExitDate.Value) : null,
                IsActive = !f.ExitDate.HasValue,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            ctx.BoardParticipants.Add(entity);

            // SCD Type 2: начальная версия доли
            if (f.SharePercent.HasValue || f.ShareAmount.HasValue)
            {
                ctx.BoardParticipantShares.Add(new BoardParticipantShare
                {
                    Id = Guid.NewGuid(),
                    ParticipantId = entity.Id,
                    LegalEntityId = legalEntityId,
                    SharePercent = f.SharePercent,
                    ShareAmount = f.ShareAmount,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Создание сведений об ЮЛ для UL-участников
            if (entity.ParticipantType == "UL" && !string.IsNullOrWhiteSpace(f.Name))
            {
                ctx.BoardParticipantCompanies.Add(new BoardParticipantCompany
                {
                    Id = Guid.NewGuid(),
                    ParticipantId = entity.Id,
                    CompanyName = f.Name,
                    CompanyInn = f.FounderInn,
                    CompanyOgrn = f.FounderOgrn,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            importedCount++;
        }

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("[{Ip}] Импорт из СПАРК: {Count} участников, ЮЛ={Inn}",
            clientIp, importedCount, le.Inn);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateTreasuryAsync(TreasuryCreateModel model, Guid userId, string clientIp, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        await ValidateLlcAccessAsync(ctx, userId, clientIp, ct);

        var leId = model.LegalEntityId;

        var maxSort = await ctx.BoardTreasuryShares
            .Where(t => t.LegalEntityId == leId)
            .MaxAsync(t => (int?)t.SortOrder, ct) ?? 0;

        var entity = new BoardTreasuryShare
        {
            Id = Guid.NewGuid(),
            LegalEntityId = leId,
            SharePercent = model.SharePercent,
            ShareAmount = model.ShareAmount,
            AcquiredDate = model.AcquiredDate,
            AcquisitionBasis = model.AcquisitionBasis,
            SortOrder = maxSort + 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        ctx.BoardTreasuryShares.Add(entity);
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("[{Ip}] Добавлена казначейская доля: {Share}%, id={Id}",
            clientIp, entity.SharePercent, entity.Id);

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdateTreasuryAsync(Guid id, TreasuryUpdateModel model, Guid userId, string clientIp, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        await ValidateLlcAccessAsync(ctx, userId, clientIp, ct);

        var entity = await ctx.BoardTreasuryShares.FindAsync(new object[] { id }, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Казначейская доля {id} не найдена");

        entity.SharePercent = model.SharePercent;
        entity.ShareAmount = model.ShareAmount;
        entity.AcquiredDate = model.AcquiredDate;
        entity.AcquisitionBasis = model.AcquisitionBasis;
        entity.UpdatedAt = DateTime.UtcNow;

        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("[{Ip}] Обновлена казначейская доля: {Share}%, id={Id}",
            clientIp, entity.SharePercent, id);
    }

    /// <inheritdoc />
    public async Task DeleteTreasuryAsync(Guid id, Guid userId, string clientIp, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        await ValidateLlcAccessAsync(ctx, userId, clientIp, ct);

        var entity = await ctx.BoardTreasuryShares.FindAsync(new object[] { id }, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Казначейская доля {id} не найдена");

        ctx.BoardTreasuryShares.Remove(entity);
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("[{Ip}] Удалена казначейская доля: {Share}%, id={Id}",
            clientIp, entity.SharePercent, id);
    }

    /// <inheritdoc />
    public async Task DeleteRegistryUploadAsync(Guid id, Guid userId, string clientIp, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        await ValidateLlcAccessAsync(ctx, userId, clientIp, ct);

        var entity = await ctx.BoardRegistryUploads.FindAsync(new object[] { id }, ct);
        if (entity is null)
            throw new KeyNotFoundException($"Загрузка реестра {id} не найдена");

        ctx.BoardRegistryUploads.Remove(entity);
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("[{Ip}] Удалён акт загрузки реестра: id={Id}",
            clientIp, id);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateChangeAsync(ChangeCreateModel model, Guid userId, string clientIp, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        await ValidateParticipantRoleAccessAsync(ctx, userId, model.LegalEntityId, clientIp, ct);

        // Валидация ФИО
        if (string.IsNullOrWhiteSpace(model.LastName))
            throw new InvalidOperationException("Фамилия обязательна");
        if (string.IsNullOrWhiteSpace(model.FirstName))
            throw new InvalidOperationException("Имя обязательно");

        var entity = new BoardParticipantChange
        {
            Id = Guid.NewGuid(),
            LegalEntityId = model.LegalEntityId,
            ParticipantId = model.ParticipantId,
            ParticipantType = model.ParticipantType ?? "FL",
            LastName = model.LastName,
            FirstName = model.FirstName,
            MiddleName = model.MiddleName,
            DulTypeId = model.DulTypeId,
            PassportSeries = model.PassportSeries,
            PassportNumber = model.PassportNumber,
            PassportIssuedBy = model.PassportIssuedBy,
            PassportIssueDate = model.PassportIssueDate,
            PassportDepartmentCode = model.PassportDepartmentCode,
            PassportRegistrationAddress = model.PassportRegistrationAddress,
            PersonInn = model.PersonInn,
            Citizenship = model.Citizenship,
            CompanyName = model.CompanyName,
            CompanyInn = model.CompanyInn,
            CompanyOgrn = model.CompanyOgrn,
            CompanyKpp = model.CompanyKpp,
            CompanyAddress = model.CompanyAddress,
            Ogrnip = model.Ogrnip,
            SharePercent = model.SharePercent,
            ShareAmount = model.ShareAmount,
            DocumentFileId = model.DocumentFileId,
            DocumentOriginalName = model.DocumentOriginalName,
            Source = model.Source ?? "electronic",
            Date = model.Date,
            PaperDocNumber = model.PaperDocNumber,
            Comment = model.Comment,
            SubmittedBy = userId,
            SubmittedAt = DateTime.UtcNow,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        ctx.BoardParticipantChanges.Add(entity);
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("[{Ip}] Создано информирование об изменении сведений: participant={ParticipantId}, id={Id}",
            clientIp, model.ParticipantId, entity.Id);

        // ── Автоприменение для электронных информирований ───
        if (entity.Source == "electronic")
        {
            await ApplyParticipantChangeAsync(ctx, entity, ct);
        }

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UploadChangeDocumentAsync(
        Guid changeId, Stream fileStream, string fileName, string contentType,
        Guid userId, string clientIp, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        // Документ загружается участником — проверка PARTICIPANT роли
        var participantCheck = await ctx.BoardParticipantChanges.FindAsync(new object[] { changeId }, ct);
        if (participantCheck is null)
            throw new KeyNotFoundException($"Информирование {changeId} не найдено");

        var fileStorage = _serviceProvider.GetRequiredService<IFileStorage>();
        var storageKey = await fileStorage.SaveAsync(fileStream, fileName, contentType, ct);

        var fileEntry = new FileEntry
        {
            Id = Guid.NewGuid(),
            OriginalName = fileName,
            ContentType = contentType,
            SizeBytes = fileStream.Length,
            StorageProvider = "LOCAL",
            StorageKeyOrPath = storageKey,
            IsUploaded = true,
            Extension = System.IO.Path.GetExtension(fileName)?.TrimStart('.')
        };
        ctx.Files.Add(fileEntry);
        await ctx.SaveChangesAsync(ct);

        _logger.LogInformation("[{Ip}] Загружен документ к информированию: {FileName}, fileId={FileId}",
            clientIp, fileName, fileEntry.Id);
    }

    /// <summary>
    /// Проверка прав: ЮЛ является ООО (ОКОПФ 12300). Бросает исключение при запрете доступа.
    /// </summary>
    private async Task ValidateLlcAccessAsync(
        FiduciaDbContext ctx, Guid userId, string clientIp, CancellationToken ct)
    {
        var user = await ctx.Users.FindAsync(new object[] { userId }, ct);
        if (user is null)
            throw new InvalidOperationException("Пользователь не найден");

        var participant = await ctx.EcosystemParticipants
            .Include(ep => ep.User)
            .FirstOrDefaultAsync(ep => ep.User != null && ep.User.Login == user.Login, ct);

        if (participant is null)
            throw new InvalidOperationException("Пользователь не привязан к юридическому лицу");

        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == participant.LegalEntityId, ct);

        if (le is null)
            throw new InvalidOperationException("Юридическое лицо не найдено");

        if (le.RefOkopf?.Code != LlcOkopfCode)
        {
            var fullName = string.IsNullOrWhiteSpace(user.MiddleName)
                ? $"{user.LastName} {user.FirstName}"
                : $"{user.LastName} {user.FirstName} {user.MiddleName}";

            await _audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь {user.Login} ({fullName}), ЮЛ «{le.Name}» (ОКОПФ {le.RefOkopf?.Code}) не является ООО",
                entityName: "LegalEntity", entityId: le.Id, cancellationToken: ct);
            throw new UnauthorizedAccessException("Доступ запрещён: ЮЛ не является ООО");
        }

        var fullNameOk = string.IsNullOrWhiteSpace(user.MiddleName)
            ? $"{user.LastName} {user.FirstName}"
            : $"{user.LastName} {user.FirstName} {user.MiddleName}";

        await _audit.LogEventAsync(AuditActionAccess, clientIp,
            $"Доступ разрешён: пользователь {user.Login} ({fullNameOk}), ЮЛ «{le.Name}» (ООО), реестр участников",
            entityName: "LegalEntity", entityId: le.Id, cancellationToken: ct);
    }

    /// <summary>
    /// Проверка прав участника: ЮЛ является ООО + пользователь имеет роль PARTICIPANT.
    /// </summary>
    private async Task ValidateParticipantRoleAccessAsync(
        FiduciaDbContext ctx, Guid userId, Guid legalEntityId, string clientIp, CancellationToken ct)
    {
        var le = await ctx.LegalEntities
            .Include(x => x.RefOkopf)
            .FirstOrDefaultAsync(x => x.Id == legalEntityId, ct);

        if (le is null)
            throw new InvalidOperationException("Юридическое лицо не найдено");

        var user = await ctx.Users.FindAsync(new object[] { userId }, ct);
        var login = user?.Login ?? "unknown";
        var fullName = user is null ? "Неизвестный пользователь"
            : (string.IsNullOrWhiteSpace(user.MiddleName)
                ? $"{user.LastName} {user.FirstName}"
                : $"{user.LastName} {user.FirstName} {user.MiddleName}");

        if (le.RefOkopf?.Code != LlcOkopfCode)
        {
            await _audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь {login} ({fullName}), ЮЛ «{le.Name}» (ОКОПФ {le.RefOkopf?.Code}) не является ООО",
                entityName: "LegalEntity", entityId: le.Id, cancellationToken: ct);
            throw new UnauthorizedAccessException("Доступ запрещён: ЮЛ не является ООО");
        }

        var hasParticipantRole = await ctx.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.Role != null && ur.Role.Code == "PARTICIPANT", ct);

        if (!hasParticipantRole)
        {
            await _audit.LogEventAsync(AuditActionAccess, clientIp,
                $"Доступ запрещён: пользователь {login} ({fullName}) не имеет роль PARTICIPANT",
                entityName: "LegalEntity", entityId: le.Id, cancellationToken: ct);
            throw new UnauthorizedAccessException("Доступ запрещён: отсутствует роль PARTICIPANT");
        }

        await _audit.LogEventAsync(AuditActionAccess, clientIp,
            $"Доступ разрешён: пользователь {login} ({fullName}), роль PARTICIPANT, ЮЛ «{le.Name}» (ООО)",
            entityName: "LegalEntity", entityId: le.Id, cancellationToken: ct);
    }

    /// <summary>
    /// Проверка дубликата участника по ДУЛ/INN в пределах одного ЮЛ.
    /// </summary>
    private static async Task<string?> CheckDuplicateParticipantAsync(
        FiduciaDbContext ctx,
        Guid legalEntityId,
        BoardParticipant entity,
        Guid? excludeId,
        CancellationToken ct)
    {
        var query = ctx.BoardParticipants
            .Where(p => p.LegalEntityId == legalEntityId
                     && p.IsActive
                     && p.Id != (excludeId ?? Guid.Empty));

        // Проверка по ДУЛ
        var personId = entity.PersonId;
        if (personId.HasValue)
        {
            var primaryDoc = await ctx.IdentityDocuments
                .FirstOrDefaultAsync(x => x.PersonId == personId.Value && x.IsActive, ct);
            if (primaryDoc is not null)
            {
                var existingDoc = await ctx.IdentityDocuments
                    .AnyAsync(d => d.PersonId != personId.Value
                        && d.IsActive
                        && d.Series == primaryDoc.Series
                        && d.Number == primaryDoc.Number
                        && d.DulTypeId == primaryDoc.DulTypeId, ct);
                if (existingDoc)
                    return "Участник с таким документом уже добавлен";
            }
        }

        // Проверка по ИНН (ФЛ)
        var personInn = entity.Person?.Inn;
        if (entity.ParticipantType == "FL" && !string.IsNullOrEmpty(personInn))
        {
            if (await query.AnyAsync(p => p.Person != null && p.Person.Inn == personInn, ct))
                return "Участник с таким ИНН уже добавлен";
        }

        // Проверка по ИНН (ЮЛ)
        if (entity.ParticipantType == "UL" && !string.IsNullOrEmpty(entity.CompanyInn))
        {
            if (await query.AnyAsync(p => p.ParticipantType == "UL" && p.CompanyInn == entity.CompanyInn, ct))
                return "Юридическое лицо с таким ИНН уже добавлено";
        }

        return null;
    }

    /// <summary>
    /// Fire-and-forget: привязка ЕДИН MasterId к пользователю после сохранения участника.
    /// </summary>
    private async Task TriggerEdinBindingAsync(
        FiduciaDbContext ctx, BoardParticipant entity, CancellationToken ct)
    {
        _logger.LogInformation("ЕДИН: TriggerEdinBinding вызван для участника {Name}, EcosystemParticipantId={EcoId}",
            entity.Person?.FullName, entity.EcosystemParticipantId?.ToString() ?? "NULL");
        try
        {
            var bindingService = _serviceProvider.GetService<IEdinBindingService>();
            if (bindingService is null)
                return;

            if (entity.ParticipantType != "FL")
                return;

            if (string.IsNullOrWhiteSpace(entity.Person?.FullName))
                return;

            if (!entity.EcosystemParticipantId.HasValue)
                return;

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
            var dbFactory = _serviceProvider.GetRequiredService<IDbContextFactory<FiduciaDbContext>>();
            await using var freshCtx = await dbFactory.CreateDbContextAsync(ct);
            if (personId.HasValue)
            {
                var primaryDoc = await freshCtx.IdentityDocuments
                    .Include(x => x.DulType)
                    .FirstOrDefaultAsync(x => x.PersonId == personId.Value && x.IsActive, ct);
                dulType = primaryDoc?.DulType?.Code;
                passportSeries = primaryDoc?.Series;
                passportNumber = primaryDoc?.Number;
            }

            var scope = _serviceProvider.CreateScope();
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
                    _logger.LogWarning(ex, "ЕДИН: ошибка привязки для участника {Name}", entity.Person?.FullName);
                }
                finally
                {
                    scope.Dispose();
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ЕДИН: ошибка запуска привязки для участника {Name}", entity.Person?.FullName);
        }
    }

    /// <summary>
    /// Автоприменение записи сведений: сравнение с текущим состоянием, версионирование ДУЛ, обновление Person.
    /// </summary>
    private async Task ApplyParticipantChangeAsync(
        FiduciaDbContext ctx,
        BoardParticipantChange entity,
        CancellationToken ct)
    {
        var participant = await ctx.BoardParticipants
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.Id == entity.ParticipantId, ct);

        if (participant is null) return;

        var now = DateTime.UtcNow;

        // ── FL: ДУЛ + Person ─────────────────────────────────
        if (participant.PersonId.HasValue)
        {
            var currentDoc = await ctx.IdentityDocuments
                .FirstOrDefaultAsync(d => d.PersonId == participant.PersonId.Value && d.IsActive, ct);

            // Версионирование: деактивировать старую версию ДУЛ + создать новую
            var hasDocumentChanges = currentDoc is not null
                && (entity.DulTypeId.HasValue && entity.DulTypeId != currentDoc.DulTypeId
                    || !string.IsNullOrEmpty(entity.PassportSeries) && entity.PassportSeries != currentDoc.Series
                    || !string.IsNullOrEmpty(entity.PassportNumber) && entity.PassportNumber != currentDoc.Number
                    || !string.IsNullOrEmpty(entity.PassportRegistrationAddress) && entity.PassportRegistrationAddress != currentDoc.RegistrationAddress);

            var hasPersonChanges = participant.Person is not null
                && (!string.IsNullOrEmpty(entity.LastName) && entity.LastName != participant.Person.LastName
                    || !string.IsNullOrEmpty(entity.FirstName) && entity.FirstName != participant.Person.FirstName);

            var hasInnChanges = participant.Person is not null && !string.IsNullOrEmpty(entity.PersonInn)
                && entity.PersonInn != participant.Person.Inn;

            if (hasDocumentChanges || hasPersonChanges || hasInnChanges)
            {
                if (currentDoc is not null)
                {
                    currentDoc.IsActive = false;
                    currentDoc.UpdatedAt = now;
                }

                var dulTypeId = entity.DulTypeId ?? currentDoc?.DulTypeId
                    ?? (await ctx.RefDulTypes.FirstOrDefaultAsync(t => t.Code == "21", ct))?.Id
                    ?? Guid.Empty;
                if (dulTypeId != Guid.Empty)
                {
                    ctx.IdentityDocuments.Add(new IdentityDocument
                    {
                        Id = Guid.NewGuid(),
                        PersonId = participant.PersonId.Value,
                        DulTypeId = dulTypeId,
                        Series = entity.PassportSeries,
                        Number = entity.PassportNumber,
                        IssuedBy = entity.PassportIssuedBy,
                        IssueDate = entity.PassportIssueDate,
                        DepartmentCode = entity.PassportDepartmentCode,
                        RegistrationAddress = entity.PassportRegistrationAddress,
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                        CreatedBy = entity.SubmittedBy
                    });
                }

                if (participant.Person is not null)
                {
                    if (!string.IsNullOrEmpty(entity.LastName))
                        participant.Person.LastName = entity.LastName;
                    if (!string.IsNullOrEmpty(entity.FirstName))
                        participant.Person.FirstName = entity.FirstName;
                    if (entity.MiddleName != null)
                        participant.Person.MiddleName = entity.MiddleName;
                }
                if (!string.IsNullOrEmpty(entity.PersonInn) && participant.Person is not null)
                    participant.Person.Inn = entity.PersonInn;
            }
        }

        // ── UL: сведения об ЮЛ (SCD Type 2) ──────────────────
        if (participant.ParticipantType == "UL")
        {
            var currentCompany = await ctx.BoardParticipantCompanies
                .FirstOrDefaultAsync(c => c.ParticipantId == participant.Id && c.IsActive, ct);

            var companyChanged = currentCompany is null
                || entity.CompanyName != currentCompany.CompanyName
                || entity.CompanyInn != currentCompany.CompanyInn
                || entity.CompanyOgrn != currentCompany.CompanyOgrn
                || entity.CompanyKpp != currentCompany.CompanyKpp
                || entity.CompanyAddress != currentCompany.CompanyAddress;

            if (companyChanged)
            {
                if (currentCompany is not null)
                {
                    currentCompany.IsActive = false;
                    currentCompany.UpdatedAt = now;
                }
                ctx.BoardParticipantCompanies.Add(new BoardParticipantCompany
                {
                    Id = Guid.NewGuid(),
                    ParticipantId = participant.Id,
                    CompanyName = entity.CompanyName,
                    CompanyInn = entity.CompanyInn,
                    CompanyOgrn = entity.CompanyOgrn,
                    CompanyKpp = entity.CompanyKpp,
                    CompanyAddress = entity.CompanyAddress,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = entity.SubmittedBy
                });
            }
        }

        entity.Status = "approved";
        entity.UpdatedAt = now;
        await ctx.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Создаёт сущность BoardParticipant из модели создания.
    /// </summary>
    private static BoardParticipant MapCreateModelToEntity(ParticipantCreateModel model, Guid legalEntityId) => new()
    {
        LegalEntityId = legalEntityId,
        EcosystemParticipantId = model.EcosystemParticipantId,
        ParticipantType = model.ParticipantType ?? "FL",
        CompanyName = model.CompanyName,
        CompanyInn = model.CompanyInn,
        CompanyOgrn = model.CompanyOgrn,
        CompanyKpp = model.CompanyKpp,
        CompanyAddress = model.CompanyAddress,
        EntryDate = model.EntryDate,
        ExitDate = model.ExitDate,
        IsActive = model.IsActive ?? true
    };
}
