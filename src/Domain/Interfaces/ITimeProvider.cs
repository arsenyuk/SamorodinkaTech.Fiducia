namespace SamorodinkaTech.Fiducia.Domain.Interfaces;

/// <summary>
/// Абстракция системного времени для соблюдения принципа инверсии зависимостей (SOLID: DIP).
/// Позволяет тестировать времязависимую логику с контролируемыми значениями часов.
/// </summary>
public interface ITimeProvider
{
    /// <summary>Текущее время в UTC.</summary>
    DateTime UtcNow { get; }
}
