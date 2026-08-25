namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Расширенная карточка компании из СПАРК — SOAP-метод GetCompanyExtendedReport.
/// Содержит дополнительные поля по сравнению с ShortReport.
/// </summary>
public class SparkCompanyExtended
{
    // ── Основные поля (как в SparkCompany) ──────────────────────────

    public int SparkId { get; init; }
    public string Inn { get; init; } = default!;
    public string? Kpp { get; init; }
    public string? Ogrn { get; init; }
    public string? Okpo { get; init; }
    public string FullName { get; init; } = default!;
    public string? ShortName { get; init; }
    public string? OkopfCode { get; init; }
    public string? OkopfName { get; init; }
    public string? LegalAddress { get; init; }
    public bool IsActing { get; init; }
    public string? Status { get; init; }
    public DateTime? RegistrationDate { get; init; }
    public decimal? CharterCapital { get; init; }

    // ── Расширенные поля ────────────────────────────────────────────

    /// <summary>Дата ликвидации.</summary>
    public DateTime? LiquidationDate { get; init; }

    /// <summary>Причина ликвидации.</summary>
    public string? LiquidationReason { get; init; }

    /// <summary>Основной вид деятельности (ОКВЭД).</summary>
    public string? OkvedMain { get; init; }

    /// <summary>Наименование основного ОКВЭД.</summary>
    public string? OkvedMainName { get; init; }

    /// <summary>Дополнительные виды деятельности (ОКВЭД).</summary>
    public List<string> OkvedAdditional { get; init; } = new();

    /// <summary>Телефон.</summary>
    public string? Phone { get; init; }

    /// <summary>Email.</summary>
    public string? Email { get; init; }

    /// <summary>Сайт.</summary>
    public string? Website { get; init; }

    /// <summary>Численность работников.</summary>
    public int? EmployeesCount { get; init; }

    /// <summary>Средняя зарплата.</summary>
    public decimal? AverageSalary { get; init; }

    /// <summary>Выручка (тыс. руб.).</summary>
    public decimal? Revenue { get; init; }

    /// <summary>Чистая прибыль (тыс. руб.).</summary>
    public decimal? NetProfit { get; init; }

    /// <summary>Дата последней отчётности.</summary>
    public DateTime? LastReportDate { get; init; }

    /// <summary>Регистрационный номер в ЕГРЮЛ.</summary>
    public string? EgrulRegNumber { get; init; }

    /// <summary>Дата записи в ЕГРЮЛ.</summary>
    public DateTime? EgrulRegDate { get; init; }

    /// <summary>Налоговый орган.</summary>
    public string? TaxAuthority { get; init; }

    /// <summary>Регистрирующий орган.</summary>
    public string? RegistrationAuthority { get; init; }
}
