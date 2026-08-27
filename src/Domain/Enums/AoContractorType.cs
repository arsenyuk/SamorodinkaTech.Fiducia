namespace SamorodinkaTech.Fiducia.Domain.Enums;

/// <summary>
/// Тип договора: регистратор, информационное агентство,
/// договор управления с ИП или с ЮЛ (ст. 42 14-ФЗ).
/// </summary>
public enum ContractType
{
    /// <summary>Регистратор — ведение реестра владельцев ценных бумаг.</summary>
    REGISTRAR = 0,

    /// <summary>Информационное агентство — раскрытие информации.</summary>
    INFO_AGENCY = 1,

    /// <summary>Договор управления с ИП-управляющим (ст. 42 14-ФЗ).</summary>
    MANAGEMENT_IP = 2,

    /// <summary>Договор управления с коммерческой организацией (ст. 42 14-ФЗ).</summary>
    MANAGEMENT_UL = 3
}
