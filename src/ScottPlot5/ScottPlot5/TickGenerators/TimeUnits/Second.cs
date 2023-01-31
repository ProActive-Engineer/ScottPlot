﻿using System.Globalization;
using ScottPlot.TickGenerators.TimeUnits;

namespace ScottPlot.Axis.TimeUnits;

public class Second : ITimeUnit
{
    public IReadOnlyList<int> Divisors => StandardDivisors.Sexagesimal;

    public TimeSpan MinSize => TimeSpan.FromSeconds(1);

    public string GetDateTimeFormatString()
    {
        return $"{CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern}\n{CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern}";
    }

    public DateTime Next(DateTime dateTime, int increment = 1)
    {
        return dateTime.AddSeconds(increment);
    }
}
