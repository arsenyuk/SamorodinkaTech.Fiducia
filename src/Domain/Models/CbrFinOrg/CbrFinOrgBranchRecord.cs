namespace SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

/// <summary>
/// Информация о филиале (подразделении) кредитной организации.
/// Соответствует элементу BranchRecord в ответе GetBranchesInfoByID.
/// </summary>
public class CbrFinOrgBranchRecord
{
    /// <summary>Уникальный идентификатор филиала (Id).</summary>
    public long Id { get; set; }

    /// <summary>Номер филиала / дополнительного офиса (Num).</summary>
    public string? Number { get; set; }

    /// <summary>Наименование подразделения (Name).</summary>
    public string? Name { get; set; }

    /// <summary>Тип подразделения (BranchType).</summary>
    public string? BranchType { get; set; }

    /// <summary>Фактический адрес (Address).</summary>
    public string? Address { get; set; }

    /// <summary>Дата открытия (OpenDate).</summary>
    public DateTime OpenDate { get; set; }

    /// <summary>Принадлежность (Affiliation).</summary>
    public string? Affiliation { get; set; }

    /// <summary>Имеет подчинённые структуры (HasChild).</summary>
    public bool HasChild { get; set; }
}
