namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Лицензия организации из справочника ЦБ РФ — внешний кэш (ext_cbr_finorg_license).
/// Не является авторитетным источником. Обновляется только через API FinOrg.asmx.
/// </summary>
public class ExtCbrFinOrgLicense
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>ИНН организации — внешний ключ (organization_inn).</summary>
    public string OrganizationInn { get; set; } = default!;

    /// <summary>Код вида деятельности (vid_id).</summary>
    public int VidId { get; set; }

    /// <summary>Наименование вида деятельности (activity_name).</summary>
    public string? ActivityName { get; set; }

    /// <summary>Номер лицензии (number).</summary>
    public string? Number { get; set; }

    /// <summary>Наименование лицензии (name).</summary>
    public string? Name { get; set; }

    /// <summary>Дата начала действия лицензии (start_date).</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Дата прекращения действия лицензии (end_date). null = бессрочная.</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Временная метка получения данных из API (fetched_at).</summary>
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
