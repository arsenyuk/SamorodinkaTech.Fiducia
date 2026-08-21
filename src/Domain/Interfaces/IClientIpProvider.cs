namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Провайдер IP-адреса текущего клиента.
/// Используется в декораторах аудита внешних интеграций.
/// </summary>
public interface IClientIpProvider
{
    /// <summary>
    /// Возвращает IP-адрес текущего клиента.
    /// </summary>
    string GetClientIp();
}
