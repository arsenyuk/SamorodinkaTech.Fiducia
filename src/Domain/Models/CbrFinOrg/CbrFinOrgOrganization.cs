namespace SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

/// <summary>
/// Полная информация об участнике финансового рынка (УФР) из справочника ЦБ РФ.
/// Соответствует элементу FullInfo в ответе GetFullInfoByINN / GetFullInfoByOGRN.
/// </summary>
public class CbrFinOrgOrganization
{
    /// <summary>Уникальный идентификатор организации (ID).</summary>
    public long Id { get; set; }

    /// <summary>ОГРН организации.</summary>
    public long? Ogrn { get; set; }

    /// <summary>ИНН организации.</summary>
    public string? Inn { get; set; }

    /// <summary>Краткое наименование финансовой организации (ShortName).</summary>
    public string? ShortName { get; set; }

    /// <summary>Полное наименование финансовой организации (Name).</summary>
    public string? Name { get; set; }

    /// <summary>Наименование на английском языке (EngName).</summary>
    public string? EngName { get; set; }

    /// <summary>Адрес местонахождения (Address).</summary>
    public string? Address { get; set; }

    /// <summary>Телефоны (Phones).</summary>
    public string? Phones { get; set; }

    /// <summary>Электронная почта (Email).</summary>
    public string? Email { get; set; }

    /// <summary>Код ОКАТО региона (OKATO).</summary>
    public int? Okato { get; set; }

    /// <summary>Субъект РФ (Reg).</summary>
    public string? Region { get; set; }

    /// <summary>Коды типов организации (FOTypes).</summary>
    public List<string> FoTypes { get; set; } = new();

    /// <summary>Статус организации: Active / NotActive / Any / Unknown (Status).</summary>
    public string Status { get; set; } = "";

    /// <summary>Членство в саморегулируемой организации (IsSroMember).</summary>
    public bool IsSroMember { get; set; }

    /// <summary>Участие в системе страхования вкладов (IsRss).</summary>
    public bool IsRss { get; set; }

    /// <summary>Участие в системе гарантирования прав участников НПФ по НПО (NPO_FLG).</summary>
    public bool IsNpo { get; set; }

    /// <summary>Участие в системе гарантирования прав застрахованных лиц (ASV_FLG).</summary>
    public bool IsAsv { get; set; }

    /// <summary>Регистрационный номер (REGNUM).</summary>
    public int? RegNumber { get; set; }

    /// <summary>БИК организации (BIC).</summary>
    public string? Bic { get; set; }

    /// <summary>Статус кредитной организации (BnkStatus).</summary>
    public string? BankStatus { get; set; }

    /// <summary>Дата регистрации Банком России (RegistrationDate).</summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>Имеет ли организация подразделения (HasBranches).</summary>
    public bool HasBranches { get; set; }

    /// <summary>Информация об уставном капитале (Fund).</summary>
    public CbrFinOrgFundInfo? Fund { get; set; }

    /// <summary>Список лицензий организации (LicList).</summary>
    public List<CbrFinOrgLicense> Licenses { get; set; } = new();

    /// <summary>Веб-сайты организации (WebSites).</summary>
    public List<string> WebSites { get; set; } = new();

    /// <summary>Текст ошибки, если запрос не удался (Error).</summary>
    public string? Error { get; set; }
}
