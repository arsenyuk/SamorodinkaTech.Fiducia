using SamorodinkaTech.Fiducia.Domain.Models.Sms;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Client for SMS sending via Stream Telecom Platform (stream-telecom.ru).
/// REST API, POST method only.
/// </summary>
public interface ISmsApiClient
{
    /// <summary>
    /// Send a single SMS message.
    /// </summary>
    /// <param name="request">SMS send parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Send result with message ID or error.</returns>
    Task<SmsSendResponse?> SendSmsAsync(
        SmsSendRequest request,
        CancellationToken cancellationToken = default);
}
