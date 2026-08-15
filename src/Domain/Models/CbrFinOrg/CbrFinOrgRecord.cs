namespace SamorodinkaTech.Fiducia.Domain.Models.CbrFinOrg;

/// <summary>
/// Краткая запись об участнике финансового рынка из результата поиска.
/// Соответствует элементу Record в ответе Search / SearchByINNs / SearchByOGRNs.
/// </summary>
public class CbrFinOrgRecord
{
    /// <summary>Внутренний код организации (Id).</summary>
    public long Id { get; set; }

    /// <summary>ОГРН организации (OGRN).</summary>
    public long? Ogrn { get; set; }

    /// <summary>ИНН организации (INN).</summary>
    public string? Inn { get; set; }

    /// <summary>Наименование организации (Name).</summary>
    public string? Name { get; set; }

    /// <summary>Статус организации: Active / NotActive (Status).</summary>
    public string Status { get; set; } = "";

    /// <summary>Текст ошибки, если запись не найдена (ErrorText).</summary>
    public string? ErrorText { get; set; }
}
