namespace SamorodinkaTech.Fiducia.Domain.Models.Spark;

/// <summary>
/// Карточка компании из СПАРК — SOAP-метод GetCompanyShortReport.
/// </summary>
public class SparkCompany
{
    /// <summary>Идентификатор компании в СПАРК (SparkID).</summary>
    public int SparkId { get; init; }

    /// <summary>Тип компании: 1=обычная, 2=холдинг, 3=страховая, 4=страховой брокер, 5=банк (CompanyType).</summary>
    public int CompanyType { get; init; }

    /// <summary>ИНН (10 знаков для ЮЛ).</summary>
    public string Inn { get; init; } = default!;

    /// <summary>КПП.</summary>
    public string? Kpp { get; init; }

    /// <summary>ОГРН (13 знаков).</summary>
    public string? Ogrn { get; init; }

    /// <summary>ОКПО.</summary>
    public string? Okpo { get; init; }

    /// <summary>Полное наименование (FullNameRus).</summary>
    public string FullName { get; init; } = default!;

    /// <summary>Краткое наименование (ShortNameRus).</summary>
    public string? ShortName { get; init; }

    /// <summary>Код ОКОПФ (OKOPF/Code).</summary>
    public string? OkopfCode { get; init; }

    /// <summary>Наименование ОКОПФ (OKOPF/Name).</summary>
    public string? OkopfName { get; init; }

    /// <summary>Юридический адрес (LegalAddresses/Address/@Address).</summary>
    public string? LegalAddress { get; init; }

    /// <summary>Признак действующей компании (Status/@IsActing).</summary>
    public bool IsActing { get; init; }

    /// <summary>Текст статуса компании (Status/@Type).</summary>
    public string? Status { get; init; }

    /// <summary>Дата первой регистрации (DateFirstReg).</summary>
    public DateTime? RegistrationDate { get; init; }

    /// <summary>Уставный капитал в рублях (CharterCapital).</summary>
    public decimal? CharterCapital { get; init; }
}
