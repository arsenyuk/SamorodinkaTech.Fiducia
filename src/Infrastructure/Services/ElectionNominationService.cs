using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Services;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация сервиса работы с выдвижением кандидатов на должности СД.
/// </summary>
public class ElectionNominationService : IElectionNominationService
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly NotificationTextBuilder _textBuilder;
    private readonly ILogger<ElectionNominationService> _logger;

    /// <summary>
    /// Создаёт экземпляр сервиса выдвижения кандидатов.
    /// </summary>
    public ElectionNominationService(
        IApplicationDbContext context,
        INotificationService notificationService,
        ILogger<ElectionNominationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _textBuilder = new NotificationTextBuilder();
    }

    /// <inheritdoc />
    public async Task<int> SendConsentNotificationsAsync(
        Guid proposalId,
        string legalEntityName,
        CancellationToken cancellationToken = default)
    {
        var proposal = await _context.ElectionProposals
            .FirstOrDefaultAsync(p => p.Id == proposalId, cancellationToken);

        if (proposal is null)
        {
            _logger.LogWarning("Предложение {ProposalId} не найдено — уведомления о согласии не отправлены", proposalId);
            return 0;
        }

        // Находим подтверждённые кандидатуры
        var confirmedCandidacies = await _context.ElectionCandidacies
            .Where(c => c.ProposalId == proposalId && c.ConfirmedByMemberId != null)
            .ToListAsync(cancellationToken);

        if (confirmedCandidacies.Count == 0)
        {
            _logger.LogInformation("Нет подтверждённых кандидатур для предложения {ProposalId}", proposalId);
            return 0;
        }

        // Получаем userId для каждого кандидата
        var candidateMemberIds = confirmedCandidacies
            .Select(c => c.CandidateMemberId)
            .ToList();

        var boardMembers = await _context.BoardMembers
            .Where(bm => candidateMemberIds.Contains(bm.Id))
            .ToListAsync(cancellationToken);

        var memberMap = boardMembers.ToDictionary(bm => bm.Id, bm => bm);

        // Определяем тип уведомления по должности
        var notificationType = proposal.Position switch
        {
            "CHAIR" => NotificationType.CHAIRMAN_CONSENT,
            "DEPUTY_CHAIR" => NotificationType.DEPUTY_CHAIRMAN_CONSENT,
            _ => NotificationType.GENERAL
        };

        // Получаем уже существующие записи согласий, чтобы не создавать дубликаты
        var existingConsents = await _context.ElectionConsents
            .Where(c => c.ProposalId == proposalId)
            .ToListAsync(cancellationToken);

        var consentMap = existingConsents
            .ToDictionary(c => c.CandidateMemberId, c => c);

        var sent = 0;

        foreach (var candidacy in confirmedCandidacies)
        {
            if (!memberMap.TryGetValue(candidacy.CandidateMemberId, out var boardMember))
                continue;

            if (!boardMember.UserId.HasValue)
            {
                _logger.LogWarning(
                    "У кандидата {CandidateMemberId} ({FullName}) нет привязанной учётной записи — уведомление не отправлено",
                    boardMember.Id, boardMember.FullName);
                continue;
            }

            // Генерируем токен для страницы согласия (или используем существующий)
            var token = consentMap.TryGetValue(candidacy.CandidateMemberId, out var existing)
                ? existing.ConsentToken
                : Guid.NewGuid().ToString("N");

            var consentUrl = $"/election/consent/{token}";

            if (!consentMap.ContainsKey(candidacy.CandidateMemberId))
            {
                _context.ElectionConsents.Add(new ElectionConsent
                {
                    Id = Guid.NewGuid(),
                    ProposalId = proposalId,
                    CandidateMemberId = candidacy.CandidateMemberId,
                    ConsentToken = token,
                    ConsentGiven = false
                });
            }

            var (title, body) = notificationType switch
            {
                NotificationType.CHAIRMAN_CONSENT => _textBuilder.BuildChairmanConsent(legalEntityName, boardMember.FullName),
                NotificationType.DEPUTY_CHAIRMAN_CONSENT => _textBuilder.BuildDeputyChairmanConsent(legalEntityName, boardMember.FullName),
                _ => _textBuilder.BuildGeneral("Согласие на должность", $"Уважаемый {boardMember.FullName}! Подтвердите согласие на должность {proposal.Position}.")
            };

            var fullBody = body + $"\n\nДля подписания согласия перейдите по ссылке:\n{consentUrl}";

            await _notificationService.SendAsync(
                notificationType,
                title,
                fullBody,
                userId: boardMember.UserId.Value,
                cancellationToken: cancellationToken);

            sent++;
        }

        if (sent > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "SENT_NOMINATION_CONSENTS ProposalId={ProposalId} Position={Position} Sent={Sent} Total={Total}",
            proposalId, proposal.Position, sent, confirmedCandidacies.Count);

        return sent;
    }

    /// <inheritdoc />
    public async Task<int> SendNominationNotificationsAsync(
        Guid boardOfDirectorsId,
        string position,
        string legalEntityName,
        CancellationToken cancellationToken = default)
    {
        var boardMembers = await _context.BoardMembers
            .Where(bm => bm.BoardOfDirectorsId == boardOfDirectorsId && bm.UserId != null)
            .ToListAsync(cancellationToken);

        if (boardMembers.Count == 0)
        {
            _logger.LogWarning(
                "Нет членов СД с учётными записями для Совета {BoardId} — уведомления не отправлены",
                boardOfDirectorsId);
            return 0;
        }

        var notificationType = position switch
        {
            "CHAIR" => NotificationType.CHAIRMAN_NOMINATION,
            "DEPUTY_CHAIR" => NotificationType.DEPUTY_CHAIRMAN_NOMINATION,
            _ => NotificationType.GENERAL
        };

        var (title, body) = notificationType switch
        {
            NotificationType.CHAIRMAN_NOMINATION => _textBuilder.BuildChairmanNomination(legalEntityName),
            NotificationType.DEPUTY_CHAIRMAN_NOMINATION => _textBuilder.BuildDeputyChairmanNomination(legalEntityName),
            _ => _textBuilder.BuildGeneral("Сбор предложений", $"Сбор предложений на должность {position}.")
        };

        var userIds = boardMembers
            .Where(bm => bm.UserId.HasValue)
            .Select(bm => bm.UserId!.Value)
            .ToList();

        var ids = await _notificationService.SendToManyAsync(
            notificationType,
            title,
            body,
            userIds,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "SENT_NOMINATION_NOTIFICATIONS BoardId={BoardId} Position={Position} Sent={Sent}",
            boardOfDirectorsId, position, ids.Count);

        return ids.Count;
    }
}
