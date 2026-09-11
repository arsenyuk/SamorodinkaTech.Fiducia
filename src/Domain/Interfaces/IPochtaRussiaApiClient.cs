using SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Клиент API «Отправка» Почты России (otpravka-api.pochta.ru).
/// Предоставляет операции расчёта тарифов, нормализации адресов и создания заказов.
/// </summary>
public interface IPochtaRussiaApiClient
{
    /// <summary>
    /// Рассчитать стоимость пересылки (POST /1.0/tariff).
    /// </summary>
    /// <param name="request">Параметры отправления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Расчёт тарифов или null при ошибке.</returns>
    Task<PochtaRussiaRateResponse?> CalculateRateAsync(
        PochtaRussiaRateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Нормализовать адреса (POST /1.0/clean/address).
    /// </summary>
    /// <param name="addresses">Список адресов для нормализации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список нормализованных адресов.</returns>
    Task<List<PochtaRussiaAddressResponse>> NormalizeAddressesAsync(
        IReadOnlyList<PochtaRussiaAddressRequest> addresses,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать заказ на отправку (PUT /1.0/user/backlog).
    /// Гибридная отправка: электронное создание, печать и отправка в ОПС.
    /// </summary>
    /// <param name="order">Параметры заказа.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат создания заказа или null при ошибке.</returns>
    Task<PochtaRussiaOrderResponse?> CreateOrderAsync(
        PochtaRussiaOrderRequest order,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить текущее количество запросов по API (GET /1.0/settings/limit).
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Лимиты API или null при ошибке.</returns>
    Task<PochtaRussiaApiLimitResponse?> GetApiLimitAsync(
        CancellationToken cancellationToken = default);
}
