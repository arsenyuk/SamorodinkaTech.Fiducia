using SamorodinkaTech.Fiducia.Timeline.Services;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Хелпер для работы с производственным календарём РФ.
/// Делегирует всю логику в <see cref="WorkCalendar"/> — единый источник истины.
/// </summary>
public static class WorkingDayHelper
{
    /// <summary>Проверяет, является ли дата нерабочим днём (выходной или праздник РФ).</summary>
    public static bool IsNonWorking(DateOnly date, IReadOnlySet<DateOnly>? holidays = null)
        => WorkCalendar.IsNonWorking(date, holidays);

    /// <summary>Возвращает ближайший рабочий день, начиная с указанной даты (включительно).</summary>
    public static DateOnly GetNextWorkingDay(DateOnly date, IReadOnlySet<DateOnly>? holidays = null)
        => WorkCalendar.GetNextWorkingDay(date, holidays);

    /// <summary>Возвращает множество официальных нерабочих праздничных дней РФ за указанный год.</summary>
    public static HashSet<DateOnly> GetHolidays(int year)
        => WorkCalendar.GetHolidays(year);
}
