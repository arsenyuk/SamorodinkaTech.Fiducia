namespace SamorodinkaTech.Fiducia.Domain.Models.DaData;

/// <summary>
/// Financial balance data from DaData.
/// Source: EGRUL financial statements.
/// Note: data may be up to 3 days old. Requires DaData "Professional" tariff.
/// </summary>
public record DaDataBalance
{
    /// <summary>ИНН организации.</summary>
    public string? Inn { get; init; }

    /// <summary>Активы (тыс. руб.).</summary>
    public decimal? Assets { get; init; }

    /// <summary>Собственный капитал (тыс. руб.).</summary>
    public decimal? Equity { get; init; }

    /// <summary>Выручка (тыс. руб.).</summary>
    public decimal? Revenue { get; init; }

    /// <summary>Чистая прибыль (тыс. руб.).</summary>
    public decimal? NetProfit { get; init; }

    /// <summary>Оборотные активы (тыс. руб.).</summary>
    public decimal? CurrentAssets { get; init; }

    /// <summary>Внеоборотные активы (тыс. руб.).</summary>
    public decimal? FixedAssets { get; init; }

    /// <summary>Дебиторская задолженность (тыс. руб.).</summary>
    public decimal? AccountsReceivable { get; init; }

    /// <summary>Кредиторская задолженность (тыс. руб.).</summary>
    public decimal? AccountsPayable { get; init; }

    /// <summary>Убыток (тыс. руб.).</summary>
    public decimal? NetLoss { get; init; }

    /// <summary>Год отчётности.</summary>
    public int? Year { get; init; }

    /// <summary>Дата последнего обновления данных из ЕГРЮЛ (до 3 дней).</summary>
    public DateTime? EgrulUpdatedAt { get; init; }
}
