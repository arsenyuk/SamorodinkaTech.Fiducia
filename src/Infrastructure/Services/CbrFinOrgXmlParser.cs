using System.Xml.Linq;
using SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Парсинг XML-ответов SOAP-сервиса ЦБ РФ (FinOrg.asmx) в доменные DTO.
/// Статический внутренний класс — парсинг без внешних зависимостей.
/// </summary>
internal static class CbrFinOrgXmlParser
{
    /// <summary>
    /// Парсит элемент FullInfo (ответ GetFullInfoByINN / GetFullInfoByOGRN / GetFullInfoByID).
    /// </summary>
    public static CbrFinOrgOrganization? ParseFullInfo(XElement? element)
    {
        if (element is null)
            return null;

        var org = new CbrFinOrgOrganization
        {
            Id = GetLong(element, "ID"),
            Ogrn = GetNullableLong(element, "OGRN"),
            Inn = GetString(element, "INN"),
            ShortName = GetString(element, "ShortName"),
            Name = GetString(element, "Name"),
            EngName = GetString(element, "EngName"),
            Address = GetString(element, "Address"),
            Phones = GetString(element, "Phones"),
            Email = GetString(element, "Email"),
            Okato = GetNullableInt(element, "OKATO"),
            Region = GetString(element, "Reg"),
            Status = GetString(element, "Status") ?? "",
            IsSroMember = GetBool(element, "IsSroMember"),
            IsRss = GetBool(element, "IsRss"),
            IsNpo = GetBool(element, "NPO_FLG"),
            IsAsv = GetBool(element, "ASV_FLG"),
            RegNumber = GetNullableInt(element, "REGNUM"),
            Bic = GetString(element, "BIC"),
            BankStatus = GetString(element, "BnkStatus"),
            RegistrationDate = GetNullableDateTime(element, "RegistrationDate"),
            HasBranches = GetBool(element, "HasBranches"),
            Error = GetString(element, "Error")
        };

        // FOTypes (список строк)
        var foTypesElement = element.Element("FOTypes");
        if (foTypesElement is not null)
            org.FoTypes = foTypesElement.Elements("string")
                .Select(e => e.Value)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

        // Fund (уставный капитал)
        var fundElement = element.Element("Fund");
        if (fundElement is not null)
            org.Fund = ParseFundInfo(fundElement);

        // LicList (лицензии)
        var licListElement = element.Element("LicList");
        if (licListElement is not null)
            org.Licenses = licListElement.Elements("LicInfo")
                .Select(ParseLicense)
                .Where(l => l is not null)
                .Cast<CbrFinOrgLicense>()
                .ToList();

        // WebSites
        var webSitesElement = element.Element("WebSites");
        if (webSitesElement is not null)
            org.WebSites = webSitesElement.Elements("string")
                .Select(e => e.Value)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

        return org;
    }

    /// <summary>
    /// Парсит элемент LicInfo (информация о лицензии).
    /// </summary>
    public static CbrFinOrgLicense? ParseLicense(XElement? element)
    {
        if (element is null)
            return null;

        var lic = new CbrFinOrgLicense
        {
            VidId = GetInt(element, "VidID"),
            ActivityName = GetString(element, "VidD"),
            Number = GetString(element, "LIC_Number"),
            Name = GetString(element, "LIC_Name"),
            StartDate = GetNullableDateTime(element, "LIC_DTStart"),
            EndDate = GetNullableDateTime(element, "LIC_DTEnd")
        };

        var finServicesElement = element.Element("FinServices");
        if (finServicesElement is not null)
            lic.FinServices = finServicesElement.Elements("FinService")
                .Select(e => e.Value)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

        return lic;
    }

