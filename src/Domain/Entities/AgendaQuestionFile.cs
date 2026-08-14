namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Связь вопроса повестки с файлами (agenda_question_files).
/// </summary>
public class AgendaQuestionFile
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на вопрос повестки (agenda_question_id).</summary>
    public Guid AgendaQuestionId { get; set; }

    /// <summary>Ссылка на файл (file_id).</summary>
    public Guid FileId { get; set; }

    /// <summary>Навигация к вопросу повестки.</summary>
    public AgendaQuestion? AgendaQuestion { get; set; }

    /// <summary>Навигация к файлу.</summary>
    public FileEntry? File { get; set; }
}
