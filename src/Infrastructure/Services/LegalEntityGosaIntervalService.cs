using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Validation;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Базовая реализация сервисной логики интервала ГОСА по бизнес-процессу.
/// </summary>
public class LegalEntityGosaIntervalService : ILegalEntityGosaIntervalService
{
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// Создаёт сервис с провайдером системного времени (SOLID: DIP).
    /// </summary>
    public LegalEntityGosaIntervalService(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public bool IsPjsc(string? okopfCode) => OkopfTypeMapper.IsPjsc(okopfCode);

    public bool IsLlc(string? okopfCode) => OkopfTypeMapper.IsLlc(okopfCode);

    public (DateOnly start, DateOnly end) GetDefaultWindow() =>
        (new DateOnly(_timeProvider.UtcNow.Year, 3, 1), new DateOnly(_timeProvider.UtcNow.Year, 6, 30));

    public (DateOnly start, DateOnly end) GetWindowForOkopf(string? okopfCode)
    {
        var year = _timeProvider.UtcNow.Year;
        return OkopfTypeMapper.IsLlc(okopfCode)
            ? (new DateOnly(year, 3, 1), new DateOnly(year, 4, 30))
            : (new DateOnly(year, 3, 1), new DateOnly(year, 6, 30));
    }

    public bool ValidateForOkopf(string? okopfCode, DateOnly start, DateOnly end)
    {
        if (end < start) return false;

        if (OkopfTypeMapper.IsPjsc(okopfCode))
        {
            var (min, max) = GetDefaultWindow();
            return start >= min && end <= max;
        }

        // НАО и ООО: только фиксированный интервал согласно ОПФ
        var (defStart, defEnd) = GetWindowForOkopf(okopfCode);
        return start == defStart && end == defEnd;
    }
}
