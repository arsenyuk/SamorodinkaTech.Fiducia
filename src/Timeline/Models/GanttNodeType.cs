namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Тип узла в диаграмме Ганта.</summary>
public enum GanttNodeType
{
    /// <summary>Проект (корневой группирующий узел, обязателен).</summary>
    Project,

    /// <summary>Этап (группирующая строка с дочерними задачами/вехами, опционален).</summary>
    Stage,

    /// <summary>Задача (имеет даты начала/окончания).</summary>
    Task,

    /// <summary>Веха (точечная дата, не имеет продолжительности).</summary>
    Milestone
}