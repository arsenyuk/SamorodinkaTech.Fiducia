using System.Globalization;
using System.Xml.Linq;
using SamorodinkaTech.Fiducia.Domain.Models.Spark;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Парсинг XML-ответов SOAP-сервиса СПАРК в доменные модели.
/// Отвечает только за преобразование XElement → SparkCompany / SparkManager / SparkFounder.
/// </summary>
internal static class SparkXmlParser
{
    /// <summary>
    /// Парсит элемент Report (из GetCompanyShortReport) в SparkCompany.
    /// </summary>
    public static SparkCompany ParseCompany(XElement report)
    {
        var okopf = report.Element("OKOPF");
        var status = report.Element("Status");

        return new SparkCompany
        {
            SparkId = ParseInt(report.Element("SparkID")?.Value) ?? 0,
            CompanyType = ParseInt(report.Element("CompanyType")?.Value) ?? 1,
            Inn = report.Element("INN")?.Value ?? "",
            Kpp = report.Element("KPP")?.Value,
            Ogrn = report.Element("OGRN")?.Value,
            Okpo = report.Element("OKPO")?.Value,
            FullName = report.Element("FullNameRus")?.Value ?? "",
            ShortName = report.Element("ShortNameRus")?.Value,
            OkopfCode = okopf?.Attribute("Code")?.Value,
            OkopfName = okopf?.Attribute("Name")?.Value,
            LegalAddress = report.Element("LegalAddresses")
                ?.Element("Address")
                ?.Attribute("Address")?.Value,
            IsActing = status?.Attribute("IsActing")?.Value == "true",
            Status = status?.Attribute("Type")?.Value,
            RegistrationDate = ParseDate(report.Element("DateFirstReg")?.Value),
            CharterCapital = ParseDecimal(report.Element("CharterCapital")?.Value)
        };
    }

    /// <summary>
    /// Парсит элемент Leader (из GetCompanyShortReport) в SparkManager.
    /// Соответствует типу LeaderRUS: атрибуты FIO, Position, INN, ActualDate, и др.
    /// </summary>
    public static SparkManager ParseManager(XElement leader)
    {
        return new SparkManager
        {
            FullName = leader.Attribute("FIO")?.Value ?? "",
            Position = leader.Attribute("Position")?.Value,
            Inn = leader.Attribute("INN")?.Value,
            ActualDate = ParseDate(leader.Attribute("ActualDate")?.Value),
            LegalCapacityEndDate = ParseDate(leader.Attribute("LegalCapacityEndDate")?.Value),
            ManagementCompany = leader.Attribute("ManagementCompany")?.Value,
            ManagementCompanyINN = leader.Attribute("ManagementCompanyINN")?.Value
        };
    }

    /// <summary>
    /// Парсит элемент Coowner/Owner (из GetCompanyCoowners) в SparkFounder.
    /// Структура точно не известна — реализован best-effort парсинг.
    /// </summary>
    public static SparkFounder? ParseFounder(XElement coowner)
    {
        var type = ParseInt(coowner.Attribute("Type")?.Value ?? coowner.Element("Type")?.Value);
        var name = coowner.Attribute("Name")?.Value ?? coowner.Element("Name")?.Value;
        var inn = coowner.Attribute("INN")?.Value ?? coowner.Element("INN")?.Value;
        var ogrn = coowner.Attribute("OGRN")?.Value ?? coowner.Element("OGRN")?.Value;
        var fullName = coowner.Attribute("FullName")?.Value ?? coowner.Element("FullName")?.Value;
        var personInn = coowner.Attribute("PersonINN")?.Value ?? coowner.Element("PersonINN")?.Value;
        var shareAmount = ParseDecimal(coowner.Attribute("ShareAmount")?.Value ?? coowner.Element("ShareAmount")?.Value);
        var sharePercent = ParseDecimal(coowner.Attribute("SharePercent")?.Value ?? coowner.Element("SharePercent")?.Value);
        var country = coowner.Attribute("Country")?.Value ?? coowner.Element("Country")?.Value;
        var entryDate = ParseDate(coowner.Attribute("EntryDate")?.Value ?? coowner.Element("EntryDate")?.Value);
        var exitDate = ParseDate(coowner.Attribute("ExitDate")?.Value ?? coowner.Element("ExitDate")?.Value);
        var citizenship = coowner.Attribute("Citizenship")?.Value ?? coowner.Element("Citizenship")?.Value;

        // CoownerType: 0=российское ЮЛ, 1=иностранное ЮЛ, 2=ФЛ
        var isForeign = type == 1;

        return new SparkFounder
        {
            Name = name,
            Inn = inn,
            Ogrn = ogrn,
            Country = country ?? (isForeign ? "Иностранное" : null),
            IsForeign = isForeign,
            FullName = fullName,
            PersonInn = personInn,
            Citizenship = citizenship,
            ShareAmount = shareAmount,
            SharePercent = sharePercent,
            EntryDate = entryDate,
            ExitDate = exitDate
        };
    }

