using SamorodinkaTech.Fiducia.Domain.Models.Email;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис отправки email-писем через SMTP.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Отправляет email-письмо.
    /// </summary>
    /// <param name="message">Данные письма (получатель, тема, тело).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
