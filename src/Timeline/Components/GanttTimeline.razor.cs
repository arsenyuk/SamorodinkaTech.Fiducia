using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SamorodinkaTech.Fiducia.Timeline.Models;
using SamorodinkaTech.Fiducia.Timeline.Services;

namespace SamorodinkaTech.Fiducia.Timeline.Components;

/// <summary>Code-behind для компонента GanttTimeline.</summary>
public abstract class GanttTimelineBase : ComponentBase, IAsyncDisposable
{
    // ── Parameters ─────────────────────────────────────────────────────

    /// <summary>Callback при клике по делению шкалы (нижний ряд).</summary>
    [Parameter] public EventCallback<DivisionClickEventArgs> OnDivisionClick { get; set; }

    /// <summary>Callback при смене масштаба.</summary>
    [Parameter] public EventCallback<TimelineScale> OnScaleChanged { get; set; }

    /// <summary>Ширина ячейки для масштаба Days (px).</summary>
    [Parameter] public int CellWidthDays { get; set; } = 40;

    /// <summary>Ширина ячейки для масштаба Weeks (px).</summary>
    [Parameter] public int CellWidthWeeks { get; set; } = 80;

    /// <summary>Ширина ячейки для масштаба Months (px).</summary>
    [Parameter] public int CellWidthMonths { get; set; } = 120;

    /// <summary>Ширина ячейки для масштаба Quarters (px).</summary>
    [Parameter] public int CellWidthQuarters { get; set; } = 160;

    /// <summary>Ширина ячейки для масштаба Years (px).</summary>
    [Parameter] public int CellWidthYears { get; set; } = 200;

    // ── Injected services ──────────────────────────────────────────────

    [Inject] private IJSRuntime JS { get; set; } = default!;

    // ── Internal state ─────────────────────────────────────────────────

    protected TimelineScale _scale;
    protected TimelineResult? _result;
    protected int _cellWidthPx;
    protected int _totalWidthPx;
    protected double _todayLinePx = -1;

    protected ElementReference _scrollRef;

    private TimelineInput? _data;
    private DateOnly? _centerDate;
    private IJSObjectReference? _jsModule;
    private bool _initialized;

    // ── Public API ─────────────────────────────────────────────────────

    /// <summary>Инициализирует или обновляет шкалу новыми данными.</summary>
    public async Task SetDataAsync(TimelineInput input)
    {
        _data = input;
        _scale = input.Scale;
        Recalculate();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>Программная смена масштаба.</summary>
    public async Task SetScaleAsync(TimelineScale scale)
    {
        _centerDate = await GetCenterDateAsync();
        _scale = scale;
        Recalculate();
        await InvokeAsync(StateHasChanged);
        await OnScaleChanged.InvokeAsync(scale);
    }

    /// <summary>Программное обновление диапазона дат.</summary>
    public async Task SetDateRangeAsync(DateOnly start, DateOnly end)
    {
        if (_data == null) return;
        _data = _data with { StartDate = start, EndDate = end };
        Recalculate();
        await InvokeAsync(StateHasChanged);
    }

    // ── Lifecycle ──────────────────────────────────────────────────────

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_initialized && _centerDate.HasValue)
        {
            var date = _centerDate.Value;
            _centerDate = null;
            await ScrollToDateAsync(date);
        }

        if (firstRender)
            _initialized = true;
    }

    // ── Recalculation ──────────────────────────────────────────────────

    private void Recalculate()
    {
        if (_data == null) return;

        _cellWidthPx = _scale switch
        {
            TimelineScale.Days => CellWidthDays,
            TimelineScale.Weeks => CellWidthWeeks,
            TimelineScale.Months => CellWidthMonths,
            TimelineScale.Quarters => CellWidthQuarters,
            TimelineScale.Years => CellWidthYears,
            _ => 80
        };

        var input = _data with { Scale = _scale };
        _result = TimelineCalculator.Compute(input);
        _totalWidthPx = _result.Lower.Count * _cellWidthPx;
        _todayLinePx = _result.TodayCellPosition >= 0 ? _result.TodayCellPosition * _cellWidthPx : -1;
    }

    // ── Event handlers ─────────────────────────────────────────────────

    protected async Task ZoomIn()
    {
        if (_scale == TimelineScale.Days) return;
        await SetScaleAsync(_scale - 1);
    }

    protected async Task ZoomOut()
    {
        if (_scale == TimelineScale.Years) return;
        await SetScaleAsync(_scale + 1);
    }

    protected async Task OnScaleSelectChanged(ChangeEventArgs e)
    {
        if (Enum.TryParse<TimelineScale>(e.Value?.ToString(), out var newScale) && newScale != _scale)
            await SetScaleAsync(newScale);
    }

    protected async Task OnCellClicked(TimelineDivision div)
    {
        await OnDivisionClick.InvokeAsync(new DivisionClickEventArgs
        {
            Start = div.Start,
            End = div.End,
            UnitType = _scale
        });
    }

    // ── Scroll / centering ─────────────────────────────────────────────

    private async Task<DateOnly> GetCenterDateAsync()
    {
        if (_data == null || _result == null || _result.Lower.Count == 0)
            return _data?.StartDate ?? DateOnly.MinValue;

        try
        {
            var module = await GetJsModuleAsync();
            var info = await module.InvokeAsync<ScrollInfo>("getScrollInfo", _scrollRef);
            if (info.ScrollWidth <= 0 || _totalWidthPx <= 0)
                return _data.StartDate;

            var centerPx = info.ScrollLeft + info.ClientWidth / 2.0;
            var ratio = Math.Clamp(centerPx / _totalWidthPx, 0, 1);
            var totalDays = _data.EndDate.DayNumber - _data.StartDate.DayNumber;
            var offsetDays = (int)(ratio * totalDays);
            return _data.StartDate.AddDays(offsetDays);
        }
        catch
        {
            return _data.StartDate;
        }
    }

    private async Task ScrollToDateAsync(DateOnly date)
    {
        if (_result == null || _result.Lower.Count == 0) return;

        // Find pixel position of the date within the new timeline
        var totalDays = _data!.EndDate.DayNumber - _data.StartDate.DayNumber;
        if (totalDays <= 0) return;

        var offsetDays = date.DayNumber - _data.StartDate.DayNumber;
        var ratio = Math.Clamp((double)offsetDays / totalDays, 0, 1);
        var posPx = ratio * _totalWidthPx;

        try
        {
            var module = await GetJsModuleAsync();
            var info = await module.InvokeAsync<ScrollInfo>("getScrollInfo", _scrollRef);
            var targetScroll = posPx - info.ClientWidth / 2.0;
            targetScroll = Math.Clamp(targetScroll, 0, _totalWidthPx - info.ClientWidth);
            await module.InvokeVoidAsync("scrollTo", _scrollRef, targetScroll, false);
        }
        catch
        {
            // JS interop failed — non-critical
        }
    }

    private async Task<IJSObjectReference> GetJsModuleAsync()
    {
        if (_jsModule == null)
        {
            _jsModule = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./_content/SamorodinkaTech.Fiducia.Timeline/gantt-timeline.js");
        }
        return _jsModule;
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            try { await _jsModule.DisposeAsync(); }
            catch { /* JS runtime may already be disposed */ }
        }
    }

    // ── JS interop types ───────────────────────────────────────────────

    private class ScrollInfo
    {
        public double ScrollLeft { get; set; }
        public double ClientWidth { get; set; }
        public double ScrollWidth { get; set; }
    }
}