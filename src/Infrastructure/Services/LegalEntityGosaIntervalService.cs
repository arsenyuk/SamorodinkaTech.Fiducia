using SamorodinkaTech.Fiducia.Domain.Entities;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Базовая реализация сервисной логики интервала ГОСА по бизнес-процессу.
/// </summary>
public class LegalEntityGosaIntervalService : ILegalEntityGosaIntervalService
{
    // Код ОКОПФ ПАО по ОКОПФ: 12247 — Публичные акционерные общества
    private const string PaoOkopfCode = "12247";

    public bool IsPao(string? okopfCode) => string.Equals(okopfCode?.Trim(), PaoOkopfCode, StringComparison.Ordinal);

    public (DateOnly start, DateOnly end) GetDefaultWindow() =>
        (new DateOnly(DateTime.UtcNow.Year, 3, 1), new DateOnly(DateTime.UtcNow.Year, 6, 30));

    public bool ValidateForOkopf(string? okopfCode, DateOnly start, DateOnly end)
    {
        if (end < start) return false;

        var (min, max) = GetDefaultWindow();

        if (IsPao(okopfCode))
        {
            // ПАО: любой подинтервал в пределах 01.03–30.06
            return start >= min && end <= max;
        }

        // АО/НАО: только ровно 01.03–30.06
        return start == min && end == max;
    }
}
