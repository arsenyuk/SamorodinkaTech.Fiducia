using SamorodinkaTech.Fiducia.Timeline.Models;

namespace SamorodinkaTech.Fiducia.Timeline.Services;

/// <summary>Вычисляет ряды делений шкалы по входным параметрам.</summary>
public static class TimelineCalculator
{
    private static readonly string[] MonthShort = ["янв", "фев", "мар", "апр", "май", "июн", "июл", "авг", "сен", "окт", "ноя", "дек"];
    private static readonly string[] MonthFull = ["Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"];

    public static TimelineResult Compute(TimelineInput input)
    {
        var lower = ComputeLower(input);
        var upperScale = ResolveUpperScale(input.Scale, input.StartDate, input.EndDate);
        var upper = upperScale == input.Scale ? [] : ComputeUpper(lower, upperScale);
        var todayPos = ComputeTodayPosition(lower, input.Today);

        return new TimelineResult(upper, lower, todayPos);
    }

    /// <summary>Вычисляет все пять уровней одновременно (годы → кварталы → месяцы → недели → дни).</summary>
    public static MultiLevelResult ComputeAllLevels(TimelineInput input)
    {
        var days = input.CalendarDays.Count > 0
            ? ComputeDaysFromCalendar(input.CalendarDays, input.Today)
            : ComputeLowerDays(input);
        if (days.Count == 0)
            return new MultiLevelResult();

        var weeks = GroupAndLabel(days, WeekGroupKey, LabelWeek);
        var months = GroupAndLabel(weeks, d => d.Start.Year * 100 + d.Start.Month, LabelMonth);
        var quarters = GroupAndLabel(months, d => d.Start.Year * 10 + ((d.Start.Month - 1) / 3 + 1), LabelQuarter);
        var years = GroupAndLabel(quarters, d => d.Start.Year, LabelYear);

        var todayPos = ComputeTodayPixel(days, input.Today, 30);

        return new MultiLevelResult
        {
            Years = years,
            Quarters = quarters,
            Months = months,
            Weeks = weeks,
            Days = days,
            TodayPixel = todayPos
        };
    }

    private static int WeekGroupKey(TimelineDivision d) => d.Start.Year * 100 + WeekNumberOf(d.Start);

    private static string LabelWeek(TimelineDivision group)
    {
        var end = group.End;
        if (group.Start.Month == end.Month)
            return $"{group.Start.Day}–{end.Day} {MonthShort[group.Start.Month - 1]}";
        return $"{group.Start.Day} {MonthShort[group.Start.Month - 1]} – {end.Day} {MonthShort[end.Month - 1]}";
    }

    private static string LabelMonth(TimelineDivision group) => MonthShort[group.Start.Month - 1];
    private static string LabelQuarter(TimelineDivision group) => $"Q{(group.Start.Month - 1) / 3 + 1}";
    private static string LabelYear(TimelineDivision group) => group.Start.Year.ToString();

    private static string TooltipWeek(TimelineDivision group) =>
        $"{group.Start.Day} {MonthFull[group.Start.Month - 1]} – {group.End.Day} {MonthFull[group.End.Month - 1]} {group.End.Year}";

    private static List<TimelineDivision> GroupAndLabel(
        List<TimelineDivision> source,
        Func<TimelineDivision, int> keySelector,
        Func<TimelineDivision, string> labelSelector)
    {
        var result = new List<TimelineDivision>();
        TimelineDivision? current = null;
        var span = 0;

        foreach (var div in source)
        {
            var key = keySelector(div);

            if (current == null || keySelector(current) != key)
            {
                if (current != null)
                    result.Add(current with
                    {
                        Span = span,
                        Label = labelSelector(current),
                        Tooltip = labelSelector(current)
                    });

                current = div with { Span = 0 };
                span = 0;
            }
            else
            {
                // Extend the group end date
                current = current with { End = div.End };
            }

            span += div.Span;
        }

        if (current != null)
            result.Add(current with
            {
                Span = span,
                Label = labelSelector(current),
                Tooltip = labelSelector(current)
            });

        return result;
    }

    /// <summary>Группирует деления нижнего уровня в деления верхнего уровня по ключу.</summary>

