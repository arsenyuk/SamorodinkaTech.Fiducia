namespace SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

/// <summary>
/// Запрос на нормализацию адреса (POST /1.0/clean/address).
/// </summary>
public class PochtaRussiaAddressRequest
{
    /// <summary>Идентификатор записи (для связи запрос-ответ).</summary>
    public string Id { get; init; } = default!;

    /// <summary>Оригинальный адрес одной строкой.</summary>
    public string OriginalAddress { get; init; } = default!;
}
