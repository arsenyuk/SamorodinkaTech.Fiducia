namespace SamorodinkaTech.Fiducia.Domain.Models.Email;

/// <summary>
/// Данные email-письма для отправки.
/// </summary>
public class EmailMessage
{
    /// <summary>Адрес получателя.</summary>
    public string To { get; init; } = "";

    /// <summary>Тема письма.</summary>
    public string Subject { get; init; } = "";

    /// <summary>Тело письма в формате HTML.</summary>
    public string HtmlBody { get; init; } = "";

    /// <summary>Тело письма в виде простого текста (fallback).</summary>
    public string TextBody { get; init; } = "";
}
