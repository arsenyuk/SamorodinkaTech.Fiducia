namespace SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Тип связи между задачами.</summary>
public enum GanttDependencyType
{
    /// <summary>Обычная связь.</summary>
    Regular,

    /// <summary>Связь на критическом пути.</summary>
    CriticalPath,

    /// <summary>Предупреждающая связь (просрочка, риск).</summary>
    Warning
}