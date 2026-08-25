namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Информация об аккаунте СПАРК — SOAP-метод GetStateAccount.
/// Остаток лимита платных запросов.
/// </summary>
public class SparkStateAccount
{
    /// <summary>Текущий баланс (количество доступных запросов).</summary>
    public int Balance { get; init; }

    /// <summary>Общий лимит запросов.</summary>
    public int TotalLimit { get; init; }

    /// <summary>Использовано запросов.</summary>
    public int UsedCount { get; init; }

    /// <summary>Дата окончания действия лицензии.</summary>
    public DateTime? LicenseEndDate { get; init; }

    /// <summary>Наименование тарифного плана.</summary>
    public string? TariffName { get; init; }

    /// <summary>Статус аккаунта.</summary>
    public string? Status { get; init; }
}
