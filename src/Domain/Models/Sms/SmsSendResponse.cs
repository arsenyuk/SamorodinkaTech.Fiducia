namespace SamorodinkaTech.Fiducia.Domain.Models.Sms;

/// <summary>
/// SMS send response from Stream Telecom API.
/// </summary>
public record SmsSendResponse
{
    /// <summary>Whether the send was successful.</summary>
    public bool Success { get; init; }

    /// <summary>Message ID assigned by the platform (returned on success).</summary>
    public string? MessageId { get; init; }

    /// <summary>Error code from the API (null on success).</summary>
    public int? ErrorCode { get; init; }

    /// <summary>Error description from the API (null on success).</summary>
    public string? ErrorMessage { get; init; }
}
