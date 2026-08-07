namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Данные о руководителе компании из СПАРК — SOAP-метод GetCompanyShortReport (тип LeaderRUS).
/// </summary>
public class SparkManager
{
    /// <summary>ФИО руководителя (LeaderRUS/@FIO).</summary>
    public string FullName { get; init; } = default!;

    /// <summary>Должность (LeaderRUS/@Position).</summary>
    public string? Position { get; init; }

    /// <summary>ИНН руководителя (LeaderRUS/@INN).</summary>
    public string? Inn { get; init; }

    /// <summary>Дата актуальности данных (LeaderRUS/@ActualDate).</summary>
    public DateTime? ActualDate { get; init; }

    /// <summary>Дата потери правоспособности (LeaderRUS/@LegalCapacityEndDate).</summary>
    public DateTime? LegalCapacityEndDate { get; init; }

    /// <summary>Наименование управляющей компании (LeaderRUS/@ManagementCompany).</summary>
    public string? ManagementCompany { get; init; }

    /// <summary>ИНН управляющей компании (LeaderRUS/@ManagementCompanyINN).</summary>
    public string? ManagementCompanyINN { get; init; }
}
