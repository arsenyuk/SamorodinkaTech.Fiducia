namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис управления сессиями пользователей (УПД.15).
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Генерирует JWT-токен для аутентифицированного пользователя.
    /// </summary>
    string GenerateToken(Guid userId, string role);

    /// <summary>
    /// Валидирует JWT-токен и возвращает ID пользователя.
    /// </summary>
    int? ValidateToken(string token);

    /// <summary>
    /// Возвращает период бездействия (в минутах) до принудительного выхода.
    /// </summary>
    int GetIdleTimeoutMinutes();
}
