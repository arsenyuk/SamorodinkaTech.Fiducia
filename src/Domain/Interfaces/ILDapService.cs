namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

using SamorodinkaTech.Fiducia.Domain.Models.Ldap;

/// <summary>
/// Сервис для работы с корпоративным LDAP/AD-каталогом.
/// Обеспечивает поиск пользователей, проверку членства в группах
/// и синхронизацию состава совета директоров.
/// </summary>
public interface ILdapService
{
    /// <summary>
    /// Проверяет учётные данные пользователя в каталоге (bind).
    /// </summary>
    /// <param name="login">Логин (sAMAccountName / uid).</param>
    /// <param name="password">Пароль.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>true, если аутентификация успешна.</returns>
    Task<bool> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ищет пользователя по логину.
    /// </summary>
    /// <param name="login">Логин (sAMAccountName / uid).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Пользователь или null, если не найден.</returns>
    Task<LdapUser?> FindUserByLoginAsync(
        string login,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ищет пользователей по атрибутам.
    /// </summary>
    /// <param name="filter">LDAP-фильтр (например, "(department=Board)").</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список найденных пользователей.</returns>
    Task<IReadOnlyList<LdapUser>> SearchUsersAsync(
        string filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает список пользователей — членов указанной группы.
    /// </summary>
    /// <param name="groupDn">DN группы.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список членов группы.</returns>
    Task<IReadOnlyList<LdapUser>> GetGroupMembersAsync(
        string groupDn,
        CancellationToken cancellationToken = default);
}
