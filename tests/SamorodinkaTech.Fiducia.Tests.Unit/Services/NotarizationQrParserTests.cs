using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class NotarizationQrParserTests
{
    private readonly NotarizationQrParser _parser;

    public NotarizationQrParserTests()
    {
        var logger = Mock.Of<ILogger<NotarizationQrParser>>();
        _parser = new NotarizationQrParser(logger);
    }

    // ── URL parsing ─────────────────────────────────────────────

    [Fact]
    public void Parse_ValidUrl_ReturnsCorrectFields()
    {
        var url = "https://reestr.notariat.ru/ru/notarial-acts/?id=12345&date=2024-01-15&notary=Иванов+Иван+Иванович&district=Московский&type=доверенность&applicant=Петров+Пётр";

        var result = _parser.Parse(url);

        result.Should().NotBeNull();
        result!.RegistryNumber.Should().Be("12345");
        result.NotarizationDate.Should().Be(new DateOnly(2024, 1, 15));
        result.NotaryFullName.Should().Be("Иванов Иван Иванович");
        result.NotaryDistrict.Should().Be("Московский");
        result.DocumentType.Should().Be("доверенность");
        result.ApplicantName.Should().Be("Петров Пётр");
        result.RawUrl.Should().Be(url);
    }

    [Fact]
    public void Parse_UrlWithPartialParams_ReturnsPartialResult()
    {
        var url = "https://reestr.notariat.ru/ru/notarial-acts/?id=67890&notary=Петров";

        var result = _parser.Parse(url);

        result.Should().NotBeNull();
        result!.RegistryNumber.Should().Be("67890");
        result.NotaryFullName.Should().Be("Петров");
        result.NotarizationDate.Should().BeNull();
        result.NotaryDistrict.Should().BeNull();
    }

    [Fact]
    public void Parse_UrlWithRussianParams_ParsesCorrectly()
    {
        var url = "https://reestr.notariat.ru/ru/notarial-acts/?номер=АБВ-123&дата=2024-06-01&нотариус=Сидоров";

        var result = _parser.Parse(url);

        result.Should().NotBeNull();
        result!.RegistryNumber.Should().Be("АБВ-123");
        result.NotarizationDate.Should().Be(new DateOnly(2024, 6, 1));
        result.NotaryFullName.Should().Be("Сидоров");
    }

    // ── Key-value parsing ───────────────────────────────────────

    [Fact]
    public void Parse_KeyValueFormat_ReturnsCorrectFields()
    {
        var data = "id=12345;date=2024-01-15;notary=Иванов Иван Иванович;district=Московский";

        var result = _parser.Parse(data);

        result.Should().NotBeNull();
        result!.RegistryNumber.Should().Be("12345");
        result.NotarizationDate.Should().Be(new DateOnly(2024, 1, 15));
        result.NotaryFullName.Should().Be("Иванов Иван Иванович");
        result.NotaryDistrict.Should().Be("Московский");
    }

    [Fact]
    public void Parse_KeyValueWithRussianKeys_ParsesCorrectly()
    {
        var data = "номер=АБВ-123;дата=15.01.2024;нотариус=Петров Пётр;округ=Ленинский";

        var result = _parser.Parse(data);

        result.Should().NotBeNull();
        result!.RegistryNumber.Should().Be("АБВ-123");
        result.NotarizationDate.Should().Be(new DateOnly(2024, 1, 15));
        result.NotaryFullName.Should().Be("Петров Пётр");
        result.NotaryDistrict.Should().Be("Ленинский");
    }

    // ── Simple registry number ──────────────────────────────────

    [Fact]
    public void Parse_RegistryNumberOnly_ReturnsMinimalResult()
    {
        var result = _parser.Parse("12345-67890");

        result.Should().NotBeNull();
        result!.RegistryNumber.Should().Be("12345-67890");
        result.NotarizationDate.Should().BeNull();
        result.NotaryFullName.Should().BeNull();
    }

    // ── Edge cases ──────────────────────────────────────────────

    [Fact]
    public void Parse_Null_ReturnsNull()
    {
        _parser.Parse(null!).Should().BeNull();
    }

    [Fact]
    public void Parse_EmptyString_ReturnsNull()
    {
        _parser.Parse("").Should().BeNull();
    }

    [Fact]
    public void Parse_Whitespace_ReturnsNull()
    {
        _parser.Parse("   ").Should().BeNull();
    }

    [Fact]
    public void Parse_RandomText_ReturnsNull()
    {
        _parser.Parse("hello world this is not a qr code").Should().BeNull();
    }

    [Fact]
    public void Parse_UrlWithDifferentDateFormat_ParsesDate()
    {
        var url = "https://reestr.notariat.ru/ru/notarial-acts/?id=999&date=15/06/2024";

        var result = _parser.Parse(url);

        result.Should().NotBeNull();
        result!.NotarizationDate.Should().Be(new DateOnly(2024, 6, 15));
    }

    [Fact]
    public void Parse_UrlWithDateTimeFormat_ParsesDate()
    {
        var url = "https://reestr.notariat.ru/ru/notarial-acts/?id=999&date=2024-06-15T10:30:00";

        var result = _parser.Parse(url);

        result.Should().NotBeNull();
        result!.NotarizationDate.Should().Be(new DateOnly(2024, 6, 15));
    }
}
