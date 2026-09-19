using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Infrastructure.Services;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Тесты DocumentProvisionService: автоматическая подгрузка документов при REQUIRE_INFORMATION.
/// </summary>
public class DocumentProvisionServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.FiduciaDbContext _ctx;
    private readonly DocumentProvisionService _sut;

    public DocumentProvisionServiceTests()
    {
        var (factory, _) = WriteServiceTestBase.CreateMockFactory(out _ctx);
        var logger = Mock.Of<ILogger<DocumentProvisionService>>();
        _sut = new DocumentProvisionService(factory, logger);
    }

    public void Dispose() => _ctx.Dispose();

    private void Refresh() => _ctx.ChangeTracker.Clear();

    /// <summary>
    /// Требование не найдено — метод завершает работу без ошибок.
    /// </summary>
    [Fact]
    public async Task AutoProvisionDocumentsAsync_RequestNotFound_DoesNotThrow()
    {
        var act = () => _sut.AutoProvisionDocumentsAsync(Guid.NewGuid());
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Требование без типа REQUEST_INFORMATION — метод завершает работу без ошибок.
    /// </summary>
    [Fact]
    public async Task AutoProvisionDocumentsAsync_NotRequestInformation_DoesNotThrow()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "EXIT_APPLICATION");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "submitted");

        var act = () => _sut.AutoProvisionDocumentsAsync(sr.Id);
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Требование REQUEST_INFORMATION без payload — метод завершает работу без ошибок.
    /// </summary>
    [Fact]
    public async Task AutoProvisionDocumentsAsync_EmptyPayload_DoesNotThrow()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "REQUEST_INFORMATION");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "submitted");
        sr.Payload = null;
        _ctx.SaveChanges();

        var act = () => _sut.AutoProvisionDocumentsAsync(sr.Id);
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Требование REQUEST_INFORMATION с пустым payload (пустой JSON) — метод завершает работу без ошибок.
    /// </summary>
    [Fact]
    public async Task AutoProvisionDocumentsAsync_EmptyJsonPayload_DoesNotThrow()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "REQUEST_INFORMATION");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "submitted");
        sr.Payload = "{\"documentTypeCodes\":[]}";
        _ctx.SaveChanges();

        var act = () => _sut.AutoProvisionDocumentsAsync(sr.Id);
        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// Требование REQUEST_INFORMATION с неизвестными типами документов — метод завершает работу без ошибок (группы пусты).
    /// </summary>
    [Fact]
    public async Task AutoProvisionDocumentsAsync_UnknownTypeCodes_DoesNotThrow()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);
        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "REQUEST_INFORMATION");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "submitted");
        sr.Payload = "{\"documentTypeCodes\":[\"UNKNOWN_TYPE\"]}";
        _ctx.SaveChanges();

        await _sut.AutoProvisionDocumentsAsync(sr.Id);

        Refresh();
        var items = _ctx.ShareRequestItems.Where(i => i.ShareRequestId == sr.Id).ToList();
        items.Should().BeEmpty();
    }

    /// <summary>
    /// Требование REQUEST_INFORMATION с payload, содержащим тип CHARTER — создаёт пункт.
    /// </summary>
    [Fact]
    public async Task AutoProvisionDocumentsAsync_CharterTypeCode_CreatesItem()
    {
        var okopfId = WriteServiceTestBase.SeedOkopf(_ctx, "12300");
        var le = WriteServiceTestBase.SeedLegalEntity(_ctx, okopfId);

        // Создаём справочник типов документов
        var docType = new RefDocumentType
        {
            Id = Guid.NewGuid(),
            Code = "CHARTER",
            Name = "Устав",
            GroupCode = "FOUNDING",
            GroupName = "Учредительные документы",
            IsForLlc = true
        };
        _ctx.DocumentTypes.Add(docType);
        _ctx.SaveChanges();

        var rt = WriteServiceTestBase.SeedRequestType(_ctx, "REQUEST_INFORMATION");
        var bp = WriteServiceTestBase.SeedBoardParticipant(_ctx, le.Id);
        var sr = WriteServiceTestBase.SeedShareRequest(_ctx, le.Id, bp.Id, rt.Id, "submitted");
        sr.LegalEntityId = le.Id;
        sr.Payload = "{\"documentTypeCodes\":[\"CHARTER\"]}";
        _ctx.SaveChanges();

        await _sut.AutoProvisionDocumentsAsync(sr.Id);

        Refresh();
        var items = _ctx.ShareRequestItems.Where(i => i.ShareRequestId == sr.Id).ToList();
        items.Should().HaveCount(1);
        items[0].Title.Should().Contain("Учредительные документы");
    }
}
