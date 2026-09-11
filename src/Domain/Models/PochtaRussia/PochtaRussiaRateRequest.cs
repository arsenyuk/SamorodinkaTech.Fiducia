namespace SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

/// <summary>
/// Запрос на расчёт стоимости пересылки (POST /1.0/tariff).
/// </summary>
public class PochtaRussiaRateRequest
{
    /// <summary>Вид РПО: LETTER, LETTER_CLASS_1, POSTAL_PARCEL, BANDEROL, EMS и др.</summary>
    public string MailType { get; init; } = default!;

    /// <summary>Категория РПО: SIMPLE, REGISTERED, ORDERED, ORDINARY, ECM, EMS.</summary>
    public string MailCategory { get; init; } = default!;

    /// <summary>Масса отправления в граммах.</summary>
    public int Mass { get; init; }

    /// <summary>Почтовый индекс ОПС отправления (из профиля, если не задан).</summary>
    public string? IndexFrom { get; init; }

    /// <summary>Почтовый индекс ОПС назначения.</summary>
    public string? IndexTo { get; init; }

    /// <summary>Код страны назначения (643 = Россия).</summary>
    public int? MailDirect { get; init; }

    /// <summary>Объявленная ценность в копейках.</summary>
    public int? DeclaredValue { get; init; }

    /// <summary>Способ оплаты: CASH, CASHLESS.</summary>
    public string? PaymentMethod { get; init; }

    /// <summary>Вид транспортировки: SURFACE, AVIA.</summary>
    public string? TransportType { get; init; }

    /// <summary>Линейные размеры отправления.</summary>
    public PochtaRussiaDimension? Dimension { get; init; }

    /// <summary>Типоразмер (вместо dimension).</summary>
    public string? DimensionType { get; init; }

    /// <summary>Отметка "Хрупкое".</summary>
    public bool? Fragile { get; init; }

    /// <summary>Отметка "Курьер".</summary>
    public bool? Courier { get; init; }

    /// <summary>Опись вложения.</summary>
    public bool? Inventory { get; init; }

    /// <summary>Отметка "С заказным уведомлением".</summary>
    public bool? WithOrderOfNotice { get; init; }

    /// <summary>Отметка "С простым уведомлением".</summary>
    public bool? WithSimpleNotice { get; init; }

    /// <summary>Отметка "С электронным уведомлением".</summary>
    public bool? WithElectronicNotice { get; init; }

    /// <summary>Признак услуги SMS уведомления.</summary>
    public int? SmsNoticeRecipient { get; init; }

    /// <summary>Возврат сопроводительных документов.</summary>
    public bool? Vsd { get; init; }

    /// <summary>Признак услуги проверки комплектности.</summary>
    public bool? CompletenessChecking { get; init; }

    /// <summary>Признак услуги проверки вложения.</summary>
    public bool? ContentsChecking { get; init; }

    /// <summary>Категория вложения (для международных отправлений).</summary>
    public string? EntriesType { get; init; }
}
