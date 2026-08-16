namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Карточка организации из справочника ЦБ РФ — внешний кэш (ext_cbr_finorg_organization).
/// Не является авторитетным источником. Обновляется только через API FinOrg.asmx.
/// TTL кэша: 24 часа (fetched_at).
/// </summary>
public class ExtCbrFinOrgOrganization
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>ИНН организации — ключ поиска (inn).</summary>
    public string Inn { get; set; } = default!;

    /// <summary>Внутренний идентификатор ЦБ (cbr_id).</summary>
    public long? CbrId { get; set; }

    /// <summary>ОГРН организации (ogrn).</summary>
    public string? Ogrn { get; set; }

    /// <summary>Полное наименование (full_name).</summary>
    public string? FullName { get; set; }

    /// <summary>Краткое наименование (short_name).</summary>
    public string? ShortName { get; set; }

    /// <summary>Наименование на английском языке (eng_name).</summary>
    public string? EngName { get; set; }

    /// <summary>Адрес местонахождения (address).</summary>
    public string? Address { get; set; }

    /// <summary>Телефоны (phones).</summary>
    public string? Phones { get; set; }

    /// <summary>Электронная почта (email).</summary>
    public string? Email { get; set; }

    /// <summary>Код ОКАТО региона (okato).</summary>
    public int? Okato { get; set; }

    /// <summary>Субъект РФ (region).</summary>
    public string? Region { get; set; }

    /// <summary>Коды типов организации через запятую (fo_types).</summary>
    public string? FoTypes { get; set; }

    /// <summary>Статус организации: Active / NotActive (status).</summary>
    public string Status { get; set; } = "";

    /// <summary>Членство в саморегулируемой организации (is_sro_member).</summary>
    public bool IsSroMember { get; set; }

    /// <summary>Участие в системе страхования вкладов (is_rss).</summary>
    public bool IsRss { get; set; }

    /// <summary>Участие в системе гарантирования прав участников НПФ по НПО (is_npo).</summary>
    public bool IsNpo { get; set; }

    /// <summary>Участие в системе гарантирования прав застрахованных лиц (is_asv).</summary>
    public bool IsAsv { get; set; }

    /// <summary>Регистрационный номер (reg_number).</summary>
    public int? RegNumber { get; set; }

    /// <summary>БИК организации (bic).</summary>
    public string? Bic { get; set; }

    /// <summary>Статус кредитной организации (bank_status).</summary>
    public string? BankStatus { get; set; }

    /// <summary>Дата регистрации Банком России (registration_date).</summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>Имеет ли организация подразделения (has_branches).</summary>
    public bool HasBranches { get; set; }

    /// <summary>Уставный капитал в рублях (fund_value).</summary>
    public decimal? FundValue { get; set; }

    /// <summary>Веб-сайты через запятую (web_sites).</summary>
    public string? WebSites { get; set; }

    /// <summary>Текст ошибки, если запрос не удался (error).</summary>
    public string? Error { get; set; }

    /// <summary>Временная метка получения данных из API (fetched_at).</summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
