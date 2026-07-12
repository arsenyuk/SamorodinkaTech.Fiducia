using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Базовая реализация сервисной логики интервала ГОСА по бизнес-процессу.
/// </summary>
public class LegalEntityGosaIntervalService : ILegalEntityGosaIntervalService
{
    private readonly ITimeProvider _timeProvider;

    // Код ОКОПФ ПАО по ОКОПФ: 12247 — Публичные акционерные общества
    private const string PaoOkopfCode = "12247";

    // Код ОКОПФ ООО по ОКОПФ: 12300 — Общества с ограниченной ответственностью
    private const string LlcOkopfCode = "12300";

    /// <summary>
    /// Создаёт сервис с провайдером системного времени (SOLID: DIP).
    /// </summary>
    public LegalEntityGosaIntervalService(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public bool IsPao(string? okopfCode) => string.Equals(okopfCode?.Trim(), PaoOkopfCode, StringComparison.Ordinal);

    private static bool IsLlc(string? okopfCode) => string.Equals(okopfCode?.Trim(), LlcOkopfCode, StringComparison.Ordinal);

    public (DateOnly start, DateOnly end) GetDefaultWindow() =>
        (new DateOnly(_timeProvider.UtcNow.Year, 3, 1), new DateOnly(_timeProvider.UtcNow.Year, 6, 30));

    public (DateOnly start, DateOnly end) GetWindowForOkopf(string? okopfCode)
    {
        var year = _timeProvider.UtcNow.Year;
        return IsLlc(okopfCode)
            ? (new DateOnly(year, 3, 1), new DateOnly(year, 4, 30))
            : (new DateOnly(year, 3, 1), new DateOnly(year, 6, 30));
    }

    public bool ValidateForOkopf(string? okopfCode, DateOnly start, DateOnly end)
    {
        if (end < start) return false;

        if (IsPao(okopfCode))
        {
            var (min, max) = GetDefaultWindow();
            return start >= min && end <= max;
        }

        // АО/НАО и ООО: только фиксированный интервал согласно ОПФ
        var (defStart, defEnd) = GetWindowForOkopf(okopfCode);
        return start == defStart && end == defEnd;
    }
}
