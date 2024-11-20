namespace DotNetHelpers.Extensions;

public static class DateTimeExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="day"></param>
    /// <param name="weekDay">1(dom) ... 7(sáb)</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static DateTime? Find(this DateTime dateTime, int? day = null, int? weekDay = null)
    {
        DateTime refTime = dateTime;
        int incDays = 0;

        if (weekDay != null)
        {
            incDays = (weekDay < 0) ? -1 : 1;
            weekDay = Math.Abs(weekDay.Value);

            if (weekDay is < 1 or > 7)
                throw new ArgumentOutOfRangeException(nameof(day), $"Dia da semana '{weekDay}' inválido!");

            while ((int)refTime.DayOfWeek != weekDay - 1)
                refTime = refTime.AddDays(incDays);

            incDays = incDays * 7;
        }

        if (day != null)
        {
            if (incDays == 0)
                incDays = (day < 0) ? -1 : 1;

            day = Math.Abs(day.Value);

            if (day is < 1 or > 31)
                throw new ArgumentOutOfRangeException(nameof(day), $"Dia '{day}' inválido!");

            while (refTime.Day != day)
                refTime = refTime.AddDays(incDays);
        }



        return refTime;
    }

}
