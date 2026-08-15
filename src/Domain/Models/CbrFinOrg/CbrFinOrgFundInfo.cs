namespace SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

/// <summary>
/// Информация об уставном капитале кредитной организации.
/// Соответствует элементу FundInfo в ответе ЦБ РФ.
/// </summary>
public class CbrFinOrgFundInfo
{
    /// <summary>Дата согласования последней редакции устава (APPROVAL_DATE).</summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>Дата согласования последних изменений в устав (CHANGE_DATE).</summary>
    public DateTime? ChangeDate { get; set; }

    /// <summary>Номер изменений устава (CHANGE_NUM).</summary>
    public int? ChangeNum { get; set; }

    /// <summary>Дата регистрации в уполномоченном органе (APPROVAL_REG_DATE).</summary>
    public DateTime? ApprovalRegDate { get; set; }

    /// <summary>Дата регистрации изменений устава (CHANGE_REG_DATE).</summary>
    public DateTime? ChangeRegDate { get; set; }

    /// <summary>Уставный капитал в рублях (FUND_VALUE).</summary>
    public decimal FundValue { get; set; }

    /// <summary>Дата изменения величины уставного капитала (FUND_CHANGE_DATE).</summary>
    public DateTime? FundChangeDate { get; set; }
}
