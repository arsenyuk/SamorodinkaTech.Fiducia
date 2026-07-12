namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Карточка компании из СПАРК — внешний кэш (ext_spark_company, BDR-009).
/// Не является авторитетным источником. Обновляется только через API.
/// </summary>
public class ExtSparkCompany
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>ИНН компании — ключ поиска (inn).</summary>
    public string Inn { get; set; } = default!;

    /// <summary>ОГРН (ogrn).</summary>
    public string? Ogrn { get; set; }

    /// <summary>Полное наименование (full_name).</summary>
    public string? FullName { get; set; }

    /// <summary>Краткое наименование (short_name).</summary>
    public string? ShortName { get; set; }

    /// <summary>Код ОКОПФ (okopf_code).</summary>
    public string? OkopfCode { get; set; }

    /// <summary>Наименование ОКОПФ (okopf_name).</summary>
    public string? OkopfName { get; set; }

    /// <summary>Юридический адрес (legal_address).</summary>
    public string? LegalAddress { get; set; }

    /// <summary>Статус компании (status).</summary>
    public string? Status { get; set; }

    /// <summary>Дата регистрации (registration_date).</summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>Количество акционеров (shareholders_count).</summary>
    public int? ShareholdersCount { get; set; }

    /// <summary>Количество сотрудников (employees_count).</summary>
    public int? EmployeesCount { get; set; }

    /// <summary>Временная метка получения данных из API (fetched_at).</summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
