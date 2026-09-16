using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Enums;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

public class ContractServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.FiduciaDbContext _ctx;
    private readonly Mock<IChunkedUploadService> _uploadServiceMock;
    private readonly ContractService _sut;

    public ContractServiceTests()
    {
        var (factory, _) = WriteServiceTestBase.CreateMockFactory(out _ctx);
        _uploadServiceMock = new Mock<IChunkedUploadService>();
        var logger = Mock.Of<ILogger<ContractService>>();
        _sut = new ContractService(factory, _uploadServiceMock.Object, logger);
    }

    public void Dispose() => _ctx.Dispose();

    private void Refresh() => _ctx.ChangeTracker.Clear();

    [Fact]
    public async Task CreateAsync_ValidModel_ReturnsNewId()
    {
        var leId = Guid.NewGuid();
        var model = new ContractCreateModel
        {
            LegalEntityId = leId,
            UserId = Guid.NewGuid(),
            ContractType = ContractType.REGISTRAR,
            CounterpartyName = "Реестродержатель ООО",
            CounterpartyInn = "7701234567"
        };

        var id = await _sut.CreateAsync(model, fileStream: null, fileName: null);

        Refresh();
        id.Should().NotBeEmpty();
        var entity = _ctx.Contracts.FirstOrDefault(c => c.Id == id);
        entity.Should().NotBeNull();
        entity!.LegalEntityId.Should().Be(leId);
        entity.ContractType.Should().Be(ContractType.REGISTRAR);
        entity.CounterpartyName.Should().Be("Реестродержатель ООО");
        entity.CounterpartyInn.Should().Be("7701234567");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_MissingCounterparty_Throws()
    {
        var model = new ContractCreateModel
        {
            LegalEntityId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ContractType = ContractType.INFO_AGENCY,
            CounterpartyName = "",
            CounterpartyInn = "7701234567"
        };

        var act = () => _sut.CreateAsync(model, null, null);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*required*");
    }

    [Fact]
    public async Task CreateAsync_EmptyLegalEntityId_Throws()
    {
        var model = new ContractCreateModel
        {
            LegalEntityId = Guid.Empty,
            UserId = Guid.NewGuid(),
            ContractType = ContractType.INFO_AGENCY,
            CounterpartyName = "Контрагент",
            CounterpartyInn = "7701234567"
        };

        var act = () => _sut.CreateAsync(model, null, null);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*не выбрано*");
    }

    [Fact]
    public async Task UpdateAsync_ExistingContract_UpdatesFields()
    {
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            LegalEntityId = Guid.NewGuid(),
            ContractType = ContractType.REGISTRAR,
            CounterpartyName = "Старое имя",
            CounterpartyInn = "7700000001",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _ctx.Contracts.Add(contract);
        _ctx.SaveChanges();

        var model = new ContractUpdateModel
        {
            ContractNumber = "Д-2025/001",
            ContractDate = new DateOnly(2025, 1, 15),
            IsIndefinite = false,
            ContractValidTo = new DateOnly(2026, 1, 15)
        };

        await _sut.UpdateAsync(contract.Id, model, fileStream: null, fileName: null);

        Refresh();
        var updated = _ctx.Contracts.FirstOrDefault(c => c.Id == contract.Id);
        updated.Should().NotBeNull();
        updated!.ContractNumber.Should().Be("Д-2025/001");
        updated.ContractDate.Should().Be(new DateOnly(2025, 1, 15));
        updated.ContractValidTo.Should().Be(new DateOnly(2026, 1, 15));
        updated.IsIndefinite.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_NotFound_Throws()
    {
        var model = new ContractUpdateModel
        {
            ContractNumber = "Д-2025/001",
            IsIndefinite = true
        };

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), model, null, null);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*не найден*");
    }
}
