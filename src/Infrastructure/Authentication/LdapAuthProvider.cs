using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Ldap;

namespace SamorodinkaTech.Fiducia.Infrastructure.Authentication;

/// <summary>
/// Провайдер аутентификации через LDAP/OpenLDAP-каталог с SSO.
/// Проверяет учётные данные через ILdapService и назначает роли
/// на основе членства в LDAP-группах:
///   cn=SysAdmins → SYS_ADMIN
///   cn=BoardOfDirectors → MEMBER_BOARD
/// При первом входе системного администратора создаёт/обновляет запись в users (upsert).
/// </summary>
public class LdapAuthProvider : IAuthProvider
{
    private readonly ILdapService _ldap;
    private readonly IApplicationDbContext _db;
    private readonly ILogger<LdapAuthProvider> _logger;
    private readonly string _sysAdminGroupDn;
    private readonly string _boardGroupDn;

    /// <summary>ID роли SYS_ADMIN (ref_roles).</summary>
    private static readonly Guid SysAdminRoleId = new("11111111-1111-1111-1111-111111111111");

    /// <summary>ID системного пользователя —.created_by для seed-данных.</summary>
    private static readonly Guid SystemUserId = new("00000000-0000-0000-0000-000000000000");

    public string ProviderName => "LDAP";

    /// <summary>
    /// Создаёт провайдер LDAP-аутентификации.
    /// </summary>
    public LdapAuthProvider(
        ILdapService ldap,
        IApplicationDbContext db,
        ILogger<LdapAuthProvider> logger,
        string sysAdminGroupDn,
        string boardGroupDn)
    {
        _ldap = ldap ?? throw new ArgumentNullException(nameof(ldap));
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sysAdminGroupDn = sysAdminGroupDn ?? throw new ArgumentNullException(nameof(sysAdminGroupDn));
        _boardGroupDn = boardGroupDn ?? throw new ArgumentNullException(nameof(boardGroupDn));
    }

    /// <inheritdoc />
    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        _logger.LogDebug("LDAP SSO: попытка входа {Username}", username);

        try
        {
            // 1. Проверка учётных данных через LDAP bind
            var authenticated = await _ldap.AuthenticateAsync(username, password);

            if (!authenticated)
            {
                _logger.LogWarning("LDAP SSO: неверные учётные данные для {Username}", username);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Неверный логин или пароль"
                };
            }

            // 2. Получаем данные пользователя из LDAP
            var ldapUser = await _ldap.FindUserByLoginAsync(username);

