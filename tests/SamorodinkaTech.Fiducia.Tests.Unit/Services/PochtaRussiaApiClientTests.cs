using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Models.PochtaRussia;
using SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Unit-тесты клиента API Почты России с использованием MockPochtaRussiaApiClient.
/// </summary>
public class PochtaRussiaApiClientTests
{
    private readonly MockPochtaRussiaApiClient _client = new();

    // ── CalculateRateAsync ───────────────────────────────────────

    [Fact]
    public async Task CalculateRateAsync_ValidRequest_ShouldReturnRate()
    {
        var expectedRate = new PochtaRussiaRateResponse
        {
            TotalRate = 15000,
            TotalVat = 2700,
            GroundRate = new PochtaRussiaTariff { Rate = 15000, Vat = 2700 },
            DeliveryTime = new PochtaRussiaDeliveryTime { MinDays = 3, MaxDays = 7 }
        };
        _client.RateResult = expectedRate;

        var request = new PochtaRussiaRateRequest
        {
            MailType = "LETTER",
            MailCategory = "ORDERED",
            Mass = 100,
            IndexTo = "117105",
            MailDirect = 643
        };

        var result = await _client.CalculateRateAsync(request);

        result.Should().NotBeNull();
        result!.TotalRate.Should().Be(15000);
        result.TotalVat.Should().Be(2700);
        result.GroundRate.Should().NotBeNull();
        result.GroundRate!.Rate.Should().Be(15000);
        result.DeliveryTime.Should().NotBeNull();
        result.DeliveryTime!.MinDays.Should().Be(3);
        result.DeliveryTime.MaxDays.Should().Be(7);
        _client.RateCallCount.Should().Be(1);
        _client.LastRateMailType.Should().Be("LETTER");
    }

    [Fact]
    public async Task CalculateRateAsync_WithInsurance_ShouldReturnInsuranceRate()
    {
        var expectedRate = new PochtaRussiaRateResponse
        {
            TotalRate = 25000,
            TotalVat = 4500,
            InsuranceRate = new PochtaRussiaTariff { Rate = 10000, Vat = 1800 }
        };
        _client.RateResult = expectedRate;

        var request = new PochtaRussiaRateRequest
        {
            MailType = "LETTER",
            MailCategory = "REGISTERED",
            Mass = 200,
            IndexTo = "630084",
            MailDirect = 643,
            DeclaredValue = 500000
        };

        var result = await _client.CalculateRateAsync(request);

        result.Should().NotBeNull();
        result!.InsuranceRate.Should().NotBeNull();
        result.InsuranceRate!.Rate.Should().Be(10000);
        result.InsuranceRate.Vat.Should().Be(1800);
    }