    /// <summary>Подбирает масштаб верхнего ряда по правилам из ТЗ.</summary>
    private static TimelineScale ResolveUpperScale(TimelineScale lowerScale, DateOnly start, DateOnly end)
    {
        return lowerScale switch
        {
            TimelineScale.Days => TimelineScale.Weeks,
            TimelineScale.Weeks => TimelineScale.Months,
            TimelineScale.Months => DurationDays(start, end) <= 92 ? TimelineScale.Years : TimelineScale.Quarters,
            TimelineScale.Quarters => TimelineScale.Years,
            TimelineScale.Years => TimelineScale.Years,
            _ => TimelineScale.Years
        };
    }

    private static int DurationDays(DateOnly start, DateOnly end) => end.DayNumber - start.DayNumber + 1;

    // ── Lower-level computation ────────────────────────────────────────

    private static List<TimelineDivision> ComputeLower(TimelineInput input)
    {
        return input.Scale switch
        {
            TimelineScale.Days => ComputeLowerDays(input),
            TimelineScale.Weeks => ComputeLowerWeeks(input),
            TimelineScale.Months => ComputeLowerMonths(input),
            TimelineScale.Quarters => ComputeLowerQuarters(input),
            TimelineScale.Years => ComputeLowerYears(input),
            _ => []
        };
    }

    private static List<TimelineDivision> ComputeLowerDays(TimelineInput input)
    {
        var list = new List<TimelineDivision>();
        for (var d = input.StartDate; d <= input.EndDate; d = d.AddDays(1))
        {
            list.Add(new TimelineDivision
            {
                Start = d,
                End = d,
                Span = 1,
                Label = $"{d.Day} {MonthShort[d.Month - 1]}",
                Tooltip = $"{d.Day} {MonthFull[d.Month - 1]} {d.Year}",
                IsWeekend = d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
                IsHoliday = input.Holidays.Contains(d),
                IsToday = d == input.Today
            });
        }
        return list;
    }

    private static List<TimelineDivision> ComputeDaysFromCalendar(IReadOnlyList<CalendarDay> calendarDays, DateOnly today)
    {
        var list = new List<TimelineDivision>(calendarDays.Count);
        foreach (var cd in calendarDays)
        {
            list.Add(new TimelineDivision
            {
                Start = cd.Date,
                End = cd.Date,
                Span = 1,
                Label = $"{cd.Date.Day} {MonthShort[cd.MonthNumber - 1]}",
                Tooltip = $"{cd.Date.Day} {MonthFull[cd.MonthNumber - 1]} {cd.Year}",
                IsWeekend = cd.IsWeekend,
                IsHoliday = cd.IsHoliday,
                IsToday = cd.Date == today
            });
        }
        return list;
    }

    private static List<TimelineDivision> ComputeLowerWeeks(TimelineInput input)
    {
        var list = new List<TimelineDivision>();
        var mon = MondayOf(input.StartDate);

        while (mon <= input.EndDate)
        {
            var sun = mon.AddDays(6);
            if (sun < input.StartDate) { mon = mon.AddDays(7); continue; }

            var effStart = mon < input.StartDate ? input.StartDate : mon;
            var effEnd = sun > input.EndDate ? input.EndDate : sun;

            var isToday = input.Today >= mon && input.Today <= sun;
            var label = FormatWeekLabel(effStart, effEnd, mon, sun);
            var tooltip = $"{effStart.Day} {MonthFull[effStart.Month - 1]} – {effEnd.Day} {MonthFull[effEnd.Month - 1]} {effEnd.Year}";

            list.Add(new TimelineDivision
            {
                Start = mon,
                End = sun,
                Span = 1,
                Label = label,
                Tooltip = tooltip,
                IsWeekend = false,
                IsHoliday = false,
                IsToday = isToday
            });

            mon = mon.AddDays(7);
        }
        return list;
    }

