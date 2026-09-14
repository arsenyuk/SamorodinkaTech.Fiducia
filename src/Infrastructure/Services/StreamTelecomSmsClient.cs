using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Sms;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Stream Telecom SMS API client implementation.
/// REST API, POST method only.
/// Base URL: https://gateway.api.sc/rest/
/// </summary>
public class StreamTelecomSmsClient : ISmsApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<StreamTelecomSmsClient> _logger;
    private readonly string _login;
    private readonly string _password;
    private readonly string _defaultSender;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of <see cref="StreamTelecomSmsClient"/>.
    /// </summary>
    public StreamTelecomSmsClient(
        HttpClient httpClient,
        ILogger<StreamTelecomSmsClient> logger,
        string login,
        string password,
        string defaultSender)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _login = login ?? throw new ArgumentNullException(nameof(login));
        _password = password ?? throw new ArgumentNullException(nameof(password));
        _defaultSender = defaultSender ?? "";

        if (string.IsNullOrWhiteSpace(_login) || string.IsNullOrWhiteSpace(_password))
            _logger.LogWarning("Stream Telecom credentials not set — SMS integration disabled");
    }

    /// <inheritdoc />
    public async Task<SmsSendResponse?> SendSmsAsync(
        SmsSendRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_login) || string.IsNullOrWhiteSpace(_password))
            return null;

        if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Message))
        {
            _logger.LogWarning("Stream Telecom SendSms: phone or message is empty");
            return new SmsSendResponse
            {
                Success = false,
                ErrorCode = 2,
                ErrorMessage = "PhoneNumber and Message are required"
            };
        }

        var sender = string.IsNullOrWhiteSpace(request.SenderName)
            ? _defaultSender
            : request.SenderName;

        _logger.LogDebug("Stream Telecom SendSms: phone={Phone}, sender={Sender}, len={Len}",
            request.PhoneNumber, sender, request.Message.Length);

        try
        {
            var formData = new Dictionary<string, string>
            {
                ["login"] = _login,
                ["pass"] = _password,
                ["destinationAddress"] = request.PhoneNumber,
                ["data"] = request.Message
            };

            if (!string.IsNullOrWhiteSpace(sender))
                formData["sourceAddress"] = sender;

            if (request.SendDateUtc.HasValue)
                formData["sendDate"] = request.SendDateUtc.Value.ToString("yyyy-MM-ddTHH:mm:ss");

            if (request.ValidityMinutes != 1440)
                formData["validity"] = request.ValidityMinutes.ToString();

            if (!string.IsNullOrWhiteSpace(request.CallbackUrl))
                formData["callback_url"] = request.CallbackUrl;

            if (!string.IsNullOrWhiteSpace(request.UserId))
                formData["user_id"] = request.UserId;

            if (!string.IsNullOrWhiteSpace(request.CampaignName))
                formData["name_deliver"] = request.CampaignName;

            using var httpReq = new HttpRequestMessage(HttpMethod.Post, "/Send/SendSms/")
            {
                Content = new FormUrlEncodedContent(formData)
            };

            using var response = await _http.SendAsync(httpReq, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Stream Telecom SendSms returned {StatusCode}: {Body}",
                    (int)response.StatusCode, body);

                int? errorCode = null;
                string? errorMessage = null;

                if (int.TryParse(body.Trim('"'), out var code))
                {
                    errorCode = code;
                    errorMessage = GetErrorMessage(code);
                }

                return new SmsSendResponse
                {
                    Success = false,
                    ErrorCode = errorCode,
                    ErrorMessage = errorMessage ?? body
                };
            }

            // Response: ["371324579"] — array with message ID
            var messageIds = JsonSerializer.Deserialize<List<string>>(body, JsonOptions);
            var messageId = messageIds?.FirstOrDefault();

            _logger.LogDebug("Stream Telecom SendSms: messageId={MessageId}", messageId);

            return new SmsSendResponse
            {
                Success = true,
                MessageId = messageId
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stream Telecom SendSms error for phone={Phone}", request.PhoneNumber);
            return new SmsSendResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static string GetErrorMessage(int code) => code switch
    {
        1 => "ArgumentCanNotBeNullOrEmpty",
        2 => "InvalidArgument",
        3 => "InvalidSessionID",
        4 => "UnauthorizedAccess",
        5 => "NotEnoughCredits",
        6 => "InvalidOperation",
        7 => "Forbidden",
        8 => "GatewayError",
        9 => "InternalServerError",
        10 => "Flood SMS",
        _ => $"Unknown error ({code})"
    };
}
