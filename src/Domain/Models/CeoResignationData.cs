namespace SamorodinkaTech.Fiducia.Domain.Models;

/// <summary>
/// Данные для формирования уведомления участнику об увольнении Генерального директора (ст. 280 ТК РФ).
/// </summary>
public class CeoResignationData
{
    /// <summary>Полное наименование юридического лица.</summary>
    public string LegalEntityName { get; init; } = default!;

    /// <summary>ОГРН юридического лица.</summary>
    public string? LegalEntityOgrn { get; init; }

    /// <summary>ИНН юридического лица.</summary>
    public string? LegalEntityInn { get; init; }

    /// <summary>ФИО участника (Фамилия Имя Отчество).</summary>
    public string ParticipantFullName { get; init; } = default!;

    /// <summary>Адрес участника для корреспонденции.</summary>
    public string? ParticipantAddress { get; init; }

    /// <summary>ФИО Генерального директора (уступающего полномочия).</summary>
    public string CeoName { get; init; } = default!;

    /// <summary>Плановая дата увольнения ГД.</summary>
    public DateOnly ResignationDate { get; init; }

    /// <summary>Дата формирования уведомления.</summary>
    public DateOnly NotificationDate { get; init; }

    /// <summary>Место ознакомления с документами.</summary>
    public string? ReviewLocation { get; init; }

    /// <summary>Повестка ВОСУ (список вопросов).</summary>
    public List<string> AgendaItems { get; init; } = new();
}
