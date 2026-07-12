using System.Xml.Linq;
using FluentAssertions;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class SparkXmlParserTests
{
    // ── ParseCompany ──────────────────────────────────────────────

    [Fact]
    public void ParseCompany_ValidXml_ReturnsCorrectFields()
    {
        var xml = new XElement("Report",
            new XElement("SparkID", "12345"),
            new XElement("CompanyType", "1"),
            new XElement("INN", "7701234567"),
            new XElement("KPP", "770101001"),
            new XElement("OGRN", "1027700123456"),
            new XElement("OKPO", "12345678"),
            new XElement("FullNameRus", "Общество с ограниченной ответственностью «Тест»"),
            new XElement("ShortNameRus", "ООО «Тест»"),
            new XElement("OKOPF",
                new XAttribute("Code", "12300"),
                new XAttribute("Name", "Общество с ограниченной ответственностью")),
            new XElement("LegalAddresses",
                new XElement("Address",
                    new XAttribute("Address", "г. Москва, ул. Тестовая, д. 1"))),
            new XElement("Status",
                new XAttribute("IsActing", "true"),
                new XAttribute("Type", "Действующее")),
            new XElement("DateFirstReg", "2010-01-15"),
            new XElement("CharterCapital", "10000.00"));

        var result = SparkXmlParser.ParseCompany(xml);

        result.SparkId.Should().Be(12345);
        result.CompanyType.Should().Be(1);
        result.Inn.Should().Be("7701234567");
        result.Kpp.Should().Be("770101001");
        result.Ogrn.Should().Be("1027700123456");
        result.Okpo.Should().Be("12345678");
        result.FullName.Should().Be("Общество с ограниченной ответственностью «Тест»");
        result.ShortName.Should().Be("ООО «Тест»");
        result.OkopfCode.Should().Be("12300");
        result.OkopfName.Should().Be("Общество с ограниченной ответственностью");
        result.LegalAddress.Should().Be("г. Москва, ул. Тестовая, д. 1");
        result.IsActing.Should().BeTrue();
        result.Status.Should().Be("Действующее");
        result.RegistrationDate.Should().Be(new DateTime(2010, 1, 15));
        result.CharterCapital.Should().Be(10000.00m);
    }

    [Fact]
    public void ParseCompany_MinimalXml_ReturnsDefaults()
    {
        var xml = new XElement("Report",
            new XElement("INN", "7701234567"),
            new XElement("FullNameRus", "Тест"));

        var result = SparkXmlParser.ParseCompany(xml);

        result.SparkId.Should().Be(0);
        result.CompanyType.Should().Be(1);
        result.Inn.Should().Be("7701234567");
        result.FullName.Should().Be("Тест");
        result.ShortName.Should().BeNull();
        result.OkopfCode.Should().BeNull();
        result.IsActing.Should().BeFalse();
    }

    [Fact]
    public void ParseCompany_NullElements_HandlesGracefully()
    {
        var xml = new XElement("Report");

        var result = SparkXmlParser.ParseCompany(xml);

        result.Inn.Should().Be("");
        result.FullName.Should().Be("");
        result.IsActing.Should().BeFalse();
    }

    // ── ParseManager ──────────────────────────────────────────────

    [Fact]
    public void ParseManager_ValidXml_ReturnsCorrectFields()
    {
        var xml = new XElement("Leader",
            new XAttribute("FIO", "Иванов Иван Иванович"),
            new XAttribute("Position", "Генеральный директор"),
            new XAttribute("INN", "770123456789"),
            new XAttribute("ActualDate", "2024-01-15"),
            new XAttribute("LegalCapacityEndDate", "2025-12-31"),
            new XAttribute("ManagementCompany", "УК «Тест»"),
            new XAttribute("ManagementCompanyINN", "770987654321"));

        var result = SparkXmlParser.ParseManager(xml);

        result.FullName.Should().Be("Иванов Иван Иванович");
        result.Position.Should().Be("Генеральный директор");
        result.Inn.Should().Be("770123456789");
        result.ActualDate.Should().Be(new DateTime(2024, 1, 15));
        result.LegalCapacityEndDate.Should().Be(new DateTime(2025, 12, 31));
        result.ManagementCompany.Should().Be("УК «Тест»");
        result.ManagementCompanyINN.Should().Be("770987654321");
    }

    [Fact]
    public void ParseManager_NullAttributes_HandlesGracefully()
    {
        var xml = new XElement("Leader");

        var result = SparkXmlParser.ParseManager(xml);

        result.FullName.Should().Be("");
        result.Position.Should().BeNull();
        result.Inn.Should().BeNull();
    }

    // ── ParseFounder ──────────────────────────────────────────────

    [Fact]
    public void ParseFounder_Attributes_ReturnsRussianLegalEntity()
    {
        var xml = new XElement("Coowner",
            new XAttribute("Type", "0"),
            new XAttribute("Name", "ООО «Учредитель»"),
            new XAttribute("INN", "7701111111"),
            new XAttribute("OGRN", "1027700111111"),
            new XAttribute("ShareAmount", "5000.00"),
            new XAttribute("SharePercent", "50.00"),
            new XAttribute("EntryDate", "2020-01-01"));

        var result = SparkXmlParser.ParseFounder(xml);

        result.Should().NotBeNull();
        result!.Name.Should().Be("ООО «Учредитель»");
        result.Inn.Should().Be("7701111111");
        result.Ogrn.Should().Be("1027700111111");
        result.IsForeign.Should().BeFalse();
        result.ShareAmount.Should().Be(5000.00m);
        result.SharePercent.Should().Be(50.00m);
        result.EntryDate.Should().Be(new DateTime(2020, 1, 1));
    }

    [Fact]
    public void ParseFounder_ForeignType1_ReturnsIsForeign()
    {
        var xml = new XElement("Coowner",
            new XAttribute("Type", "1"),
            new XAttribute("Name", "Foreign LLC"),
            new XAttribute("Country", "USA"));

        var result = SparkXmlParser.ParseFounder(xml);

        result.Should().NotBeNull();
        result!.IsForeign.Should().BeTrue();
        result.Country.Should().Be("USA");
    }

    [Fact]
    public void ParseFounder_PersonType2_ReturnsPersonFields()
    {
        var xml = new XElement("Coowner",
            new XAttribute("Type", "2"),
            new XAttribute("FullName", "Петров Пётр Петрович"),
            new XAttribute("PersonINN", "770123456789"),
            new XAttribute("Citizenship", "Российская Федерация"));

        var result = SparkXmlParser.ParseFounder(xml);

        result.Should().NotBeNull();
        result!.FullName.Should().Be("Петров Пётр Петрович");
        result.PersonInn.Should().Be("770123456789");
        result.Citizenship.Should().Be("Российская Федерация");
    }

    [Fact]
    public void ParseFounder_EmptyElement_ReturnsEmptyFounder()
    {
        var xml = new XElement("Coowner");

        var result = SparkXmlParser.ParseFounder(xml);

        result.Should().NotBeNull();
        result!.Name.Should().BeNull();
        result.IsForeign.Should().BeFalse();
    }

    // ── ParseInt ──────────────────────────────────────────────────

    [Theory]
    [InlineData("123", 123)]
    [InlineData("-1", -1)]
    [InlineData("0", 0)]
    public void ParseInt_ValidNumber_ReturnsInt(string input, int expected)
    {
        SparkXmlParser.ParseInt(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("12.34")]
    public void ParseInt_InvalidInput_ReturnsNull(string? input)
    {
        SparkXmlParser.ParseInt(input).Should().BeNull();
    }

    // ── ParseDate ─────────────────────────────────────────────────

    [Fact]
    public void ParseDate_ValidDate_ReturnsDateTime()
    {
        SparkXmlParser.ParseDate("2024-01-15").Should().Be(new DateTime(2024, 1, 15));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-date")]
    public void ParseDate_InvalidInput_ReturnsNull(string? input)
    {
        SparkXmlParser.ParseDate(input).Should().BeNull();
    }

    // ── ParseDecimal ──────────────────────────────────────────────

    [Theory]
    [InlineData("1000.50", 1000.50)]
    [InlineData("0", 0)]
    [InlineData("-500.25", -500.25)]
    public void ParseDecimal_ValidNumber_ReturnsDecimal(string input, decimal expected)
    {
        SparkXmlParser.ParseDecimal(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    public void ParseDecimal_InvalidInput_ReturnsNull(string? input)
    {
        SparkXmlParser.ParseDecimal(input).Should().BeNull();
    }
}
