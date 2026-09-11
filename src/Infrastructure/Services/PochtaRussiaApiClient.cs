using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Клиент API «Отправка» Почты России (otpravka-api.pochta.ru).
/// REST API с JSON-обменом данными.
/// Аутентификация: AccessToken + Basic (login:password).
/// </summary>
public class PochtaRussiaApiClient : IPochtaRussiaApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<PochtaRussiaApiClient> _logger;
    private readonly string _accessToken;
    private readonly string _basicAuth;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Создаёт экземпляр клиента API Почты России.
    /// </summary>
    /// <param name="httpClient">HttpClient (BaseAddress = URL API).</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="accessToken">Токен авторизации приложения.</param>
    /// <param name="login">Логин пользователя.</param>
    /// <param name="password">Пароль пользователя.</param>
    public PochtaRussiaApiClient(
        HttpClient httpClient,
        ILogger<PochtaRussiaApiClient> logger,
        string accessToken,
        string login,
        string password)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));

        if (string.IsNullOrWhiteSpace(_accessToken))
            _logger.LogWarning("Токен Почты России не задан — интеграция отключена");

        _basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}"));
    }

    /// <inheritdoc />
    public async Task<PochtaRussiaRateResponse?> CalculateRateAsync(
        PochtaRussiaRateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
            return null;

        _logger.LogDebug("Запрос расчёта тарифа Почты России: MailType={MailType}, Mass={Mass}",
            request.MailType, request.Mass);

        var requestBody = new
        {
            mail_type = request.MailType,
            mail_category = request.MailCategory,
            mass = request.Mass,
            index_from = request.IndexFrom,
            index_to = request.IndexTo,
            mail_direct = request.MailDirect,
            declared_value = request.DeclaredValue,
            payment_method = request.PaymentMethod,
            transport_type = request.TransportType,
            dimension = request.Dimension is not null ? new
            {
                height = request.Dimension.Height,
                length = request.Dimension.Length,
                width = request.Dimension.Width
            } : null,
            dimension_type = request.DimensionType,
            fragile = request.Fragile,
            courier = request.Courier,
            inventory = request.Inventory,
            with_order_of_notice = request.WithOrderOfNotice,
            with_simple_notice = request.WithSimpleNotice,
            with_electronic_notice = request.WithElectronicNotice,
            sms_notice_recipient = request.SmsNoticeRecipient,
            vsd = request.Vsd,
            completeness_checking = request.CompletenessChecking,
            contents_checking = request.ContentsChecking,
            entries_type = request.EntriesType
        };

        try
        {
            var response = await PostJsonAsync<PochtaRussiaRateResponse>("/1.0/tariff", requestBody, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка HTTP POST /1.0/tariff");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<PochtaRussiaAddressResponse>> NormalizeAddressesAsync(
        IReadOnlyList<PochtaRussiaAddressRequest> addresses,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
            return new List<PochtaRussiaAddressResponse>();

        _logger.LogDebug("Запрос нормализации адресов Почты России: {Count} адресов", addresses.Count);

        var requestBody = addresses.Select(a => new
        {
            id = a.Id,
            original_address = a.OriginalAddress
        }).ToList();

        try
        {
            var response = await PostJsonAsync<List<PochtaRussiaAddressResponse>>("/1.0/clean/address", requestBody, cancellationToken);
            return response ?? new List<PochtaRussiaAddressResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка HTTP POST /1.0/clean/address");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PochtaRussiaOrderResponse?> CreateOrderAsync(
        PochtaRussiaOrderRequest order,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
            return null;

        _logger.LogDebug("Запрос создания заказа Почты России: OrderNum={OrderNum}, MailType={MailType}",
            order.OrderNum, order.MailType);

        var requestBody = new[]
        {
            new
            {
                order_num = order.OrderNum,
                mail_type = order.MailType,
                mail_category = order.MailCategory,
                mail_direct = order.MailDirect,
                mass = order.Mass,
                index_to = order.IndexTo,
                recipient_name = order.RecipientName,
                given_name = order.GivenName,
                surname = order.Surname,
                middle_name = order.MiddleName,
                place_to = order.PlaceTo,
                region_to = order.RegionTo,
                street_to = order.StreetTo,
                house_to = order.HouseTo,
                postoffice_code = order.PostofficeCode,
                room_to = order.RoomTo,
                insr_value = order.InsrValue,
                with_order_of_notice = order.WithOrderOfNotice,
                with_simple_notice = order.WithSimpleNotice,
                with_electronic_notice = order.WithElectronicNotice,
                sms_notice_recipient = order.SmsNoticeRecipient,
                inventory = order.Inventory,
                vsd = order.Vsd,
                payment_method = order.PaymentMethod,
                tel_address = order.TelAddress,
                tariff_count = order.TariffCount,
                dimension = order.Dimension is not null ? new
                {
                    height = order.Dimension.Height,
                    length = order.Dimension.Length,
                    width = order.Dimension.Width
                } : null,
                fragile = order.Fragile,
                raw_address = order.RawAddress,
                sender_name = order.SenderName,
                sender_comment = order.SenderComment,
                inner_num = order.InnerNum
            }
        };

        try
        {
            var response = await PutJsonAsync<PochtaRussiaOrderResponse>("/1.0/user/backlog", requestBody, cancellationToken);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка HTTP PUT /1.0/user/backlog");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<PochtaRussiaApiLimitResponse?> GetApiLimitAsync(
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
            return null;

        _logger.LogDebug("Запрос лимитов API Почты России");

        try
        {
            var url = "/1.0/settings/limit";
            var request = CreateRequest(HttpMethod.Get, url);
            var response = await _http.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("HTTP GET {Url} вернул {StatusCode}: {Body}",
                    url, (int)response.StatusCode, body);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PochtaRussiaApiLimitResponse>(
                JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ошибка HTTP GET /1.0/settings/limit");
            throw;
        }
    }

    // ── Вспомогательные методы ───────────────────────────────────

    private async Task<T?> PostJsonAsync<T>(string url, object requestBody, CancellationToken ct)
    {
        var request = CreateRequest(HttpMethod.Post, url);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("HTTP POST {Url} вернул {StatusCode}: {Body}",
                url, (int)response.StatusCode, body);
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    private async Task<T?> PutJsonAsync<T>(string url, object requestBody, CancellationToken ct)
    {
        var request = CreateRequest(HttpMethod.Put, url);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody, JsonOptions),
            Encoding.UTF8,
            "application/json");

        var response = await _http.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("HTTP PUT {Url} вернул {StatusCode}: {Body}",
                url, (int)response.StatusCode, body);
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Authorization", $"AccessToken {_accessToken}");
        request.Headers.Add("X-User-Authorization", $"Basic {_basicAuth}");
        request.Headers.Add("Accept", "application/json;charset=UTF-8");
        return request;
    }
}