    private static List<TimelineDivision> ComputeLowerMonths(TimelineInput input)
    {
        var list = new List<TimelineDivision>();
        var m = new DateOnly(input.StartDate.Year, input.StartDate.Month, 1);

        while (m <= input.EndDate)
        {
            var last = m.AddMonths(1).AddDays(-1);
            if (last < input.StartDate) { m = m.AddMonths(1); continue; }

            var isToday = input.Today >= m && input.Today <= last;
            list.Add(new TimelineDivision
            {
                Start = m,
                End = last,
                Span = 1,
                Label = MonthShort[m.Month - 1],
                Tooltip = $"{MonthFull[m.Month - 1]} {m.Year}",
                IsWeekend = false,
                IsHoliday = false,
                IsToday = isToday
            });

            m = m.AddMonths(1);
        }
        return list;
    }

    private static List<TimelineDivision> ComputeLowerQuarters(TimelineInput input)
    {
        var list = new List<TimelineDivision>();
        var qMonth = ((input.StartDate.Month - 1) / 3) * 3 + 1;
        var qStart = new DateOnly(input.StartDate.Year, qMonth, 1);

        while (qStart <= input.EndDate)
        {
            var qEnd = qStart.AddMonths(3).AddDays(-1);
            if (qEnd < input.StartDate) { qStart = qStart.AddMonths(3); continue; }

            var qNum = (qStart.Month - 1) / 3 + 1;
            var isToday = input.Today >= qStart && input.Today <= qEnd;
            list.Add(new TimelineDivision
            {
                Start = qStart,
                End = qEnd,
                Span = 1,
                Label = $"Q{qNum}",
                Tooltip = $"Q{qNum} {qStart.Year} ({MonthFull[qStart.Month - 1]} – {MonthFull[qEnd.Month - 1]})",
                IsWeekend = false,
                IsHoliday = false,
                IsToday = isToday
            });

            qStart = qStart.AddMonths(3);
        }
        return list;
    }

    private static List<TimelineDivision> ComputeLowerYears(TimelineInput input)
    {
        var list = new List<TimelineDivision>();
        for (var y = input.StartDate.Year; y <= input.EndDate.Year; y++)
        {
            var yStart = new DateOnly(y, 1, 1);
            var yEnd = new DateOnly(y, 12, 31);
            var isToday = input.Today >= yStart && input.Today <= yEnd;
            list.Add(new TimelineDivision
            {
                Start = yStart,
                End = yEnd,
                Span = 1,
                Label = y.ToString(),
                Tooltip = y.ToString(),
                IsWeekend = false,
                IsHoliday = false,
                IsToday = isToday
            });
        }
        return list;
    }

    // ── Upper-level computation (grouping) ─────────────────────────────

    private static List<TimelineDivision> ComputeUpper(List<TimelineDivision> lower, TimelineScale upperScale)
    {
        if (lower.Count == 0)
            return [];

        var result = new List<TimelineDivision>();
        TimelineDivision? current = null;
        var span = 0;

        foreach (var div in lower)
        {
            var key = UpperGroupKey(div.Start, upperScale);

            if (current == null || UpperGroupKey(current.Start, upperScale) != key)
            {
                if (current != null)
                    result.Add(current with { Span = span });

                var (gStart, gEnd) = UpperGroupRange(div.Start, upperScale);
                current = new TimelineDivision
                {
                    Start = gStart,
                    End = gEnd,
                    Span = 0,
                    Label = FormatUpperLabel(gStart, gEnd, upperScale),
                    Tooltip = FormatUpperTooltip(gStart, gEnd, upperScale)
                };
                span = 0;
            }

            span += div.Span;
        }

        if (current != null)
            result.Add(current with { Span = span });

        return result;
    }

    private static int UpperGroupKey(DateOnly d, TimelineScale upperScale)
    {
        return upperScale switch
        {
            TimelineScale.Weeks => d.Year * 100 + WeekNumberOf(d),
            TimelineScale.Months => d.Year * 100 + d.Month,
            TimelineScale.Quarters => d.Year * 10 + ((d.Month - 1) / 3 + 1),
            TimelineScale.Years => d.Year,
            _ => 0
        };
    }

