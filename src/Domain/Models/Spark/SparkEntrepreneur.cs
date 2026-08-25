namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Карточка ИП из СПАРК — SOAP-метод GetEnterpreneurShortReport.
/// </summary>
public class SparkEntrepreneur
{
    /// <summary>Идентификатор в СПАРК (SparkID).</summary>
    public int SparkId { get; init; }

    /// <summary>ИНН ИП (12 знаков).</summary>
    public string Inn { get; init; } = default!;

    /// <summary>ОГРНИП (15 знаков).</summary>
    public string? Ogrnip { get; init; }

    /// <summary>ФИО ИП.</summary>
    public string FullName { get; init; } = default!;

    /// <summary>Дата регистрации ИП.</summary>
    public DateTime? RegistrationDate { get; init; }

    /// <summary>Дата прекращения деятельности (если ИП закрыт).</summary>
    public DateTime? CancellationDate { get; init; }

    /// <summary>Статус ИП.</summary>
    public string? Status { get; init; }

    /// <summary>Признак действующего ИП.</summary>
    public bool IsActing { get; init; }

    /// <summary>Адрес регистрации.</summary>
    public string? Address { get; init; }

    /// <summary>Код ОКВЭД основного вида деятельности.</summary>
    public string? OkvedMain { get; init; }

    /// <summary>Наименование основного вида деятельности.</summary>
    public string? OkvedMainName { get; init; }
}
