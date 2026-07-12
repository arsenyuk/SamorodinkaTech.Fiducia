namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Настройки оформления email-писем для юридического лица — header и footer в формате Markdown (legal_entity_email_settings).
/// </summary>
public class LegalEntityEmailSettings
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Юридическое лицо.</summary>
    public LegalEntity? LegalEntity { get; set; }

    /// <summary>Включить header письма (header_enabled).</summary>
    public bool HeaderEnabled { get; set; }

    /// <summary>Содержимое header'а письма в формате Markdown (header_markdown).</summary>
    public string HeaderMarkdown { get; set; } = string.Empty;

    /// <summary>Включить footer письма (footer_enabled).</summary>
    public bool FooterEnabled { get; set; }

    /// <summary>Содержимое footer'а письма в формате Markdown (footer_markdown).</summary>
    public string FooterMarkdown { get; set; } = string.Empty;

    /// <summary>Дата и время последнего обновления (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
