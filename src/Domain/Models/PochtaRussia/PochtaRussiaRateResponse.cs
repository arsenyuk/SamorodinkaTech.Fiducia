namespace SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

/// <summary>
/// Ответ на запрос расчёта стоимости пересылки (POST /1.0/tariff).
/// Все суммы тарифов указываются в копейках.
/// </summary>
public class PochtaRussiaRateResponse
{
    /// <summary>Плата всего (копейки).</summary>
    public int TotalRate { get; init; }

    /// <summary>Всего НДС (копейки).</summary>
    public int TotalVat { get; init; }

    /// <summary>Плата за пересылку наземным транспортом.</summary>
    public PochtaRussiaTariff? GroundRate { get; init; }

    /// <summary>Плата за объявленную ценность.</summary>
    public PochtaRussiaTariff? InsuranceRate { get; init; }

    /// <summary>Надбавка за уведомление о вручении.</summary>
    public PochtaRussiaTariff? NoticeRate { get; init; }

    /// <summary>Плата за SMS уведомление получателю.</summary>
    public PochtaRussiaTariff? SmsNoticeRecipientRate { get; init; }

    /// <summary>Надбавка за отметку "Осторожно/Хрупкое/Терморежим".</summary>
    public PochtaRussiaTariff? FragileRate { get; init; }

    /// <summary>Плата за Авиа-пересылку.</summary>
    public PochtaRussiaTariff? AviaRate { get; init; }

    /// <summary>Надбавка за негабарит при весе более 10кг.</summary>
    public PochtaRussiaTariff? OversizeRate { get; init; }

    /// <summary>Плата за "Опись вложения".</summary>
    public PochtaRussiaTariff? InventoryRate { get; init; }

    /// <summary>Плата за "Возврат сопроводительных документов".</summary>
    public PochtaRussiaTariff? VsdRate { get; init; }

    /// <summary>Плата за "Проверку комплектности".</summary>
    public PochtaRussiaTariff? CompletenessCheckingRate { get; init; }

    /// <summary>Плата за "Проверку вложений".</summary>
    public PochtaRussiaTariff? ContentsCheckingRate { get; init; }

    /// <summary>Примерные сроки доставки.</summary>
    public PochtaRussiaDeliveryTime? DeliveryTime { get; init; }
}

/// <summary>
/// Тариф (става + НДС) в копейках.
/// </summary>
public class PochtaRussiaTariff
{
    /// <summary>Тариф без НДС (копейки).</summary>
    public int Rate { get; init; }

    /// <summary>НДС (копейки).</summary>
    public int? Vat { get; init; }
}

/// <summary>
/// Примерные сроки доставки (дни).
/// </summary>
public class PochtaRussiaDeliveryTime
{
    /// <summary>Максимальное время доставки (дни).</summary>
    public int MaxDays { get; init; }

    /// <summary>Минимальное время доставки (дни).</summary>
    public int? MinDays { get; init; }
}
