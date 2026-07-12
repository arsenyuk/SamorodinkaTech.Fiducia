namespace SamorodinkaTech.Fiducia.Domain.Validation;

/// <summary>
/// Входная модель для серверной валидации сохранения юридического лица.
/// Не зависит от окружения, БД или EF — чистые данные.
/// </summary>
public record LegalEntitySaveValidationModel
{
    /// <summary>Количество акционеров (null — не указано).</summary>
    public int? ShareholdersCount { get; set; }

    /// <summary>Код ОКОПФ (сырая строка из БД, например «12247»).</summary>
    public string? OkopfCode { get; set; }

    /// <summary>Включён ли Совет директоров.</summary>
    public bool HasBoardOfDirectors { get; set; }

    /// <summary>Минимальное количество участников СД (автоматически вычисленное).</summary>
    public int? BoardMinNumber { get; set; }

    /// <summary>Фактическое количество участников СД.</summary>
    public int? BoardMemberNumber { get; set; }

    /// <summary>Начало окна ГОСА.</summary>
    public DateOnly? GosaWindowStart { get; set; }

    /// <summary>Окончание окна ГОСА.</summary>
    public DateOnly? GosaWindowEnd { get; set; }

    /// <summary>Должность руководителя.</summary>
    public string? Position { get; set; }

    /// <summary>ФИО руководителя.</summary>
    public string? FullName { get; set; }
}
