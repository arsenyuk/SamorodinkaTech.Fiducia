using Microsoft.AspNetCore.Components;
using SamorodinkaTech.Fiducia.Timeline.Models;

namespace SamorodinkaTech.Fiducia.Timeline.Components;

/// <summary>Временная шкала — одна строка дней календаря.</summary>
public abstract class GanttTimelineBase : ComponentBase
{
    /// <summary>Входные данные (календарь дней).</summary>
    [Parameter] public TimelineInput? Data { get; set; }

    /// <summary>Ширина одной дневной ячейки в пикселях.</summary>
    [Parameter] public int DayCellWidth { get; set; } = 30;

    protected IReadOnlyList<CalendarDay>? _days;
    protected int _dayWidth;
    protected int _totalWidthPx;
    protected DateOnly _today;

    protected override void OnParametersSet()
    {
        if (Data?.CalendarDays is { Count: > 0 } days)
        {
            _days = days;
            _dayWidth = DayCellWidth;
            _totalWidthPx = days.Count * _dayWidth;
            _today = Data.Today;
        }
    }
}