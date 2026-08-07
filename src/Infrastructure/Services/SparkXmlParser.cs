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
}