namespace SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;

/// <summary>
/// Запрос на создание заказа (PUT /1.0/user/backlog).
/// Гибридная отправка: электронное создание заказа, печать и отправка в ОПС.
/// </summary>
public class PochtaRussiaOrderRequest
{
    /// <summary>Внешний идентификатор заказа (формируется отправителем).</summary>
    public string OrderNum { get; init; } = default!;

    /// <summary>Вид РПО: LETTER, LETTER_CLASS_1, POSTAL_PARCEL, BANDEROL, EMS.</summary>
    public string MailType { get; init; } = default!;

    /// <summary>Категория РПО: SIMPLE, REGISTERED, ORDERED, ORDINARY, ECM, EMS.</summary>
    public string MailCategory { get; init; } = default!;

    /// <summary>Код страны назначения (643 = Россия).</summary>
    public int MailDirect { get; init; }

    /// <summary>Масса отправления в граммах.</summary>
    public int Mass { get; init; }

    /// <summary>Почтовый индекс назначения.</summary>
    public int IndexTo { get; init; }

    /// <summary>ФИО/наименование получателя одной строкой.</summary>
    public string RecipientName { get; init; } = default!;

    /// <summary>Имя получателя.</summary>
    public string GivenName { get; init; } = default!;

    /// <summary>Фамилия получателя.</summary>
    public string Surname { get; init; } = default!;

    /// <summary>Отчество получателя.</summary>
    public string? MiddleName { get; init; }

    /// <summary>Населённый пункт.</summary>
    public string PlaceTo { get; init; } = default!;

    /// <summary>Область, регион.</summary>
    public string RegionTo { get; init; } = default!;

    /// <summary>Улица.</summary>
    public string StreetTo { get; init; } = default!;

    /// <summary>Номер здания.</summary>
    public string HouseTo { get; init; } = default!;

    /// <summary>Индекс ОПС места приёма.</summary>
    public string PostofficeCode { get; init; } = default!;

    /// <summary>Помещение (комната, офис).</summary>
    public string? RoomTo { get; init; }

    /// <summary>Объявленная ценность в копейках.</summary>
    public int? InsrValue { get; init; }

    /// <summary>Отметка "С заказным уведомлением".</summary>
    public bool? WithOrderOfNotice { get; init; }

    /// <summary>Отметка "С простым уведомлением".</summary>
    public bool? WithSimpleNotice { get; init; }

    /// <summary>Отметка "С электронным уведомлением".</summary>
    public bool? WithElectronicNotice { get; init; }

    /// <summary>Признак услуги SMS уведомления.</summary>
    public int? SmsNoticeRecipient { get; init; }

    /// <summary>Опись вложения.</summary>
    public bool? Inventory { get; init; }

    /// <summary>Возврат сопроводительных документов.</summary>
    public bool? Vsd { get; init; }

    /// <summary>Способ оплаты: CASH, CASHLESS.</summary>
    public string? PaymentMethod { get; init; }

    /// <summary>Телефон получателя.</summary>
    public long? TelAddress { get; init; }

    /// <summary>Количество тарифов: 1 = полный, 2 = полный + скидочный.</summary>
    public int? TariffCount { get; init; }

    /// <summary>Линейные размеры отправления.</summary>
    public PochtaRussiaDimension? Dimension { get; init; }

    /// <summary>Отметка "Осторожно/Хрупкое/Терморежим".</summary>
    public bool? Fragile { get; init; }

    /// <summary>Необработанный адрес получателя.</summary>
    public string? RawAddress { get; init; }

    /// <summary>Наименование отправителя одной строкой.</summary>
    public string? SenderName { get; init; }

    /// <summary>Комментарий отправителя.</summary>
    public string? SenderComment { get; init; }

    /// <summary>Дополнительный идентификатор отправления.</summary>
    public string? InnerNum { get; init; }
}
