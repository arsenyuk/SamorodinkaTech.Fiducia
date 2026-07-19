namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Глобальные настройки Совета директоров (singleton-таблица, одна запись на систему).
/// Соответствует BDR‑007: не привязана к конкретному юридическому лицу.
/// </summary>
public class LegalEntityBoardSettings
{
    public Guid Id { get; set; }

    /// <summary>
    /// Дата начала законного окна проведения годового общего собрания акционеров (ГОСА).
    /// Для ПАО может быть сокращённое окно (в пределах 01.03–30.06) согласно уставу.
    /// Для АО/НАО фиксировано 01.03.
    /// </summary>
    public DateOnly? GosaWindowStart { get; set; }

    /// <summary>
    /// Дата окончания законного окна проведения ГОСА.
    /// Для ПАО может быть сокращённое окно (в пределах 01.03–30.06) согласно уставу.
    /// Для АО/НАО фиксировано 30.06.
    /// </summary>
    public DateOnly? GosaWindowEnd { get; set; }

    /// <summary>
    /// В организационной структуре предусмотрен Заместитель Председателя СД.
    /// </summary>
    public bool DeputyChairProvided { get; set; }

    /// <summary>
    /// В организационной структуре предусмотрен Секретарь СД.
    /// </summary>
    public bool SecretaryProvided { get; set; }

    /// <summary>
    /// При наличии Секретаря: протоколы СД подписываются Секретарём.
    /// </summary>
    public bool SecretarySignsProtocols { get; set; }
}
