namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Справочник форм проведения ОСА (ref_osa_form).
/// </summary>
public class OsaForm
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Код формы (code).</summary>
    public string Code { get; set; } = default!;

    /// <summary>Наименование (name).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Сокращённое наименование для отображения в списках (short_name).</summary>
    public string? ShortName { get; set; }
}