    private static (DateOnly Start, DateOnly End) UpperGroupRange(DateOnly d, TimelineScale upperScale)
    {
        return upperScale switch
        {
            TimelineScale.Weeks => (MondayOf(d), MondayOf(d).AddDays(6)),
            TimelineScale.Months => (new DateOnly(d.Year, d.Month, 1), new DateOnly(d.Year, d.Month, 1).AddMonths(1).AddDays(-1)),
            TimelineScale.Quarters => QuarterRange(d),
            TimelineScale.Years => (new DateOnly(d.Year, 1, 1), new DateOnly(d.Year, 12, 31)),
            _ => (d, d)
        };
    }

    // ── Label formatting ───────────────────────────────────────────────

    private static string FormatWeekLabel(DateOnly effStart, DateOnly effEnd, DateOnly mon, DateOnly sun)
    {
        // "7–13 янв"
        if (effStart.Month == effEnd.Month)
            return $"{effStart.Day}–{effEnd.Day} {MonthShort[effStart.Month - 1]}";

        return $"{effStart.Day} {MonthShort[effStart.Month - 1]} – {effEnd.Day} {MonthShort[effEnd.Month - 1]}";
    }

    private static string FormatUpperLabel(DateOnly start, DateOnly end, TimelineScale upperScale)
    {
        return upperScale switch
        {
            TimelineScale.Weeks => FormatWeekLabel(start, end, start, end),
            TimelineScale.Months => MonthFull[start.Month - 1],
            TimelineScale.Quarters => $"Q{(start.Month - 1) / 3 + 1} {start.Year}",
            TimelineScale.Years => start.Year.ToString(),
            _ => ""
        };
    }

    private static string FormatUpperTooltip(DateOnly start, DateOnly end, TimelineScale upperScale)
    {
        return upperScale switch
        {
            TimelineScale.Weeks => $"{start.Day} {MonthFull[start.Month - 1]} – {end.Day} {MonthFull[end.Month - 1]} {end.Year}",
            TimelineScale.Months => $"{MonthFull[start.Month - 1]} {start.Year}",
            TimelineScale.Quarters => $"Q{(start.Month - 1) / 3 + 1} {start.Year} ({MonthFull[start.Month - 1]} – {MonthFull[end.Month - 1]})",
            TimelineScale.Years => start.Year.ToString(),
            _ => ""
        };
    }

    // ── Helpers ────────────────────────────────────────────────────────

    private static DateOnly MondayOf(DateOnly d)
    {
        var offset = ((int)d.DayOfWeek - 1 + 7) % 7;
        return d.AddDays(-offset);
    }

    private static int WeekNumberOf(DateOnly d)
    {
        var firstDayOfYear = new DateOnly(d.Year, 1, 1);
        var firstMonday = MondayOf(firstDayOfYear);
        if (firstMonday > firstDayOfYear)
            firstMonday = firstMonday.AddDays(-7);
        return (d.DayNumber - firstMonday.DayNumber) / 7 + 1;
    }

    private static (DateOnly Start, DateOnly End) QuarterRange(DateOnly d)
    {
        var qMonth = ((d.Month - 1) / 3) * 3 + 1;
        var start = new DateOnly(d.Year, qMonth, 1);
        var end = start.AddMonths(3).AddDays(-1);
        return (start, end);
    }

    // ── Today position ─────────────────────────────────────────────────

    private static double ComputeTodayPosition(List<TimelineDivision> lower, DateOnly today)
    {
        if (lower.Count == 0)
            return -1;

        for (var i = 0; i < lower.Count; i++)
        {
            var div = lower[i];
            if (today >= div.Start && today <= div.End)
            {
                var daysInCell = div.End.DayNumber - div.Start.DayNumber + 1;
                var offsetInCell = today.DayNumber - div.Start.DayNumber;
                return i + (double)offsetInCell / daysInCell;
            }
        }

        return -1;
    }

    // ── Multi-level helpers ────────────────────────────────────────────

    private static double ComputeTodayPixel(List<TimelineDivision> days, DateOnly today, int dayCellWidth)
    {
        for (var i = 0; i < days.Count; i++)
        {
            if (days[i].Start <= today && days[i].End >= today)
                return i * dayCellWidth + dayCellWidth / 2.0;
        }
        return -1;
    }
}