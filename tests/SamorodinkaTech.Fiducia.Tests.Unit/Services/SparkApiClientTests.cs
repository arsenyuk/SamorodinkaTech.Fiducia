using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Models.Spark;
using SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Unit-тесты клиента СПАРК API с использованием MockSparkApiClient.
/// Проверяют контракт ISparkApiClient: получение карточки компании,
/// данных о гендиректоре, учредителей (ООО) и обработку сбоев (403, general failure).
/// </summary>
public class SparkApiClientTests
{
    private readonly MockSparkApiClient _client = new();

    [Fact]
    public async Task GetCompanyByInnAsync_ExistingCompany_ShouldReturnCompanyData()
    {
        var company = new SparkCompany
        {
            Inn = "7707083893",
            Ogrn = "1027700132195",
            FullName = "Публичное акционерное общество «Сбербанк России»",
            ShortName = "ПАО Сбербанк",
            OkopfCode = "12247",
            OkopfName = "Публичные акционерные общества",
            LegalAddress = "117312, г. Москва, ул. Вавилова, д. 19",
            Status = "Действующая",
            RegistrationDate = new DateTime(1991, 6, 20),
            ShareholdersCount = 50000
        };
        _client.AddCompany(company);

        var result = await _client.GetCompanyByInnAsync("7707083893");

        result.Should().NotBeNull();
        result!.Inn.Should().Be("7707083893");
        result.Ogrn.Should().Be("1027700132195");
        result.FullName.Should().Be("Публичное акционерное общество «Сбербанк России»");
        result.ShortName.Should().Be("ПАО Сбербанк");
        result.OkopfCode.Should().Be("12247");
        result.OkopfName.Should().Be("Публичные акционерные общества");
        result.Status.Should().Be("Действующая");
        result.LegalAddress.Should().NotBeNullOrEmpty();
        result.RegistrationDate.Should().HaveValue();
        result.ShareholdersCount.Should().Be(50000);
    }

