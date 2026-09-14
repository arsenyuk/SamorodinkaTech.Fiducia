using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.DaData;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// DaData API client implementation.
/// Provides organization data from EGRUL (up to 3 days delay).
/// </summary>
public class DaDataApiClient : IDaDataApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<DaDataApiClient> _logger;
    private readonly string _apiKey;
    private readonly string _secretKey;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of <see cref="DaDataApiClient"/>.
    /// </summary>
    public DaDataApiClient(
        HttpClient httpClient,
        ILogger<DaDataApiClient> logger,
        string apiKey,
        string secretKey)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));

        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_secretKey))
            _logger.LogWarning("DaData API key/secret not set — integration disabled");
    }

    /// <inheritdoc />
    public async Task<DaDataCompanyInfo?> FindCompanyAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_secretKey))
            return null;

        if (string.IsNullOrWhiteSpace(query))
            return null;

        _logger.LogDebug("DaData FindCompany: query={Query}", query);

        try
        {
            var request = new { query };
            using var httpReq = new HttpRequestMessage(HttpMethod.Post, "/v2/finda/org")
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            httpReq.Headers.Add("Authorization", $"Token {_apiKey}");
            httpReq.Headers.Add("X-Secret", _secretKey);

            using var response = await _http.SendAsync(httpReq, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("DaData FindCompany returned {StatusCode}: {Body}",
                    (int)response.StatusCode, body);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<DaDataFindaResponse>(
                JsonOptions, cancellationToken);

            var first = result?.Suggestions?.FirstOrDefault();
            if (first?.Data is null)
                return null;

            var d = first.Data;
            return new DaDataCompanyInfo
            {
                Inn = d.Inn,
                Ogrn = d.Ogrn,
                Kpp = d.Kpp,
                Name = d.Name?.Full,
                ShortName = d.Name?.Short,
                Okved = d.Okved,
                OkvedName = d.OkvedName,
                Address = d.Address?.UnrestrictedValue,
                Status = d.Status,
                RegistrationDate = d.RegistrationDate,
                EgrulUpdatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DaData FindCompany error for query={Query}", query);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<DaDataBalance?> GetBalanceAsync(
        string inn,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_secretKey))
            return null;

        if (string.IsNullOrWhiteSpace(inn))
            return null;

        _logger.LogDebug("DaData GetBalance: inn={Inn}", inn);

        try
        {
            var request = new { query = inn };
            using var httpReq = new HttpRequestMessage(HttpMethod.Post, "/v2/balance")
            {
                Content = JsonContent.Create(request, options: JsonOptions)
            };
            httpReq.Headers.Add("Authorization", $"Token {_apiKey}");
            httpReq.Headers.Add("X-Secret", _secretKey);

            using var response = await _http.SendAsync(httpReq, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("DaData GetBalance returned {StatusCode}: {Body}",
                    (int)response.StatusCode, body);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<DaDataBalanceResponse>(
                JsonOptions, cancellationToken);

            var first = result?.Suggestions?.FirstOrDefault();
            if (first?.Data is null)
                return null;

            var d = first.Data;
            return new DaDataBalance
            {
                Inn = inn,
                Assets = d.Assets,
                Equity = d.Equity,
                Revenue = d.Revenue,
                NetProfit = d.NetProfit,
                CurrentAssets = d.CurrentAssets,
                FixedAssets = d.FixedAssets,
                AccountsReceivable = d.AccountsReceivable,
                AccountsPayable = d.AccountsPayable,
                NetLoss = d.NetLoss,
                Year = d.Year,
                EgrulUpdatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DaData GetBalance error for inn={Inn}", inn);
            return null;
        }
    }

    // ── Internal DTOs for JSON deserialization ──────────────────────

    private sealed class DaDataFindaResponse
    {
        public List<DaDataFindaSuggestion>? Suggestions { get; set; }
    }

    private sealed class DaDataFindaSuggestion
    {
        public DaDataFindaData? Data { get; set; }
    }

    private sealed class DaDataFindaData
    {
        public string? Inn { get; set; }
        public string? Ogrn { get; set; }
        public string? Kpp { get; set; }
        public DaDataName? Name { get; set; }
        public string? Okved { get; set; }
        public string? OkvedName { get; set; }
        public DaDataAddressDto? Address { get; set; }
        public string? Status { get; set; }
        public DateTime? RegistrationDate { get; set; }
    }

    private sealed class DaDataName
    {
        public string? Full { get; set; }
        public string? Short { get; set; }
    }

    private sealed class DaDataAddressDto
    {
        public string? UnrestrictedValue { get; set; }
    }

    private sealed class DaDataBalanceResponse
    {
        public List<DaDataBalanceSuggestion>? Suggestions { get; set; }
    }

    private sealed class DaDataBalanceSuggestion
    {
        public DaDataBalanceData? Data { get; set; }
    }

    private sealed class DaDataBalanceData
    {
        public decimal? Assets { get; set; }
        public decimal? Equity { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? NetProfit { get; set; }
        public decimal? CurrentAssets { get; set; }
        public decimal? FixedAssets { get; set; }
        public decimal? AccountsReceivable { get; set; }
        public decimal? AccountsPayable { get; set; }
        public decimal? NetLoss { get; set; }
        public int? Year { get; set; }
    }
}
