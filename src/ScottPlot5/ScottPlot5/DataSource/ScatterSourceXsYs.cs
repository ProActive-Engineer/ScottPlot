﻿namespace ScottPlot.DataSource;

/// <summary>
/// This data source manages X/Y points as separate X and Y collections
/// </summary>
public class ScatterSourceXsYs : IScatterSource
{
    private readonly IReadOnlyList<double> Xs;
    private readonly IReadOnlyList<double> Ys;

    public ScatterSourceXsYs(IReadOnlyList<double> xs, IReadOnlyList<double> ys)
    {
        Xs = xs;
        Ys = ys;
    }

    private Coordinates GetCoordinatesAt(int index)
    {
        return new Coordinates(Xs[index], Ys[index]);
    }

    public IReadOnlyList<Coordinates> GetScatterPoints()
    {
        return Enumerable.Range(0, Xs.Count).Select(i => GetCoordinatesAt(i)).ToArray();
    }

    public AxisLimits GetLimits()
    {
        return new AxisLimits(GetLimitsX(), GetLimitsY());
    }

    public CoordinateRange GetLimitsX()
    {
        return new CoordinateRange(Xs.Min(), Xs.Max());
    }

    public CoordinateRange GetLimitsY()
    {
        return new CoordinateRange(Ys.Min(), Ys.Max());
    }
}
