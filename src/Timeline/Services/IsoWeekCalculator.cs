namespace SamorodinkaTech.Fiducia.Timeline.Services;

/// <summary>Вычисление номеров недель по стандарту ISO 8601.</summary>
/// <remarks>
/// ISO 8601: неделя принадлежит тому году, которому принадлежит её четверг.
/// Первая неделя года — та, которая содержит первый четверг года (4 января).
/// 29–31 декабря могут относиться к неделе 1 следующего года.
/// 1–3 января могут относиться к последней неделе предыдущего года.
/// </remarks>
public static class IsoWeekCalculator
{
    /// <summary>Возвращает ISO-год и номер недели для даты.</summary>
    public static (int IsoYear, int IsoWeek) IsoWeekOf(DateOnly d)
    {
        // ISO-день недели: 1=пн, 2=вт, ..., 7=вс
        var dotNetDow = (int)d.DayOfWeek;
        var isoDow = dotNetDow == 0 ? 7 : dotNetDow;

        // Четверг недели — определяет ISO-год
        var thursday = d.AddDays(4 - isoDow);
        var isoYear = thursday.Year;

        // 4 января — всегда в первой ISO-неделе года
        var jan4 = new DateOnly(isoYear, 1, 4);
        var jan4NetDow = (int)jan4.DayOfWeek;
        var jan4IsoDow = jan4NetDow == 0 ? 7 : jan4NetDow;

        // Первый четверг года
        var firstThursday = jan4.AddDays(4 - jan4IsoDow);

        // Первый понедельник ISO-года (четверг − 3 дня)
        var firstMonday = firstThursday.AddDays(-3);

        var isoWeek = (d.DayNumber - firstMonday.DayNumber) / 7 + 1;

        return (isoYear, isoWeek);
    }

    /// <summary>Составной ключ для группировки: ISO-год * 100 + ISO-неделя.</summary>
    public static int IsoWeekKey(DateOnly d)
    {
        var (isoYear, isoWeek) = IsoWeekOf(d);
        return isoYear * 100 + isoWeek;
    }
}
