using System.Net.Http.Headers;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Делегирующий обработчик: автоматически добавляет JWT-токен из cookie
/// в заголовок Authorization всех HTTP-запросов (для loopback Blazor Server).
/// </summary>
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["SessionToken"];
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
