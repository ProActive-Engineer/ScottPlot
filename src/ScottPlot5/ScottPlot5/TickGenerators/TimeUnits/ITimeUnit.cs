﻿namespace ScottPlot.Axis.TimeUnits;

public interface ITimeUnit
{
    /// <summary>
    /// An array of integers that serve as good divisors to subdivide this time unit
    /// </summary>
    public IReadOnlyList<int> NiceIncrements { get; }

    /// <summary>
    /// Returns the format string used to display tick labels of this time unit.
    /// https://learn.microsoft.com/en-us/dotnet/api/system.datetime.tostring
    /// </summary>
    public string GetDateTimeFormatString();

    /// <summary>
    /// Return the DateTime N units relative to this one
    /// </summary>
    public DateTime Next(DateTime dateTime, int increment = 1);

    /// <summary>
    /// ??????????
    /// </summary>
    public TimeSpan MinSize { get; }
}
