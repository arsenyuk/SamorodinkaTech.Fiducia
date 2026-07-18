using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Notifications;

/// <summary>
/// Реализация сервиса отправки уведомлений (US-009).
/// Не выполняет реальную отправку по внешним каналам связи (Email/SMS/Push) —
/// фиксирует факт формирования уведомления в таблице notifications и в логе (через Serilog/ILogger).
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    /// <summary>
    /// Создаёт экземпляр сервиса отправки уведомлений.
    /// </summary>
    /// <param name="context">Контекст доступа к данным приложения.</param>
    /// <param name="logger">Логгер.</param>
    public NotificationService(
        IApplicationDbContext context,
        ILogger<NotificationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<Guid> SendAsync(
        NotificationType notificationType,
        string title,
        string body,
        Guid? userId = null,
        Guid? committeeId = null,
        Guid? meetingId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            CommitteeId = committeeId,
            MeetingId = meetingId,
            NotificationType = notificationType.ToString(),
            Title = title,
            Body = body,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "NOTIFICATION_SENT Id={NotificationId} Type={NotificationType} UserId={UserId} Title={Title}",
            notification.Id, notification.NotificationType, notification.UserId, notification.Title);

        return notification.Id;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> SendToManyAsync(
        NotificationType notificationType,
        string title,
        string body,
        IEnumerable<Guid> userIds,
        Guid? committeeId = null,
        Guid? meetingId = null,
        CancellationToken cancellationToken = default)
    {
        var distinctUserIds = userIds.Distinct().ToList();
        var notifications = distinctUserIds.Select(userId => new Notification
        {
            UserId = userId,
            CommitteeId = committeeId,
            MeetingId = meetingId,
            NotificationType = notificationType.ToString(),
            Title = title,
            Body = body,
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

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            _logger.LogInformation(
                "NOTIFICATION_SENT Id={NotificationId} Type={NotificationType} UserId={UserId} Title={Title}",
                notification.Id, notification.NotificationType, notification.UserId, notification.Title);
        }

        return notifications.Select(n => n.Id).ToList();
    }
}
