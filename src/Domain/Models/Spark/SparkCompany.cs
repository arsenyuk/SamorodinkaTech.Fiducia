namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Карточка компании из СПАРК — базовые реквизиты и статус.
/// </summary>
public class SparkCompany
{
    /// <summary>ИНН (10 знаков для ЮЛ).</summary>
    public string Inn { get; init; } = default!;

    /// <summary>ОГРН (13 знаков).</summary>
    public string? Ogrn { get; init; }

    /// <summary>Полное наименование.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>Краткое наименование.</summary>
    public string? ShortName { get; init; }

    /// <summary>Код ОКОПФ (например, 12247 — ПАО).</summary>
    public string? OkopfCode { get; init; }

    /// <summary>Наименование ОКОПФ.</summary>
    public string? OkopfName { get; init; }

    /// <summary>Юридический адрес.</summary>
    public string? LegalAddress { get; init; }

    /// <summary>Статус компании (действующая / в процессе ликвидации / ликвидирована).</summary>
    public string? Status { get; init; }

    /// <summary>Дата регистрации.</summary>
    public DateTime? RegistrationDate { get; init; }

    /// <summary>Количество акционеров (если доступно).</summary>
    public int? ShareholdersCount { get; init; }

    /// <summary>Количество сотрудников.</summary>
    public int? EmployeesCount { get; init; }
}
