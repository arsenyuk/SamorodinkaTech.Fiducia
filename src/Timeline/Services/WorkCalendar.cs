namespace SamorodinkaTech.Fiducia.Timeline.Services;

using SamorodinkaTech.Fiducia.Timeline.Models;

/// <summary>Утилита для формирования множества нерабочих дней (производственный календарь РФ).</summary>
public static class WorkCalendar
{
    /// <summary>Последний день первой декады месяца (1–10).</summary>
    private const int FirstDecadeEndDay = 10;

    /// <summary>Последний день второй декады месяца (11–20).</summary>
    private const int SecondDecadeEndDay = 20;

    /// <summary>Генерирует производственный календарь на диапазон лет с полными атрибутами каждого дня.</summary>
    /// <param name="startYear">Первый год (включительно).</param>
    /// <param name="endYear">Последний год (включительно).</param>
    /// <returns>Список дней с атрибутами: рабочий/выходной/праздничный, номер недели, месяц, квартал, год.</returns>
    public static List<CalendarDay> GenerateCalendar(int startYear, int endYear)
    {
        var days = new List<CalendarDay>();
        var start = new DateOnly(startYear, 1, 1);
        var end = new DateOnly(endYear, 12, 31);

        // Собираем праздники за все годы
        var allHolidays = new HashSet<DateOnly>();
        for (var y = startYear; y <= endYear; y++)
            allHolidays.UnionWith(GetHolidays(y));

        for (var d = start; d <= end; d = d.AddDays(1))
        {
            var isWeekend = d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
            var isHoliday = allHolidays.Contains(d);

            days.Add(new CalendarDay
            {
                Date = d,
                IsWorkingDay = !isWeekend && !isHoliday,
                IsWeekend = isWeekend,
                IsHoliday = isHoliday,
                WeekNumber = IsoWeekCalculator.IsoWeekOf(d).IsoWeek,
                MonthNumber = d.Month,
                Quarter = (d.Month - 1) / 3 + 1,
                Decade = d.Day <= FirstDecadeEndDay ? 1 : d.Day <= SecondDecadeEndDay ? 2 : 3,
                Year = d.Year
            });
        }

        return days;
    }

    /// <summary>Возвращает множество официальных нерабочих праздничных дней РФ за указанный год.</summary>
    /// <param name="year">Год.</param>
    /// <returns>Множество дат праздничных дней. Выходные (сб/вс) не включены — они вычисляются динамически при рендеринге шкалы.</returns>
    /// <remarks>
    /// Включает официальные праздники по ст. 112 ТК РФ.
    /// Переносы выходных (постановления Правительства РФ на конкретный год) — статические справочники,
    /// обновляемые раз в год. Ниже приведены переносы на 2025–2026 гг.
    /// </remarks>
    public static HashSet<DateOnly> GetHolidays(int year)
    {
        var holidays = new HashSet<DateOnly>
        {
            // Новогодние каникулы (ст. 112 ТК РФ)
            new(year, 1, 1), new(year, 1, 2), new(year, 1, 3),
            new(year, 1, 4), new(year, 1, 5), new(year, 1, 6),
            new(year, 1, 7), new(year, 1, 8),

            // День защитника Отечества
            new(year, 2, 23),

            // Международный женский день
            new(year, 3, 8),

            // Праздник Весны и Труда
            new(year, 5, 1),

            // День Победы
            new(year, 5, 9),

            // День России
            new(year, 6, 12),

            // День народного единства
            new(year, 11, 4)
        };

        AddTransfers(year, holidays);
        return holidays;
    }

    /// <summary>Добавляет рабочие субботы (перенесённые выходные) за конкретный год.</summary>
    private static void AddTransfers(int year, HashSet<DateOnly> holidays)
    {
        switch (year)
        {
            case 2025:
                // Постановление Правительства РФ от 04.10.2024
                // 4 января (сб) → 2 мая (пт)
                holidays.Remove(new DateOnly(2025, 1, 4)); // была суббота, стала рабочей
                holidays.Add(new DateOnly(2025, 5, 2));
                // 5 января (вс) → не переносится (и так выходной)
                // 23 февраля (вс) → 8 мая (чт)
                holidays.Add(new DateOnly(2025, 5, 8));
                // 8 марта (сб) → 13 июня (пт)
                holidays.Remove(new DateOnly(2025, 3, 8)); // была суббота, стала рабочей
                holidays.Add(new DateOnly(2025, 6, 13));
                break;

            case 2026:
                // Переносы на 2026 г. (будут утверждены Правительством РФ осенью 2025)
                // Пока — базовый набор без переносов за пределы праздника
                break;

            default:
                // Для годов без известных переносов — оставляем только базовые праздники
                break;
        }
    }

    /// <summary>Проверяет, является ли дата нерабочим днём (праздник или выходной).</summary>
    public static bool IsNonWorking(DateOnly date, IReadOnlySet<DateOnly>? holidays = null)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            return true;

        return holidays?.Contains(date) ?? false;
    }

    /// <summary>Возвращает ближайший рабочий день, начиная с указанной даты (включительно).
    /// Если дата уже рабочая — возвращает её без изменений.</summary>
    public static DateOnly GetNextWorkingDay(DateOnly date, IReadOnlySet<DateOnly>? holidays = null)
    {
        while (IsNonWorking(date, holidays))
            date = date.AddDays(1);
        return date;
    }

}