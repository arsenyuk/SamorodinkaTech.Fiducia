namespace SamorodinkaTech.Fiducia.Domain.Enums;

/// <summary>
/// Вид документооборота, применяемый юридическим лицом.
/// Выбор только электронного документооборота (без бумажного) невозможен
/// в связи с особенностями текущего законодательства.
/// </summary>
public enum DocumentFlowType
{
    /// <summary>Бумажный документооборот.</summary>
    Paper = 0,

    /// <summary>Смешанный документооборот (бумажный и ЮЗЭДО).</summary>
    Mixed = 1,

    /// <summary>Юридически значимый электронный документооборот (ЮЗЭДО).</summary>
    LegalElectronic = 2
}
