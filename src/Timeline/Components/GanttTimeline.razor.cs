using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using SamorodinkaTech.Fiducia.Timeline.Models;

namespace SamorodinkaTech.Fiducia.Timeline.Components;

/// <summary>Временная шкала — строка недель + строка дней календаря.</summary>
public abstract class GanttTimelineBase : ComponentBase
{
    [Inject] private IJSRuntime Js { get; set; } = default!;
    [Inject] private ILogger<GanttTimelineBase> Logger { get; set; } = default!;

    /// <summary>Входные данные (календарь дней).</summary>
    [Parameter] public TimelineInput? Data { get; set; }

    /// <summary>Ширина одной дневной ячейки в пикселях.</summary>
    [Parameter] public int DayCellWidth { get; set; } = 30;

    protected IReadOnlyList<CalendarDay>? _days;
    protected int _dayWidth;
    protected int _totalWidthPx;
    protected DateOnly _today;
    protected List<(int WeekNumber, int Span)> _weeks = new();
    protected List<(int Year, string Label, int Span)> _years = new();
    protected bool _showQuarters = true;
    protected bool _showWeeks = true;
    protected List<(int Year, int Quarter, string Label, int Span)> _quarters = new();
    protected List<(int Year, int Month, string Label, int Span)> _months = new();
    private int _todayOffset;
    protected ElementReference _scrollRef;
    private bool _scrolled;

    protected override void OnParametersSet()
    {
        if (Data?.CalendarDays is { Count: > 0 } days)
        {
            _days = days;
            _dayWidth = DayCellWidth;
            _totalWidthPx = days.Count * _dayWidth;
            _today = Data.Today;
            ComputeWeeks();
            ComputeMonths();
            ComputeQuarters();
            ComputeYears();
            ComputeTodayOffset();
        }
    }

    private void ComputeWeeks()
    {
        _weeks.Clear();
        if (_days == null || _days.Count == 0) return;

        var currentWeek = _days[0].WeekNumber;
        var span = 0;

        foreach (var d in _days)
        {
            if (d.WeekNumber != currentWeek)
            {
                _weeks.Add((currentWeek, span));
                currentWeek = d.WeekNumber;
                span = 0;
            }
            span++;
        }
        _weeks.Add((currentWeek, span));
    }

    private void ComputeMonths()
    {
        _months.Clear();
        if (_days == null || _days.Count == 0) return;

        var monthNames = new[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
        var currentKey = (_days[0].Year, _days[0].MonthNumber);
        var span = 0;

        foreach (var d in _days)
        {
            var key = (d.Year, d.MonthNumber);
            if (key != currentKey)
            {
                _months.Add((currentKey.Year, currentKey.MonthNumber, monthNames[currentKey.MonthNumber - 1], span));
                currentKey = key;
                span = 0;
            }
            span++;
        }
        _months.Add((currentKey.Year, currentKey.MonthNumber, monthNames[currentKey.MonthNumber - 1], span));
    }

    private void ComputeQuarters()
    {
        _quarters.Clear();
        if (_days == null || _days.Count == 0) return;

        var quarterLabels = new[] { "I квартал", "II квартал", "III квартал", "IV квартал" };
        var currentKey = (_days[0].Year, _days[0].Quarter);
        var span = 0;

        foreach (var d in _days)
        {
            var key = (d.Year, d.Quarter);
            if (key != currentKey)
            {
                _quarters.Add((currentKey.Year, currentKey.Quarter, quarterLabels[currentKey.Quarter - 1], span));
                currentKey = key;
                span = 0;
            }
            span++;
        }
        _quarters.Add((currentKey.Year, currentKey.Quarter, quarterLabels[currentKey.Quarter - 1], span));
    }

    private void ComputeYears()
    {
        _years.Clear();
        if (_days == null || _days.Count == 0) return;

        var currentYear = _days[0].Year;
        var span = 0;

        foreach (var d in _days)
        {
            if (d.Year != currentYear)
            {
                _years.Add((currentYear, $"{currentYear} год", span));
                currentYear = d.Year;
                span = 0;
            }
            span++;
        }
        _years.Add((currentYear, $"{currentYear} год", span));
    }

    private void ComputeTodayOffset()
    {
        if (_days == null) return;
        for (var i = 0; i < _days.Count; i++)
        {
            if (_days[i].Date == _today)
            {
                _todayOffset = i * _dayWidth;
                return;
            }
        }
        _todayOffset = 0;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_scrolled && _todayOffset > 0)
        {
            _scrolled = true;
            try
            {
                Logger.LogWarning("Gantt scrollTo сегодня: offset={Offset}px", _todayOffset);
                await Js.InvokeVoidAsync("FiduciaTimeline.scrollToToday", _scrollRef, _todayOffset);
                Logger.LogWarning("Gantt scrollTo выполнен");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Gantt scrollTo отложен (prerendering)");
                _scrolled = false;
            }
        }
    }
}