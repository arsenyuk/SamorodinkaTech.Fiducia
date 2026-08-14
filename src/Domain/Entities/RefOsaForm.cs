namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник форм проведения ОСА (ref_osa_form).
/// </summary>
public class RefOsaForm
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код формы (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Сокращённое наименование для отображения в списках (short_name).</summary>
    public string? ShortName { get; set; }
    /// <summary>Дата и время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Идентификатор создателя (created_by).</summary>
    public Guid CreatedBy { get; set; }
}