    [Fact]
    public async Task CalculateRateAsync_SimulateUnavailable_ShouldReturnNull()
    {
        _client.SimulateUnavailable = true;

        var request = new PochtaRussiaRateRequest
        {
            MailType = "LETTER",
            MailCategory = "SIMPLE",
            Mass = 50
        };

        var result = await _client.CalculateRateAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CalculateRateAsync_SimulateFailure_ShouldThrow()
    {
        _client.SimulateFailure = true;

        var request = new PochtaRussiaRateRequest
        {
            MailType = "LETTER",
            MailCategory = "SIMPLE",
            Mass = 50
        };

        var act = () => _client.CalculateRateAsync(request);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*Simulated*");
    }

    // ── NormalizeAddressesAsync ──────────────────────────────────

    [Fact]
    public async Task NormalizeAddressesAsync_ValidAddress_ShouldReturnNormalized()
    {
        var expected = new List<PochtaRussiaAddressResponse>
        {
            new()
            {
                Id = "1",
                OriginalAddress = "г Москва, ул Варшавская, д 37",
                QualityCode = "GOOD",
                ValidationCode = "VALIDATED",
                Index = "117105",
                Place = "Москва",
                Region = "Москва",
                Street = "ул Варшавская",
                House = "37"
            }
        };
        _client.NormalizeResult = expected;

        var addresses = new List<PochtaRussiaAddressRequest>
        {
            new() { Id = "1", OriginalAddress = "г Москва, ул Варшавская, д 37" }
        };

        var result = await _client.NormalizeAddressesAsync(addresses);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be("1");
        result[0].Index.Should().Be("117105");
        result[0].QualityCode.Should().Be("GOOD");
        result[0].ValidationCode.Should().Be("VALIDATED");
        result[0].IsDeliverable.Should().BeTrue();
        _client.NormalizeCallCount.Should().Be(1);
    }

    [Fact]
    public async Task NormalizeAddressesAsync_InvalidAddress_ShouldReturnNonDeliverable()
    {
        var expected = new List<PochtaRussiaAddressResponse>
        {
            new()
            {
                Id = "1",
                OriginalAddress = "неправильный адрес",
                QualityCode = "NOT_FOUND",
                ValidationCode = "INCORRECT"
            }
        };
        _client.NormalizeResult = expected;

        var addresses = new List<PochtaRussiaAddressRequest>
        {
            new() { Id = "1", OriginalAddress = "неправильный адрес" }
        };

        var result = await _client.NormalizeAddressesAsync(addresses);

        result.Should().HaveCount(1);
        result[0].IsDeliverable.Should().BeFalse();
    }

    [Fact]
    public async Task NormalizeAddressesAsync_SimulateFailure_ShouldThrow()
    {
        _client.SimulateFailure = true;

        var addresses = new List<PochtaRussiaAddressRequest>
        {
            new() { Id = "1", OriginalAddress = "test" }
        };

        var act = () => _client.NormalizeAddressesAsync(addresses);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // ── CreateOrderAsync ─────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_ValidOrder_ShouldReturnResultIds()
    {
        var expectedResult = new PochtaRussiaOrderResponse
        {
            ResultIds = new List<long> { 12345 },
            Errors = new List<PochtaRussiaOrderError>()
        };
        _client.OrderResult = expectedResult;

        var order = new PochtaRussiaOrderRequest
        {
            OrderNum = "ORD-001",
            MailType = "LETTER",
            MailCategory = "ORDERED",
            MailDirect = 643,
            Mass = 100,
            IndexTo = 117105,
            RecipientName = "Иванов Иван Иванович",
            GivenName = "Иван",
            Surname = "Иванов",
            MiddleName = "Иванович",
            PlaceTo = "г Москва",
            RegionTo = "г Москва",
            StreetTo = "ш Варшавское",
            HouseTo = "37",
            PostofficeCode = "101000"
        };

        var result = await _client.CreateOrderAsync(order);

        result.Should().NotBeNull();
        result!.ResultIds.Should().HaveCount(1);
        result.ResultIds[0].Should().Be(12345);
        result.Errors.Should().BeEmpty();
        _client.OrderCallCount.Should().Be(1);
        _client.LastOrderNum.Should().Be("ORD-001");
    }

    [Fact]
    public async Task CreateOrderAsync_WithErrors_ShouldReturnErrors()
    {
        var expectedResult = new PochtaRussiaOrderResponse
        {
            ResultIds = new List<long>(),
            Errors = new List<PochtaRussiaOrderError>
            {
                new()
                {
                    ErrorCodes = new List<PochtaRussiaErrorCode>
                    {
                        new() { Code = "ILLEGAL_MASS_EXCESS", Description = "Превышен допустимый вес" }
                    },
                    Position = 0
                }
            }
        };
        _client.OrderResult = expectedResult;

        var order = new PochtaRussiaOrderRequest
        {
            OrderNum = "ORD-002",
            MailType = "POSTAL_PARCEL",
            MailCategory = "ORDINARY",
            MailDirect = 643,
            Mass = 200000,
            IndexTo = 630084,
            RecipientName = "Сидоров Сергей",
            GivenName = "Сергей",
            Surname = "Сидоров",
            PlaceTo = "г Новосибирск",
            RegionTo = "обл Новосибирская",
            StreetTo = "пр Газовый",
            HouseTo = "13",
            PostofficeCode = "101000"
        };

        var result = await _client.CreateOrderAsync(order);

        result.Should().NotBeNull();
        result!.ResultIds.Should().BeEmpty();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].ErrorCodes[0].Code.Should().Be("ILLEGAL_MASS_EXCESS");
        result.Errors[0].Position.Should().Be(0);
    }

    [Fact]
    public async Task CreateOrderAsync_SimulateFailure_ShouldThrow()
    {
        _client.SimulateFailure = true;

        var order = new PochtaRussiaOrderRequest
        {
            OrderNum = "ORD-003",
            MailType = "LETTER",
            MailCategory = "SIMPLE",
            MailDirect = 643,
            Mass = 50,
            IndexTo = 117105,
            RecipientName = "Тест",
            GivenName = "Тест",
            Surname = "Тест",
            PlaceTo = "Москва",
            RegionTo = "Москва",
            StreetTo = "ул Тестовая",
            HouseTo = "1",
            PostofficeCode = "101000"
        };

        var act = () => _client.CreateOrderAsync(order);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // ── GetApiLimitAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetApiLimitAsync_ShouldReturnLimits()
    {
        var expectedLimit = new PochtaRussiaApiLimitResponse
        {
            AllowedCount = 10000,
            CurrentCount = 250
        };
        _client.ApiLimitResult = expectedLimit;

        var result = await _client.GetApiLimitAsync();

        result.Should().NotBeNull();
        result!.AllowedCount.Should().Be(10000);
        result.CurrentCount.Should().Be(250);
        _client.ApiLimitCallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetApiLimitAsync_SimulateUnavailable_ShouldReturnNull()
    {
        _client.SimulateUnavailable = true;

        var result = await _client.GetApiLimitAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetApiLimitAsync_SimulateFailure_ShouldThrow()
    {
        _client.SimulateFailure = true;

        var act = () => _client.GetApiLimitAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // ── IsDeliverable ────────────────────────────────────────────

    [Theory]
    [InlineData("GOOD", "VALIDATED", true)]
    [InlineData("GOOD", "OVERRIDDEN", true)]
    [InlineData("GOOD", "CONFIRMED_MANUALLY", true)]
    [InlineData("POSTAL_BOX", "VALIDATED", true)]
    [InlineData("ON_DEMAND", "VALIDATED", true)]
    [InlineData("UNDEF_05", "VALIDATED", true)]
    [InlineData("NOT_FOUND", "VALIDATED", false)]
    [InlineData("GOOD", "NOT_FOUND", false)]
    [InlineData("NOT_FOUND", "NOT_FOUND", false)]
    public void IsDeliverable_ShouldReturnCorrectResult(string qualityCode, string validationCode, bool expected)
    {
        var response = new PochtaRussiaAddressResponse
        {
            Id = "1",
            OriginalAddress = "test",
            QualityCode = qualityCode,
            ValidationCode = validationCode
        };

        response.IsDeliverable.Should().Be(expected);
    }
}