    // ── Вспомогательные методы ────────────────────────────────────

    public static int? ParseInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    public static DateTime? ParseDate(string? value)
    {
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result) ? result : null;
    }

    public static decimal? ParseDecimal(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
    }

    public static bool ParseBool(string? value)
    {
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
    }

    // ── Новые парсеры ─────────────────────────────────────────────

    /// <summary>
    /// Парсит элемент Report (из GetEnterpreneurShortReport) в SparkEntrepreneur.
    /// </summary>
    public static SparkEntrepreneur ParseEntrepreneur(XElement report)
    {
        return new SparkEntrepreneur
        {
            SparkId = ParseInt(report.Element("SparkID")?.Value) ?? 0,
            Inn = report.Element("INN")?.Value ?? "",
            Ogrnip = report.Element("OGRNIP")?.Value,
            FullName = report.Element("FullNameRus")?.Value ?? report.Element("FullName")?.Value ?? "",
            RegistrationDate = ParseDate(report.Element("DateFirstReg")?.Value),
            CancellationDate = ParseDate(report.Element("DateCancel")?.Value),
            Status = report.Element("Status")?.Attribute("Type")?.Value,
            IsActing = ParseBool(report.Element("Status")?.Attribute("IsActing")?.Value),
            Address = report.Element("LegalAddresses")?.Element("Address")?.Attribute("Address")?.Value,
            OkvedMain = report.Element("OKVEDMain")?.Attribute("Code")?.Value,
            OkvedMainName = report.Element("OKVEDMain")?.Attribute("Name")?.Value
        };
    }

    /// <summary>
    /// Парсит элемент Report (из GetCompanyExtendedReport) в SparkCompanyExtended.
    /// </summary>
    public static SparkCompanyExtended ParseCompanyExtended(XElement report)
    {
        var okopf = report.Element("OKOPF");
        var status = report.Element("Status");
        var liquidation = report.Element("Liquidation");

        var okvedAdditional = new List<string>();
        var okvedList = report.Element("OKVEDAdditional")?.Elements("OKVED");
        if (okvedList is not null)
        {
            foreach (var okved in okvedList)
            {
                var code = okved.Attribute("Code")?.Value;
                if (code is not null)
                    okvedAdditional.Add(code);
            }
        }

        return new SparkCompanyExtended
        {
            SparkId = ParseInt(report.Element("SparkID")?.Value) ?? 0,
            Inn = report.Element("INN")?.Value ?? "",
            Kpp = report.Element("KPP")?.Value,
            Ogrn = report.Element("OGRN")?.Value,
            Okpo = report.Element("OKPO")?.Value,
            FullName = report.Element("FullNameRus")?.Value ?? "",
            ShortName = report.Element("ShortNameRus")?.Value,
            OkopfCode = okopf?.Attribute("Code")?.Value,
            OkopfName = okopf?.Attribute("Name")?.Value,
            LegalAddress = report.Element("LegalAddresses")
                ?.Element("Address")
                ?.Attribute("Address")?.Value,
            IsActing = status?.Attribute("IsActing")?.Value == "true",
            Status = status?.Attribute("Type")?.Value,
            RegistrationDate = ParseDate(report.Element("DateFirstReg")?.Value),
            CharterCapital = ParseDecimal(report.Element("CharterCapital")?.Value),
            LiquidationDate = ParseDate(liquidation?.Element("Date")?.Value),
            LiquidationReason = liquidation?.Element("Reason")?.Value,
            OkvedMain = report.Element("OKVEDMain")?.Attribute("Code")?.Value,
            OkvedMainName = report.Element("OKVEDMain")?.Attribute("Name")?.Value,
            OkvedAdditional = okvedAdditional,
            Phone = report.Element("Phone")?.Value,
            Email = report.Element("Email")?.Value,
            Website = report.Element("Website")?.Value,
            EmployeesCount = ParseInt(report.Element("EmployeesCount")?.Value),
            AverageSalary = ParseDecimal(report.Element("AverageSalary")?.Value),
            Revenue = ParseDecimal(report.Element("Revenue")?.Value),
            NetProfit = ParseDecimal(report.Element("NetProfit")?.Value),
            LastReportDate = ParseDate(report.Element("LastReportDate")?.Value),
            EgrulRegNumber = report.Element("EgrulRegNumber")?.Value,
            EgrulRegDate = ParseDate(report.Element("EgrulRegDate")?.Value),
            TaxAuthority = report.Element("TaxAuthority")?.Value,
            RegistrationAuthority = report.Element("RegistrationAuthority")?.Value
        };
    }

    /// <summary>
    /// Парсит структуру компании (из GetCompanyStructure).
    /// </summary>
    public static SparkCompanyStructure ParseCompanyStructure(XDocument data)
    {
        var root = data.Root!;
        var parentElement = root.Element("Parent");
        var currentElement = root.Element("Current") ?? root.Element("Report");
        var childrenElements = root.Element("Children")?.Elements("Company") ?? Enumerable.Empty<XElement>();
        var affiliatesElements = root.Element("Affiliates")?.Elements("Person") ?? root.Descendants("Affiliate");

        var current = currentElement is not null ? ParseStructureItem(currentElement) : null;

        return new SparkCompanyStructure
        {
            Parent = parentElement is not null ? ParseStructureItem(parentElement) : null,
            Current = current ?? new SparkCompanyStructureItem { Name = "Неизвестно", Inn = "" },
            Children = childrenElements.Select(ParseStructureItem).ToList(),
            Affiliates = affiliatesElements.Select(ParseAffiliate).ToList()
        };
    }

    private static SparkCompanyStructureItem ParseStructureItem(XElement element)
    {
        return new SparkCompanyStructureItem
        {
            Name = element.Attribute("Name")?.Value ?? element.Element("Name")?.Value ?? "",
            Inn = element.Attribute("INN")?.Value ?? element.Element("INN")?.Value ?? "",
            Ogrn = element.Attribute("OGRN")?.Value ?? element.Element("OGRN")?.Value,
            SharePercent = ParseDecimal(element.Attribute("SharePercent")?.Value ?? element.Element("SharePercent")?.Value),
            EntryDate = ParseDate(element.Attribute("EntryDate")?.Value ?? element.Element("EntryDate")?.Value),
            ExitDate = ParseDate(element.Attribute("ExitDate")?.Value ?? element.Element("ExitDate")?.Value),
            Role = element.Attribute("Role")?.Value ?? element.Element("Role")?.Value
        };
    }

    private static SparkAffiliatedPerson ParseAffiliate(XElement element)
    {
        return new SparkAffiliatedPerson
        {
            FullName = element.Attribute("FIO")?.Value ?? element.Element("FIO")?.Value ?? "",
            Inn = element.Attribute("INN")?.Value ?? element.Element("INN")?.Value,
            Position = element.Attribute("Position")?.Value ?? element.Element("Position")?.Value,
            RelationType = element.Attribute("RelationType")?.Value ?? element.Element("RelationType")?.Value,
            SharePercent = ParseDecimal(element.Attribute("SharePercent")?.Value ?? element.Element("SharePercent")?.Value)
        };
    }

    /// <summary>
    /// Парсит элемент Report (из GetStateAccount) в SparkStateAccount.
    /// </summary>
    public static SparkStateAccount ParseStateAccount(XElement report)
    {
        return new SparkStateAccount
        {
            Balance = ParseInt(report.Element("Balance")?.Value) ?? 0,
            TotalLimit = ParseInt(report.Element("TotalLimit")?.Value) ?? 0,
            UsedCount = ParseInt(report.Element("UsedCount")?.Value) ?? 0,
            LicenseEndDate = ParseDate(report.Element("LicenseEndDate")?.Value),
            TariffName = report.Element("TariffName")?.Value,
            Status = report.Element("Status")?.Value
        };
    }

    /// <summary>
    /// Парсит ответ GetCompanyCoownersHistory.
    /// </summary>
    public static SparkCoownersHistory ParseCoownersHistory(XDocument data, string inn)
    {
        var root = data.Root!;
        var currentElements = root.Element("CurrentCoowners")?.Elements("Coowner") ?? Enumerable.Empty<XElement>();
        var historicalElements = root.Element("HistoricalCoowners")?.Elements("Coowner") ?? Enumerable.Empty<XElement>();

        return new SparkCoownersHistory
        {
            Inn = inn,
            CurrentCoowners = currentElements.Select(ParseCoownerHistoryItem).ToList(),
            HistoricalCoowners = historicalElements.Select(ParseCoownerHistoryItem).ToList()
        };
    }

    private static SparkCoownerHistoryItem ParseCoownerHistoryItem(XElement element)
    {
        return new SparkCoownerHistoryItem
        {
            Name = element.Attribute("Name")?.Value ?? element.Element("Name")?.Value,
            Inn = element.Attribute("INN")?.Value ?? element.Element("INN")?.Value,
            Ogrn = element.Attribute("OGRN")?.Value ?? element.Element("OGRN")?.Value,
            FullName = element.Attribute("FullName")?.Value ?? element.Element("FullName")?.Value,
            PersonInn = element.Attribute("PersonINN")?.Value ?? element.Element("PersonINN")?.Value,
            SharePercent = ParseDecimal(element.Attribute("SharePercent")?.Value ?? element.Element("SharePercent")?.Value),
            ShareAmount = ParseDecimal(element.Attribute("ShareAmount")?.Value ?? element.Element("ShareAmount")?.Value),
            EntryDate = ParseDate(element.Attribute("EntryDate")?.Value ?? element.Element("EntryDate")?.Value),
            ExitDate = ParseDate(element.Attribute("ExitDate")?.Value ?? element.Element("ExitDate")?.Value),
            CoownerType = element.Attribute("CoownerType")?.Value ?? element.Element("CoownerType")?.Value,
            Country = element.Attribute("Country")?.Value ?? element.Element("Country")?.Value
        };
    }

    /// <summary>
    /// Парсит элемент Report (из GetPersonComplianceReport) в SparkPersonCompliance.
    /// </summary>
    public static SparkPersonCompliance ParsePersonCompliance(XElement report)
    {
        var sanctions = report.Element("Sanctions")?.Elements("Sanction")
            .Select(s => new SparkSanctionEntry
            {
                Country = s.Attribute("Country")?.Value ?? s.Element("Country")?.Value,
                SanctionType = s.Attribute("Type")?.Value ?? s.Element("Type")?.Value,
                Basis = s.Attribute("Basis")?.Value ?? s.Element("Basis")?.Value,
                DateImposed = ParseDate(s.Attribute("DateImposed")?.Value ?? s.Element("DateImposed")?.Value),
                DateRemoved = ParseDate(s.Attribute("DateRemoved")?.Value ?? s.Element("DateRemoved")?.Value)
            }).ToList() ?? new List<SparkSanctionEntry>();

        var pdlRelations = report.Element("PDLRelations")?.Elements("Relation")
            .Select(r => new SparkPdlRelation
            {
                FullName = r.Attribute("FIO")?.Value ?? r.Element("FIO")?.Value ?? "",
                Inn = r.Attribute("INN")?.Value ?? r.Element("INN")?.Value,
                RelationType = r.Attribute("RelationType")?.Value ?? r.Element("RelationType")?.Value,
                Position = r.Attribute("Position")?.Value ?? r.Element("Position")?.Value,
                Organization = r.Attribute("Organization")?.Value ?? r.Element("Organization")?.Value
            }).ToList() ?? new List<SparkPdlRelation>();

        var registries = report.Element("Registries")?.Elements("Registry")
            .Select(r => new SparkRegistryEntry
            {
                RegistryName = r.Attribute("Name")?.Value ?? r.Element("Name")?.Value,
                Country = r.Attribute("Country")?.Value ?? r.Element("Country")?.Value,
                DateAdded = ParseDate(r.Attribute("DateAdded")?.Value ?? r.Element("DateAdded")?.Value),
                DateRemoved = ParseDate(r.Attribute("DateRemoved")?.Value ?? r.Element("DateRemoved")?.Value)
            }).ToList() ?? new List<SparkRegistryEntry>();

        return new SparkPersonCompliance
        {
            FullName = report.Element("FullName")?.Value ?? "",
            Inn = report.Element("INN")?.Value,
            RiskLevel = report.Element("RiskLevel")?.Value ?? "unknown",
            IsSanctioned = ParseBool(report.Element("IsSanctioned")?.Value),
            IsPdl = ParseBool(report.Element("IsPDL")?.Value),
            IsPep = ParseBool(report.Element("IsPEP")?.Value),
            Sanctions = sanctions,
            PdlRelations = pdlRelations,
            Registries = registries,
            CheckDate = ParseDate(report.Element("CheckDate")?.Value),
            DataSource = report.Element("DataSource")?.Value,
            AdditionalInfo = report.Element("AdditionalInfo")?.Value
        };
    }
}