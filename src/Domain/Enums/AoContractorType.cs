namespace SamorodinkaTech.Fiducia.Domain.Enums;

/// <summary>
/// Тип контрагента АО: регистратор (ведёт реестр акционеров)
/// или информационное агентство (раскрытие информации).
/// </summary>
public enum AoContractorType
{
    /// <summary>Регистратор — ведение реестра владельцев ценных бумаг.</summary>
    REGISTRAR = 0,

    /// <summary>Информационное агентство — раскрытие информации.</summary>
    INFO_AGENCY = 1
}
