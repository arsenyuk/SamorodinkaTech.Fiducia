using SamorodinkaTech.Fiducia.Domain.Models;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Генератор DOCX-уведомления ГД об увольнении (ст. 280 ТК РФ) в формате ГОСТ.
/// </summary>
public interface ICeoResignationDocxGenerator
{
    /// <summary>
    /// Генерирует DOCX-файл уведомления для конкретного участника.
    /// </summary>
    Task<byte[]> GenerateAsync(CeoResignationData data, CancellationToken cancellationToken = default);
}
