using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.Extensions.Configuration;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Authentication;

public class ActiveDirectoryProvider : IAuthProvider
{
    private readonly IConfiguration _configuration;

    public string ProviderName => "ActiveDirectory";

    public ActiveDirectoryProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        var adSettings = _configuration.GetSection("Auth:ActiveDirectory");

        var server = adSettings["Server"] ?? "ldap://dc.company.local";
        var domain = adSettings["Domain"] ?? "company.local";
        var useSsl = bool.TryParse(adSettings["UseSsl"], out var ssl) && ssl;
        var port = useSsl ? 636 : 389;

        try
        {
            using var connection = new LdapConnection(new LdapDirectoryIdentifier(server, port));

            // Формируем credentials
            var credential = new NetworkCredential($"{username}@{domain}", password);
            connection.Credential = credential;
            connection.AuthType = AuthType.Negotiate;

            // Пробуем аутентифицироваться
            connection.Bind();

            // Если успешно — возвращаем результат
            return new AuthResult
            {
                Success = true,
                UserName = username,
                Claims = new Dictionary<string, string>
                {
                    ["role"] = "MEMBER_BOARD", // Маппинг из AD групп
                    ["auth_source"] = "ActiveDirectory"
                }
            };
        }
        catch (LdapException ex)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = ex.Message switch
                {
                    var msg when msg.Contains("invalid credentials") => "Неверный логин или пароль",
                    var msg when msg.Contains("Account locked") => "Учётная запись заблокирована",
                    var msg when msg.Contains("Account disabled") => "Учётная запись отключена",
                    _ => $"Ошибка аутентификации: {ex.Message}"
                }
            };
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Ошибка подключения к AD: {ex.Message}"
            };
        }
    }

    public async Task<List<UserInfo>> GetUsersAsync()
    {
        // LDAP-запрос для получения списка пользователей
        // Реализация зависит от структуры AD
        return await Task.FromResult(new List<UserInfo>());
    }
}
