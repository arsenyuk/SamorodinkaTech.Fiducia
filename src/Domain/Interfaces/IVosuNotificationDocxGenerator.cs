using SamorodinkaTech.Fiducia.Domain.Models;

namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Генератор DOCX-уведомления о созыве/изменении повестки ВОСУ в формате ГОСТ.
/// </summary>
public interface IVosuNotificationDocxGenerator
{
    /// <summary>
    /// Генерирует DOCX-файл уведомления для конкретного участника.
    /// </summary>
    /// <param name="data">Данные для заполнения уведомления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Байтовое содержимое DOCX-файла.</returns>
    Task<byte[]> GenerateAsync(VosuNotificationData data, CancellationToken cancellationToken = default);
}
