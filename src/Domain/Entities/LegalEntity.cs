namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Юридическое лицо. Минимальная модель для справочника ЮЛ.
/// Связано со справочником ОКОПФ по идентификатору (OkopfId → ref_okopf.id).
/// </summary>
public class LegalEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!; // Наименование ЮЛ
    public string? ShortName { get; set; } // Краткое наименование ЮЛ
    public string? Inn { get; set; } // ИНН (10 знаков для ЮЛ)
    public string? Ogrn { get; set; } // ОГРН (13 знаков)

    /// <summary>Внешний ключ на ОКОПФ (okopf_id).</summary>
    public Guid? OkopfId { get; set; }
    public Okopf? Okopf { get; set; }
}
