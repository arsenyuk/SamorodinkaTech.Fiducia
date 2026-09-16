using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Email;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация IEmailService через SMTP (MailKit).
/// Подключается к SMTP-серверу при каждом вызове, не хранит persistent connection.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    /// <summary>
    /// Создаёт экземпляр SMTP-клиента.
    /// </summary>
    /// <param name="options">Настройки SMTP.</param>
    /// <param name="logger">Логгер.</param>
    public SmtpEmailService(
        SmtpOptions options,
        ILogger<SmtpEmailService> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var toAddress = message.To;

        if (!string.IsNullOrWhiteSpace(_options.DevOverrideTo))
        {
            _logger.LogInformation(
                "EMAIL_DEV_OVERRIDE OriginalTo={OriginalTo} OverrideTo={OverrideTo}",
                message.To, _options.DevOverrideTo);
            toAddress = _options.DevOverrideTo;
        }

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(MailboxAddress.Parse(_options.From));
        mimeMessage.To.Add(MailboxAddress.Parse(toAddress));
        mimeMessage.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody
        };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = new SmtpClient();
        try
        {
            var secureSocketOptions = _options.UseSsl
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.None;

            await smtpClient.ConnectAsync(_options.Host, _options.Port, secureSocketOptions, cancellationToken);
            if (!string.IsNullOrWhiteSpace(_options.User))
                await smtpClient.AuthenticateAsync(_options.User, _options.Password, cancellationToken);
            await smtpClient.SendAsync(mimeMessage, cancellationToken);
            await smtpClient.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "EMAIL_SENT To={To} Subject={Subject}",
                toAddress, message.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "EMAIL_FAILED To={To} Subject={Subject} Error={Error}",
                toAddress, message.Subject, ex.Message);
        }
    }
}
