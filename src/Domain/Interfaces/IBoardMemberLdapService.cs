using SamorodinkaTech.Fiducia.Domain.Models.Ldap;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис синхронизации состава Совета директоров из LDAP-каталога.
/// Используется при подготовке ОСА — чтение кандидатов из каталога
/// и контроль уникальности учётных записей.
/// </summary>
public interface IBoardMemberLdapService
{
    /// <summary>
    /// Возвращает список кандидатов в СД из группы «Совет директоров» LDAP-каталога.
    /// Каждый кандидат содержит предложенный тип директора на основе должности в LDAP.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task<IReadOnlyList<BoardMemberCandidate>> GetCandidatesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает данные кандидата из LDAP по логину.
    /// </summary>
    /// <param name="login">LDAP-логин (uid).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Кандидат или null, если не найден.</returns>
    Task<BoardMemberCandidate?> GetCandidateByLoginAsync(
        string login,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет, что LDAP-учётная запись не используется дважды в составе СД.
    /// </summary>
    /// <param name="existingLogins">Уже назначенные логины в текущем составе.</param>
    /// <param name="newLogin">Проверяемый логин.</param>
    /// <returns>true — дублирование обнаружено.</returns>
    bool IsDuplicate(IReadOnlySet<string> existingLogins, string newLogin);

    /// <summary>
    /// Находит конфликты уникальности в списке логинов.
    /// </summary>
    /// <param name="logins">Список логинов для проверки.</param>
    /// <returns>Логины, встречающиеся более одного раза.</returns>
    IReadOnlyList<string> FindDuplicates(IReadOnlyList<string> logins);
}