    [Fact]
    public async Task GetCompanyByInnAsync_NonExistingCompany_ShouldReturnNull()
    {
        var result = await _client.GetCompanyByInnAsync("0000000000");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetManagerAsync_ExistingCompany_ShouldReturnManagerData()
    {
        var manager = new SparkManager
        {
            FullName = "Греф Герман Оскарович",
            Position = "Президент, Председатель Правления",
            Inn = "772706789012",
            StartDate = new DateTime(2007, 11, 28)
        };
        _client.AddManager("7707083893", manager);

        var result = await _client.GetManagerAsync("7707083893");

        result.Should().NotBeNull();
        result!.FullName.Should().Be("Греф Герман Оскарович");
        result.Position.Should().Contain("Президент");
        result.Inn.Should().Be("772706789012");
        result.StartDate.Should().HaveValue();
    }

    [Fact]
    public async Task GetManagerAsync_NonExistingCompany_ShouldReturnNull()
    {
        var result = await _client.GetManagerAsync("0000000000");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCompanyByInnAsync_SimulateFailure_ShouldThrow()
    {
        _client.SimulateFailure = true;

        var act = () => _client.GetCompanyByInnAsync("7707083893");

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Simulated SPARK API failure");
    }

    [Fact]
    public async Task GetManagerAsync_SimulateFailure_ShouldThrow()
    {
        _client.SimulateFailure = true;

        var act = () => _client.GetManagerAsync("0000000000");

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Simulated SPARK API failure");
    }

    [Fact]
    public async Task GetCompanyByInnAsync_DifferentInns_ShouldReturnDifferentCompanies()
    {
        var c1 = new SparkCompany { Inn = "1111111111", FullName = "Компания А", Status = "Действующая" };
        var c2 = new SparkCompany { Inn = "2222222222", FullName = "Компания Б", Status = "Ликвидирована" };
        _client.AddCompany(c1);
        _client.AddCompany(c2);

        var r1 = await _client.GetCompanyByInnAsync("1111111111");
        var r2 = await _client.GetCompanyByInnAsync("2222222222");

        r1.Should().NotBeNull();
        r1!.FullName.Should().Be("Компания А");
        r2.Should().NotBeNull();
        r2!.FullName.Should().Be("Компания Б");
    }

    [Fact]
    public async Task GetCompanyByInnAsync_ManagerNotSet_ShouldReturnNull()
    {
        var company = new SparkCompany { Inn = "3333333333", FullName = "Компания В" };
        _client.AddCompany(company);
        // Manager не добавлен

        var companyResult = await _client.GetCompanyByInnAsync("3333333333");
        var managerResult = await _client.GetManagerAsync("3333333333");

        companyResult.Should().NotBeNull();
        managerResult.Should().BeNull();
    }

    // ── GetFoundersAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetFoundersAsync_ExistingCompanyWithFounders_ShouldReturnFounders()
    {
        _client.AddFounders("7707083893", new List<SparkFounder>
        {
            new() { FullName = "Иванов Иван Иванович", PersonInn = "770701234567",
                ShareAmount = 5000, SharePercent = 50 },
            new() { FullName = "Петров Петр Петрович", PersonInn = "770707654321",
                ShareAmount = 5000, SharePercent = 50 }
        });

        var result = await _client.GetFoundersAsync("7707083893");

        result.Should().HaveCount(2);
        result[0].FullName.Should().Be("Иванов Иван Иванович");
        result[0].PersonInn.Should().Be("770701234567");
        result[0].ShareAmount.Should().Be(5000);
        result[0].SharePercent.Should().Be(50);
        result[1].FullName.Should().Be("Петров Петр Петрович");
        result[1].SharePercent.Should().Be(50);
    }

    [Fact]
    public async Task GetFoundersAsync_NoFounders_ShouldReturnEmptyList()
    {
        var result = await _client.GetFoundersAsync("0000000000");

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFoundersAsync_SimulateForbidden_ShouldThrowHttpRequestExceptionWith403()
    {
        _client.SimulateForbiddenOnFounders = true;

        var act = () => _client.GetFoundersAsync("7707083893");

        var ex = await act.Should().ThrowAsync<HttpRequestException>();
        ex.And.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetFoundersAsync_SimulateFailure_ShouldThrow()
    {
        _client.SimulateFailure = true;

        var act = () => _client.GetFoundersAsync("7707083893");

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Simulated SPARK API failure");
    }

    [Fact]
    public async Task GetFoundersAsync_DifferentInns_ShouldReturnDifferentFounders()
    {
        _client.AddFounders("1111111111", new List<SparkFounder>
        {
            new() { FullName = "Учредитель А", SharePercent = 100 }
        });
        _client.AddFounders("2222222222", new List<SparkFounder>
        {
            new() { Name = "ООО «Учредитель Б»", Inn = "2222222223", SharePercent = 51 },
            new() { FullName = "Сидоров Сидор Сидорович", SharePercent = 49 }
        });

        var r1 = await _client.GetFoundersAsync("1111111111");
        var r2 = await _client.GetFoundersAsync("2222222222");

        r1.Should().HaveCount(1);
        r1[0].FullName.Should().Be("Учредитель А");
        r2.Should().HaveCount(2);
        r2[0].Name.Should().Be("ООО «Учредитель Б»");
        r2[0].Inn.Should().Be("2222222223");
        r2[1].FullName.Should().Be("Сидоров Сидор Сидорович");
    }

    [Fact]
    public async Task GetFoundersAsync_LegalEntityFounder_ShouldHaveNameNotFullName()
    {
        _client.AddFounders("3333333333", new List<SparkFounder>
        {
            new() { Name = "ООО «Холдинг»", Inn = "3333333334",
                ShareAmount = 7500, SharePercent = 75 }
        });

        var result = await _client.GetFoundersAsync("3333333333");

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("ООО «Холдинг»");
        result[0].Inn.Should().Be("3333333334");
        result[0].FullName.Should().BeNull();
        result[0].ShareAmount.Should().Be(7500);
        result[0].SharePercent.Should().Be(75);
    }

    [Fact]
    public async Task GetFoundersAsync_CancellationToken_ShouldRespectToken()
    {
        _client.SimulatedDelayMs = 500;
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10));

        var act = () => _client.GetFoundersAsync("7707083893", cts.Token);

        await act.Should().ThrowAsync<TaskCanceledException>();
    }
}
