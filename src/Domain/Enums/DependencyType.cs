namespace SamorodinkaTech.Fiducia.Domain.Enums;

/// <summary>
/// Тип зависимости между задачами/этапами.
/// </summary>
public enum DependencyType
{
    /// <summary>Finish-to-Start (Финиш-Старт). Задача начинается после завершения предшественника.</summary>
    FS = 0,

    /// <summary>Start-to-Start (Старт-Старт). Задача начинается одновременно с предшественником.</summary>
    SS = 1
}
