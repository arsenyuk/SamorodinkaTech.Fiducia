using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация провайдера системного времени (SOLID: SRP).
/// Единственная ответственность — предоставление текущего времени.
/// </summary>
public class SystemTimeProvider : ITimeProvider
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