    /// <summary>
    /// Парсит элемент FundInfo (уставный капитал).
    /// </summary>
    public static CbrFinOrgFundInfo? ParseFundInfo(XElement? element)
    {
        if (element is null)
            return null;

        return new CbrFinOrgFundInfo
        {
            ApprovalDate = GetNullableDateTime(element, "APPROVAL_DATE"),
            ChangeDate = GetNullableDateTime(element, "CHANGE_DATE"),
            ChangeNum = GetNullableInt(element, "CHANGE_NUM"),
            ApprovalRegDate = GetNullableDateTime(element, "APPROVAL_REG_DATE"),
            ChangeRegDate = GetNullableDateTime(element, "CHANGE_REG_DATE"),
            FundValue = GetDecimal(element, "FUND_VALUE"),
            FundChangeDate = GetNullableDateTime(element, "FUND_CHANGE_DATE")
        };
    }

    /// <summary>
    /// Парсит элемент Record (краткая запись из результата поиска).
    /// </summary>
    public static CbrFinOrgRecord? ParseRecord(XElement? element)
    {
        if (element is null)
            return null;

        return new CbrFinOrgRecord
        {
            Id = GetLong(element, "Id"),
            Ogrn = GetNullableLong(element, "OGRN"),
            Inn = GetString(element, "INN"),
            Name = GetString(element, "Name"),
            Status = GetString(element, "Status") ?? "",
            ErrorText = GetString(element, "ErrorText")
        };
    }

    /// <summary>
    /// Парсит элемент RecordSet (результат поиска с пагинацией).
    /// </summary>
    public static CbrFinOrgSearchResult ParseSearchResult(XElement? element)
    {
        if (element is null)
            return new CbrFinOrgSearchResult();

        var result = new CbrFinOrgSearchResult
        {
            IsSuccess = GetBool(element, "IsSucess"),
            TotalPages = GetUInt(element, "TotalPages"),
            CurrentPage = GetUInt(element, "CurrentPage"),
            PageSize = GetUInt(element, "PageSize"),
            TotalRows = GetUInt(element, "TotalRows"),
            Error = GetString(element, "Error")
        };

        var dsElement = element.Element("DS");
        if (dsElement is not null)
            result.Records = dsElement.Elements("Record")
                .Select(ParseRecord)
                .Where(r => r is not null)
                .Cast<CbrFinOrgRecord>()
                .ToList();

        return result;
    }

    /// <summary>
    /// Парсит элемент BranchRecord (филиал).
    /// </summary>
    public static CbrFinOrgBranchRecord? ParseBranchRecord(XElement? element)
    {
        if (element is null)
            return null;

        return new CbrFinOrgBranchRecord
        {
            Id = GetLong(element, "Id"),
            Number = GetString(element, "Num"),
            Name = GetString(element, "Name"),
            BranchType = GetString(element, "BranchType"),
            Address = GetString(element, "Address"),
            OpenDate = GetDateTime(element, "OpenDate"),
            Affiliation = GetString(element, "Affiliation"),
            HasChild = GetBool(element, "HasChild")
        };
    }

    // ── Вспомогательные методы ─────────────────────────────────────

    private static string? GetString(XElement parent, string name) =>
        parent.Element(name)?.Value;

    private static long GetLong(XElement parent, string name) =>
        long.TryParse(parent.Element(name)?.Value, out var v) ? v : 0;

    private static long? GetNullableLong(XElement parent, string name) =>
        long.TryParse(parent.Element(name)?.Value, out var v) ? v : null;

    private static int GetInt(XElement parent, string name) =>
        int.TryParse(parent.Element(name)?.Value, out var v) ? v : 0;

    private static int? GetNullableInt(XElement parent, string name) =>
        int.TryParse(parent.Element(name)?.Value, out var v) ? v : null;

    private static uint GetUInt(XElement parent, string name) =>
        uint.TryParse(parent.Element(name)?.Value, out var v) ? v : 0;

    private static decimal GetDecimal(XElement parent, string name) =>
        decimal.TryParse(parent.Element(name)?.Value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;

    private static bool GetBool(XElement parent, string name) =>
        bool.TryParse(parent.Element(name)?.Value, out var v) && v;

    private static DateTime GetDateTime(XElement parent, string name) =>
        DateTime.TryParse(parent.Element(name)?.Value, out var v) ? v : default;

    private static DateTime? GetNullableDateTime(XElement parent, string name) =>
        DateTime.TryParse(parent.Element(name)?.Value, out var v) ? v : null;
}
