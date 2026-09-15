using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Email;

namespace SamorodinkaTech.Fiducia.Infrastructure.Notifications;

/// <summary>
/// Реализация сервиса отправки уведомлений (US-009).
/// Фиксирует факт формирования уведомления в таблице notifications и в логе.
/// При наличии IEmailService — отправляет email получателю.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    private readonly IEmailService? _emailService;

    /// <summary>
    /// Создаёт экземпляр сервиса отправки уведомлений.
    /// </summary>
    /// <param name="context">Контекст доступа к данным приложения.</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="emailService">Сервис отправки email (null если SMTP отключён).</param>
    public NotificationService(
        IApplicationDbContext context,
        ILogger<NotificationService> logger,
        IEmailService? emailService = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailService = emailService;
    }

    /// <inheritdoc />
    public async Task<Guid> SendAsync(
        string notificationType,
        string title,
        string body,
        Guid? userId = null,
        Guid? committeeId = null,
        Guid? meetingId = null,
        string? url = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            CommitteeId = committeeId,
            MeetingId = meetingId,
            NotificationType = notificationType,
            Title = title,
            Body = body,
            Url = url,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);

        _logger.LogDebug(
            "DB_CREATE Notification Id={NotificationId} Type={NotificationType} UserId={UserId} CommitteeId={CommitteeId} MeetingId={MeetingId}",
            notification.Id, notification.NotificationType, notification.UserId, notification.CommitteeId, notification.MeetingId);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "NOTIFICATION_SENT Id={NotificationId} Type={NotificationType} UserId={UserId} Title={Title}",
            notification.Id, notification.NotificationType, notification.UserId, notification.Title);

        if (userId.HasValue && _emailService is not null)
        {
            _ = Task.Run(async () => await SendEmailAsync(userId.Value, title, body, cancellationToken));
        }

        return notification.Id;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> SendToManyAsync(
        string notificationType,
        string title,
        string body,
        IEnumerable<Guid> userIds,
        Guid? committeeId = null,
        Guid? meetingId = null,
        string? url = null,
        CancellationToken cancellationToken = default)
    {
        var distinctUserIds = userIds.Distinct().ToList();
        var notifications = distinctUserIds.Select(userId => new Notification
        {
            UserId = userId,
            CommitteeId = committeeId,
            MeetingId = meetingId,
            NotificationType = notificationType,
            Title = title,
            Body = body,
            Url = url,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        if (notifications.Count == 0)
        {
            return Array.Empty<Guid>();
        }

        foreach (var notification in notifications)
        {
            _context.Notifications.Add(notification);
        }

        _logger.LogDebug(
            "DB_CREATE Notifications batch Count={Count} Type={NotificationType} UserIds={UserIds}",
            notifications.Count, notificationType, string.Join(",", distinctUserIds));

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            _logger.LogInformation(
                "NOTIFICATION_SENT Id={NotificationId} Type={NotificationType} UserId={UserId} Title={Title}",
                notification.Id, notification.NotificationType, notification.UserId, notification.Title);
        }

        if (_emailService is not null)
        {
            foreach (var userId in distinctUserIds)
            {
                _ = Task.Run(async () => await SendEmailAsync(userId, title, body, cancellationToken));
            }
        }

        return notifications.Select(n => n.Id).ToList();
    }

    private async Task SendEmailAsync(Guid userId, string title, string body, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
                return;

            var participant = await _context.EcosystemParticipants
                .FirstOrDefaultAsync(ep => ep.UserId == userId, cancellationToken);

            if (participant is null)
                return;

            var emailSettings = await _context.LegalEntityEmailSettings
                .FirstOrDefaultAsync(x => x.LegalEntityId == participant.LegalEntityId, cancellationToken);

            if (emailSettings is not null && !emailSettings.EmailEnabled)
                return;

            var htmlBody = BuildHtmlBody(body, emailSettings);

            var message = new EmailMessage
            {
                To = user.Email,
                Subject = title,
                HtmlBody = htmlBody,
                TextBody = body
            };

            await _emailService!.SendAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "EMAIL_SEND_FAILED UserId={UserId} Title={Title}",
                userId, title);
        }
    }

    private static string BuildHtmlBody(string body, LegalEntityEmailSettings? emailSettings)
    {
        var html = "";

        if (emailSettings is { HeaderEnabled: true } && !string.IsNullOrWhiteSpace(emailSettings.HeaderMarkdown))
        {
            var pipeline = new Markdig.MarkdownPipelineBuilder().Build();
            html += Markdig.Markdown.ToHtml(emailSettings.HeaderMarkdown, pipeline);
        }

        html += $"<div style=\"font-family:sans-serif;font-size:14px;line-height:1.6;\">{body}</div>";

        if (emailSettings is { FooterEnabled: true } && !string.IsNullOrWhiteSpace(emailSettings.FooterMarkdown))
        {
            var pipeline = new Markdig.MarkdownPipelineBuilder().Build();
            html += Markdig.Markdown.ToHtml(emailSettings.FooterMarkdown, pipeline);
        }

        return html;
    }
}
