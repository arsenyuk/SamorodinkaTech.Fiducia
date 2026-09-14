namespace SamorodinkaTech.Fiducia.Domain.Models.Sms;

/// <summary>
/// SMS send request parameters.
/// </summary>
public record SmsSendRequest
{
    /// <summary>Phone number in international format (7XXXXXXXXXX).</summary>
    public string PhoneNumber { get; init; } = "";

    /// <summary>Message text.</summary>
    public string Message { get; init; } = "";

    /// <summary>Sender name (up to 11 Latin chars or 15 digits). If null, uses default sender from config.</summary>
    public string? SenderName { get; init; }

    /// <summary>Scheduled send date in UTC. Null = send immediately.</summary>
    public DateTime? SendDateUtc { get; init; }

    /// <summary>Message lifetime in minutes. Default: 1440 (24 hours).</summary>
    public int ValidityMinutes { get; init; } = 1440;

    /// <summary>Callback URL for delivery status notifications.</summary>
    public string? CallbackUrl { get; init; }

    /// <summary>User-defined identifier returned via callback.</summary>
    public string? UserId { get; init; }

    /// <summary>Campaign name for statistics grouping.</summary>
    public string? CampaignName { get; init; }
}