            if (ldapUser == null)
            {
                _logger.LogWarning("LDAP SSO: пользователь {Username} не найден в каталоге", username);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Пользователь не найден в каталоге"
                };
            }

            // 3. Определяем роль по членству в группах
            var role = ResolveRole(ldapUser.MemberOf);

            // 4. Ищем существующего User в БД по Login (= LDAP uid)
            var dbUser = await _db.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Login == username);

            // 5. Upsert системного администратора: создаём/обновляем запись в БД
            if (role == "SYS_ADMIN")
            {
                if (dbUser == null)
                {
                    dbUser = await CreateSysAdminFromLdapAsync(ldapUser);
                }
                else
                {
                    await UpdateSysAdminFromLdapAsync(dbUser, ldapUser);
                }
            }

            _logger.LogInformation(
                "LDAP SSO: вход выполнен {Username} ({DisplayName}), роль={RefRole}, userId={UserId}",
                username, ldapUser.DisplayName, role, dbUser?.Id);

            return new AuthResult
            {
                Success = true,
                UserId = dbUser?.Id,
                UserName = username,
                Login = username,
                Claims = new Dictionary<string, string>
                {
                    ["role"] = role,
                    ["display_name"] = ldapUser.DisplayName,
                    ["email"] = ldapUser.Email ?? string.Empty,
                    ["auth_source"] = "LDAP",
                    ["dn"] = ldapUser.DistinguishedName
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LDAP SSO: ошибка аутентификации {Username}", username);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Ошибка подключения к LDAP: {UnwrapException(ex)}"
            };
        }
    }

    /// <inheritdoc />
    public Task<List<UserInfo>> GetUsersAsync()
    {
        // LDAP не возвращает список пользователей через IAuthProvider —
        // используется ILdapService для операций с каталогом.
        return Task.FromResult(new List<UserInfo>());
    }

    private string ResolveRole(IReadOnlyList<string> memberOf)
    {
        // Приоритет: администратор > член СД > гость
        foreach (var group in memberOf)
        {
            if (group.Contains("SysAdmins", StringComparison.OrdinalIgnoreCase))
                return "SYS_ADMIN";
        }

        foreach (var group in memberOf)
        {
            if (group.Contains("BoardOfDirectors", StringComparison.OrdinalIgnoreCase))
                return "MEMBER_BOARD";
        }

        return "MEMBER_BOARD";
    }

    /// <summary>
    /// Создаёт запись системного администратора в БД из данных LDAP.
    /// </summary>
    private async Task<User> CreateSysAdminFromLdapAsync(LdapUser ldapUser)
    {
        var (lastName, firstName, middleName) = ParseDisplayName(ldapUser.DisplayName);

        var user = new User
        {
            Login = ldapUser.LoginName,
            LastName = lastName,
            FirstName = firstName,
            MiddleName = middleName,
            Email = ldapUser.Email ?? $"{ldapUser.LoginName}@fiducia.local",
            Phone = ldapUser.Phone ?? string.Empty,
            IsExternal = false,
            IsSystem = false,
            IsActive = ldapUser.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = SystemUserId,
            AccountExpiresAt = ldapUser.AccountExpiresAt,
            LdapCreatedAt = ldapUser.LdapCreatedAt,
            MpiMasterId = ldapUser.MpiMasterId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Привязка роли SYS_ADMIN
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = SysAdminRoleId
        };

        _db.UserRoles.Add(userRole);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "LDAP SSO: создан sysadmin {Login} (UserId={UserId})",
            ldapUser.LoginName, user.Id);

        return user;
    }

    /// <summary>
    /// Обновляет данные системного администратора из LDAP (только изменённые поля).
    /// </summary>
    private async Task UpdateSysAdminFromLdapAsync(User dbUser, LdapUser ldapUser)
    {
        var (lastName, firstName, middleName) = ParseDisplayName(ldapUser.DisplayName);
        var changed = false;

        if (dbUser.LastName != lastName)
        {
            dbUser.LastName = lastName;
            changed = true;
        }

        if (dbUser.FirstName != firstName)
        {
            dbUser.FirstName = firstName;
            changed = true;
        }

        if (dbUser.MiddleName != middleName)
        {
            dbUser.MiddleName = middleName;
            changed = true;
        }

        if (dbUser.Email != (ldapUser.Email ?? $"{ldapUser.LoginName}@fiducia.local"))
        {
            dbUser.Email = ldapUser.Email ?? $"{ldapUser.LoginName}@fiducia.local";
            changed = true;
        }

        if (dbUser.IsActive != ldapUser.IsActive)
        {
            dbUser.IsActive = ldapUser.IsActive;
            changed = true;
        }

        // Убедимся, что роль SYS_ADMIN назначена
        var hasSysAdminRole = dbUser.UserRoles.Any(ur => ur.RoleId == SysAdminRoleId);
        if (!hasSysAdminRole)
        {
            _db.UserRoles.Add(new UserRole
            {
                UserId = dbUser.Id,
                RoleId = SysAdminRoleId
            });
            changed = true;
        }

        if (changed)
        {
            await _db.SaveChangesAsync();
            _logger.LogInformation(
                "LDAP SSO: обновлён sysadmin {Login} (UserId={UserId})",
                ldapUser.LoginName, dbUser.Id);
        }
    }

    /// <summary>
    /// Парсит DisplayName (ФИО) на составные части.
    /// Поддерживает форматы: "Фамилия Имя Отчество" / "Фамилия Имя" / "Фамилия".
    /// </summary>
    private static (string LastName, string FirstName, string MiddleName) ParseDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return ("Неизвестно", "Пользователь", string.Empty);

        var parts = displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length switch
        {
            >= 3 => (parts[0], parts[1], parts[2]),
            2 => (parts[0], parts[1], string.Empty),
            _ => (parts[0], string.Empty, string.Empty)
        };
    }

    private static string UnwrapException(Exception ex)
    {
        var inner = ex;
        while (inner.InnerException != null)
            inner = inner.InnerException;

        return inner.Message;
    }
}
