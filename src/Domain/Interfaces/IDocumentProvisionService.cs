namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Сервис автоматической подгрузки документов при принятии требования REQUEST_INFORMATION.
/// </summary>
public interface IDocumentProvisionService
{
    /// <summary>
    /// Автоматически создаёт пункты требования (ShareRequestItem) по типам документов
    /// и прикрепляет найденные в системе файлы.
    /// </summary>
    Task AutoProvisionDocumentsAsync(Guid shareRequestId, CancellationToken ct = default);
}
