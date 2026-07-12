using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SamorodinkaTech.Fiducia.Timeline.Models;

namespace SamorodinkaTech.Fiducia.Timeline.Components;

/// <summary>Диаграмма Ганта: структура проекта + временная шкала с барами.</summary>
public partial class GanttChart : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime Js { get; set; } = default!;

    /// <summary>Входные данные: узлы проекта, календарь, даты.</summary>
    [Parameter] public GanttChartInput? Data { get; set; }

    /// <summary>Высота строки в пикселях.</summary>
    [Parameter] public int RowHeight { get; set; } = 32;

    /// <summary>Отступ снизу рабочей области в пикселях (по умолчанию 40).</summary>
    [Parameter] public int BottomPaddingPx { get; set; } = 40;

    /// <summary>Цвет бара задачи.</summary>
    [Parameter] public string TaskColor { get; set; } = "#42a5f5";

    /// <summary>Цвет бара этапа/проекта.</summary>
    [Parameter] public string StageColor { get; set; } = "#78909c";

    /// <summary>Цвет вехи.</summary>
    [Parameter] public string MilestoneColor { get; set; } = "#9c27b0";

    /// <summary>Цвет прогресса внутри бара.</summary>
    [Parameter] public string ProgressColor { get; set; } = "rgba(0,0,0,0.15)";

    /// <summary>Цвет бара просроченной задачи (endDate < today).</summary>
    [Parameter] public string OverdueColor { get; set; } = "#d32f2f";

    /// <summary>Шаблон содержимого tooltip-а. Страница формирует, компонент позиционирует.</summary>
    [Parameter] public RenderFragment<GanttNode>? TooltipTemplate { get; set; }

    /// <summary>Колбэк двойного клика по узлу (задача/веха).</summary>
    [Parameter] public EventCallback<GanttNode> OnNodeDoubleClick { get; set; }

    // ── Состояние ──────────────────────────────────────────────────────

    /// <summary>Ширина вехи в пикселях.</summary>
    private const int MilestoneWidthPx = 12;

    /// <summary>Половина ширины вехи (для центрирования).</summary>
    private const int MilestoneHalfWidthPx = 6;

    /// <summary>Минимальная ширина бара задачи в пикселях.</summary>
    private const int MinBarWidthPx = 2;

    /// <summary>Отступ в пикселях при прокрутке к текущей дате.</summary>
    private const int ScrollToTodayOffsetPx = 200;

    /// <summary>Отступ tooltip в пикселях от верхнего края бара.</summary>
    private const int TooltipTopOffsetPx = 4;

    /// <summary>Ширина ячейки дня в пикселях (масштаб: дни).</summary>
    private const int DaysScaleCellWidthPx = 30;

    /// <summary>Ширина ячейки недели в пикселях (масштаб: недели).</summary>
    private const int WeeksScaleCellWidthPx = 20;

    /// <summary>Ширина ячейки месяца в пикселях (масштаб: месяцы).</summary>
    private const int MonthsScaleCellWidthPx = 3;

    /// <summary>Ширина ячейки квартала в пикселях (масштаб: кварталы).</summary>
    private const int QuartersScaleCellWidthPx = 3;

    /// <summary>Ширина ячейки года в пикселях (масштаб: годы).</summary>
    private const int YearsScaleCellWidthPx = 4;

    private TimelineScale _scale = TimelineScale.Days;
    private int _dayCellWidth = DaysScaleCellWidthPx;
    private TimelineInput? _timelineInput;
    private List<FlatRow> _visibleRows = [];
    private List<GanttBarInfo> _barInfos = [];
    private List<DependencyLine> _depLines = [];
    private int _totalWidthPx;
    private int _totalHeightPx;
    private double _todayLeftPx;
    private int _leftHeaderHeightPx = 67;
    private bool _chkQuarters;
    private bool _chkWeeks;
    private bool _chkDecades;
    private bool _chkDayNames = true;
    private bool _showSettings;
    private bool _tmpQuarters;
    private bool _tmpWeeks;
    private bool _tmpDecades;
    private bool _tmpDayNames;
    private ElementReference _leftScrollRef;
    private ElementReference _rightScrollRef;
    private ElementReference _timelineHeaderRef;
    private DotNetObjectReference<GanttChart>? _dotNetRef;
    private GanttNode? _hoveredNode;
    private int _tooltipLeftPx;
    private int _tooltipTopPx;

    // ── Жизненный цикл ─────────────────────────────────────────────────

    protected override void OnParametersSet()
    {
        if (Data?.CalendarDays is { Count: > 0 })
        {
            BuildTimelineInput();
            BuildVisibleRows();
            ComputeBarPositions();
            ComputeDependencies();
            ComputeStripeInfo();
            ComputeTodayPosition();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_visibleRows.Count > 0)
        {
            if (firstRender)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await Js.InvokeVoidAsync("FiduciaGantt.bindVerticalSync", _leftScrollRef, _rightScrollRef);
                await Js.InvokeVoidAsync("FiduciaGantt.bindHorizontalSync", _timelineHeaderRef, _rightScrollRef);
                await Js.InvokeVoidAsync("FiduciaGantt.observeHeaderHeight", _timelineHeaderRef, _dotNetRef);
                await ScrollToTodayInternal();
            }
        }
    }

    // ── Построение временной шкалы ─────────────────────────────────────

    private void BuildTimelineInput()
    {
        if (Data?.CalendarDays is not { Count: > 0 } days) return;

        var startDate = days[0].Date;
        var endDate = days[^1].Date;

        // Корректируем границы, если узлы не помещаются в переданный интервал
        if (Data.Nodes is { Count: > 0 })
        {
            var paddingWeeks = Data.EndPaddingWeeks;
            DateOnly? nodeMaxEnd = null;
            DateOnly? nodeMinStart = null;

            foreach (var n in Data.Nodes)
            {
                var (ns, ne) = ResolveNodeDates(n);
                if (ns.HasValue && (nodeMinStart == null || ns.Value < nodeMinStart.Value))
                    nodeMinStart = ns.Value;
                if (ne.HasValue && (nodeMaxEnd == null || ne.Value > nodeMaxEnd.Value))
                    nodeMaxEnd = ne.Value;
                if (n.MilestoneDate.HasValue)
                {
                    var md = n.MilestoneDate.Value;
                    if (nodeMinStart == null || md < nodeMinStart.Value)
                        nodeMinStart = md;
                    if (nodeMaxEnd == null || md > nodeMaxEnd.Value)
                        nodeMaxEnd = md;
                }
            }

            if (nodeMinStart.HasValue && nodeMinStart.Value < startDate)
                startDate = new DateOnly(nodeMinStart.Value.Year, nodeMinStart.Value.Month, 1);

            if (nodeMaxEnd.HasValue)
            {
                var padded = nodeMaxEnd.Value.AddDays(paddingWeeks * 7);
                var monthEnd = new DateOnly(padded.Year, padded.Month, 1).AddMonths(1).AddDays(-1);
                if (monthEnd > endDate)
                    endDate = monthEnd;
            }
        }

        _timelineInput = new TimelineInput
        {
            StartDate = startDate,
            EndDate = endDate,
            Today = Data.Today,
            Scale = _scale,
            CalendarDays = days,
            Holidays = new HashSet<DateOnly>()
        };
        _totalWidthPx = (endDate.DayNumber - startDate.DayNumber + 1) * _dayCellWidth;
    }

    // ── Построение дерева видимых строк ────────────────────────────────

    private void BuildVisibleRows()
    {
        _visibleRows.Clear();
        if (Data?.Nodes is not { Count: > 0 }) return;

        var rootNodes = Data.Nodes
            .Where(n => string.IsNullOrEmpty(n.ParentId))
            .OrderBy(n => n.SortOrder)
            .ToList();

        FlattenNodes(rootNodes, Data.Nodes, 0, _visibleRows);
        _totalHeightPx = _visibleRows.Count * RowHeight + BottomPaddingPx;
    }

    private static void FlattenNodes(IReadOnlyList<GanttNode> nodes, IReadOnlyList<GanttNode> allNodes, int depth, List<FlatRow> result)
    {
        foreach (var node in nodes.OrderBy(n => n.SortOrder))
        {
            result.Add(new FlatRow(node, depth));

            var children = allNodes
                .Where(n => n.ParentId == node.Id)
                .OrderBy(n => n.SortOrder)
                .ToList();

            if (children.Count > 0 && node.Expanded)
                FlattenNodes(children, allNodes, depth + 1, result);
        }
    }

    // ── Вычисление позиций баров ───────────────────────────────────────

    private void ComputeBarPositions()
    {
        _barInfos.Clear();
        if (Data?.CalendarDays is not { Count: > 0 }) return;

        var chartStart = Data.CalendarDays[0].Date;

        for (var i = 0; i < _visibleRows.Count; i++)
        {
            var node = _visibleRows[i].Node;
            var (startDate, endDate) = ResolveNodeDates(node);

            if (!startDate.HasValue) continue;

            if (node.NodeType == GanttNodeType.Milestone)
            {
                var cellIdx = startDate.Value.DayNumber - chartStart.DayNumber;
                var centerPx = (int)(cellIdx * _dayCellWidth + _dayCellWidth / 2.0);
                _barInfos.Add(new GanttBarInfo
                {
                    RowIndex = i,
                    LeftPx = centerPx - MilestoneHalfWidthPx,
                    WidthPx = MilestoneWidthPx,
                    NodeType = GanttNodeType.Milestone,
                    Progress = node.Progress,
                    IsMilestone = true
                });
            }
            else if (endDate.HasValue)
            {
                var startCell = startDate.Value.DayNumber - chartStart.DayNumber;
                var endCell = endDate.Value.AddDays(1).DayNumber - chartStart.DayNumber;
                var leftPx = startCell * _dayCellWidth;
                var widthPx = Math.Max(MinBarWidthPx, (endCell - startCell) * _dayCellWidth);
                _barInfos.Add(new GanttBarInfo
                {
                    RowIndex = i,
                    LeftPx = leftPx,
                    WidthPx = widthPx,
                    NodeType = node.NodeType,
                    Progress = node.Progress,
                    IsMilestone = false
                });
            }
            else
            {
                // Бар на 1 день: startDate есть, endDate нет
                var startCell = startDate.Value.DayNumber - chartStart.DayNumber;
                var leftPx = startCell * _dayCellWidth;
                var widthPx = _dayCellWidth;
                _barInfos.Add(new GanttBarInfo
                {
                    RowIndex = i,
                    LeftPx = leftPx,
                    WidthPx = widthPx,
                    NodeType = node.NodeType,
                    Progress = node.Progress,
                    IsMilestone = false
                });
            }
        }
    }

    private void ComputeDependencies()
    {
        _depLines.Clear();
        if (Data?.Dependencies is not { Count: > 0 }) return;

        foreach (var dep in Data.Dependencies)
        {
            var fromIdx = _visibleRows.FindIndex(r => r.Node.Id == dep.FromId);
            var toIdx = _visibleRows.FindIndex(r => r.Node.Id == dep.ToId);
            if (fromIdx < 0 || toIdx < 0) continue;

            var fromBar = _barInfos.FirstOrDefault(b => b.RowIndex == fromIdx);
            var toBar = _barInfos.FirstOrDefault(b => b.RowIndex == toIdx);
            if (fromBar.Equals(default(GanttBarInfo)) || toBar.Equals(default(GanttBarInfo))) continue;

            var x1 = fromBar.LeftPx + fromBar.WidthPx;
            var y1 = fromIdx * RowHeight + RowHeight / 2;
            var x2 = toBar.LeftPx;
            var y2 = toIdx * RowHeight + RowHeight / 2;

            _depLines.Add(new DependencyLine(x1, y1, x2, y2, dep.Color, dep.Style, dep.Thickness));
        }
    }

    /// <summary>Определяет даты начала/окончания для узла.
    /// Для этапа и задачи — собственные StartDate/EndDate (null — нет бара).
    /// Для вехи — дата вехи как start.
    /// Для схлопнутого проекта — агрегат min(start) / max(end) детей.</summary>
    private (DateOnly? Start, DateOnly? End) ResolveNodeDates(GanttNode node)
    {
        return node.NodeType switch
        {
            GanttNodeType.Milestone => (node.MilestoneDate, null),
            GanttNodeType.Task => (node.StartDate, node.EndDate),
            GanttNodeType.Stage => (node.StartDate, node.EndDate),
            GanttNodeType.Project when !node.Expanded => ComputeProjectDates(node),
            _ => (null, null)
        };
    }

    private (DateOnly? Start, DateOnly? End) ComputeProjectDates(GanttNode project)
    {
        if (Data?.Nodes is not { Count: > 0 }) return (null, null);

        DateOnly? minStart = null;
        DateOnly? maxEnd = null;

        foreach (var child in Data.Nodes.Where(n => n.ParentId == project.Id))
        {
            var (s, e) = ResolveNodeDates(child);
            if (s.HasValue && (minStart == null || s.Value < minStart.Value))
                minStart = s.Value;
            if (e.HasValue && (maxEnd == null || e.Value > maxEnd.Value))
                maxEnd = e.Value;
        }

        return (minStart, maxEnd);
    }

    // ── Действия ───────────────────────────────────────────────────────

    private void ComputeTodayPosition()
    {
        if (Data?.CalendarDays is not { Count: > 0 }) return;
        var chartStart = Data.CalendarDays[0].Date;
        _todayLeftPx = (Data.Today.DayNumber - chartStart.DayNumber) * _dayCellWidth + _dayCellWidth / 2.0;
    }

    private void ToggleExpand(GanttNode node)
    {
        node.Expanded = !node.Expanded;
        BuildVisibleRows();
        ComputeBarPositions();
        ComputeDependencies();
        ComputeStripeInfo();
        StateHasChanged();
    }

    private void SetScale(TimelineScale scale)
    {
        _scale = scale;
        _dayCellWidth = scale switch
        {
            TimelineScale.Days => DaysScaleCellWidthPx,
            TimelineScale.Weeks => WeeksScaleCellWidthPx,
            TimelineScale.Months => MonthsScaleCellWidthPx,
            TimelineScale.Quarters => QuartersScaleCellWidthPx,
            TimelineScale.Years => YearsScaleCellWidthPx,
            _ => DaysScaleCellWidthPx
        };
        BuildTimelineInput();
        ComputeBarPositions();
        ComputeDependencies();
        ComputeStripeInfo();
        ComputeTodayPosition();
        StateHasChanged();
    }

    private async Task ScrollToTodayInternal()
    {
        if (Data?.CalendarDays is not { Count: > 0 }) return;
        var chartStart = Data.CalendarDays[0].Date;
        var scrollLeft = (int)((Data.Today.DayNumber - chartStart.DayNumber) * _dayCellWidth) - ScrollToTodayOffsetPx;
        try { await Js.InvokeVoidAsync("FiduciaGantt.scrollTo", _rightScrollRef, Math.Max(0, scrollLeft)); }
        catch { }
    }

    private async Task ScrollToToday() => await ScrollToTodayInternal();

    private void OpenSettings()
    {
        _tmpQuarters = _chkQuarters;
        _tmpWeeks = _chkWeeks;
        _tmpDecades = _chkDecades;
        _tmpDayNames = _chkDayNames;
        _showSettings = true;
        StateHasChanged();
    }

    private void ApplySettings()
    {
        _chkQuarters = _tmpQuarters;
        _chkWeeks = _tmpWeeks;
        _chkDecades = _tmpDecades;
        _chkDayNames = _tmpDayNames;
        _showSettings = false;
        StateHasChanged();
    }

    private void CancelSettings()
    {
        _showSettings = false;
        StateHasChanged();
    }

    private async Task ScrollToRow(GanttNode node)
    {
        var bar = _barInfos.FirstOrDefault(b =>
            b.RowIndex < _visibleRows.Count && _visibleRows[b.RowIndex].Node.Id == node.Id);

        if (bar.Equals(default(GanttBarInfo))) return;

        var scrollLeft = (int)bar.LeftPx - ScrollToTodayOffsetPx;
        try { await Js.InvokeVoidAsync("FiduciaGantt.scrollTo", _rightScrollRef, Math.Max(0, scrollLeft)); }
        catch { }
    }

    // ── Tooltip ─────────────────────────────────────────────────────────

    private void OnBarMouseOver(GanttNode node, GanttBarInfo bar)
    {
        if (TooltipTemplate == null) return;
        _hoveredNode = node;
        _tooltipLeftPx = bar.LeftPx + bar.WidthPx / 2;
        _tooltipTopPx = (bar.RowIndex + 1) * RowHeight + TooltipTopOffsetPx;
        StateHasChanged();
    }

    private void OnBarMouseOut()
    {
        if (_hoveredNode == null) return;
        _hoveredNode = null;
        StateHasChanged();
    }

    // ── JS-колбэк: изменение высоты заголовка шкалы ──────────────────

    /// <summary>Вызывается из JS ResizeObserver при изменении высоты GanttTimeline.</summary>
    [JSInvokable]
    public void OnHeaderHeightChanged(double height)
    {
        var h = (int)Math.Ceiling(height);
        if (h > 0 && Math.Abs(h - _leftHeaderHeightPx) > 2)
        {
            _leftHeaderHeightPx = h;
            InvokeAsync(StateHasChanged);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            _dotNetRef?.Dispose();
            await Js.InvokeVoidAsync("FiduciaGantt.unbindVerticalSync", _leftScrollRef, _rightScrollRef);
            await Js.InvokeVoidAsync("FiduciaGantt.disconnectHeaderObserver", _timelineHeaderRef);
        }
        catch { }
    }

    // ── stripes helpers ────────────────────────────────────────────

    /// <summary>Дни-полосы (выходные/праздники) для быстрого поиска.</summary>
    private HashSet<int>? _stripeDayIndices;

    /// <summary>Уникальные индексы строк, пересекающие зоны полос зебры.</summary>
    private HashSet<int> _barStripeRows = new();

    private void ComputeStripeInfo()
    {
        if (Data?.CalendarDays is null || Data.CalendarDays.Count == 0) return;

        var stripeDays = new HashSet<int>();
        foreach (var (d, idx) in Data.CalendarDays.Select((v, i) => (v, i)))
            if (d.IsWeekend || d.IsHoliday) stripeDays.Add(idx);

        _stripeDayIndices = stripeDays;
        _barStripeRows.Clear();

        foreach (var bar in _barInfos)
        {
            var s = bar.LeftPx / Math.Max(1, _dayCellWidth);
            var e = (bar.LeftPx + bar.WidthPx) / Math.Max(1, _dayCellWidth) + 2;
            for (var i = s; i <= e && i < Data.CalendarDays.Count; i++)
                if (stripeDays.Contains(i)) { _barStripeRows.Add(bar.RowIndex); break; }
        }
    }

    private sealed record FlatRow(GanttNode Node, int Depth);

    private sealed record DependencyLine(
        int X1, int Y1, int X2, int Y2,
        string Color, string Style, int Thickness);
}