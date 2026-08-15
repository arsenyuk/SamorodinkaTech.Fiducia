namespace SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

/// <summary>
/// Информация о лицензии (праве) на осуществление деятельности на финансовом рынке.
/// Соответствует элементу LicInfo в ответе ЦБ РФ.
/// </summary>
public class CbrFinOrgLicense
{
    /// <summary>Код вида деятельности на финансовом рынке (VidID).</summary>
    public int VidId { get; set; }

    /// <summary>Наименование вида деятельности (VidD).</summary>
    public string? ActivityName { get; set; }

    /// <summary>Номер лицензии (LIC_Number).</summary>
    public string? Number { get; set; }

    /// <summary>Наименование лицензии (LIC_Name).</summary>
    public string? Name { get; set; }

    /// <summary>Дата начала действия лицензии (LIC_DTStart).</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Дата прекращения действия лицензии (LIC_DTEnd).</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Виды финансовых услуг (FinServices).</summary>
    public List<string> FinServices { get; set; } = new();
}
