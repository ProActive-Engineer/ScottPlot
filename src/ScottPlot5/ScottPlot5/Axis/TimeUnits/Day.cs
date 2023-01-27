﻿using System.Globalization;

namespace ScottPlot.Axis.TimeUnits;

public class Day : ITimeUnit
{
    public IReadOnlyList<int> NiceIncrements => NiceNumbers.Days;

    public TimeSpan MinSize => TimeSpan.FromDays(1);

    public string GetFormat()
    {
        return $"d";
    }

    public DateTime Next(DateTime dateTime, int increment = 1)
    {
        return dateTime.AddDays(increment);
    }
}
