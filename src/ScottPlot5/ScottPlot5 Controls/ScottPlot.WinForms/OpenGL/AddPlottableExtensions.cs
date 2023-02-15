﻿namespace ScottPlot;

/// <summary>
/// This class extends Plot.Add.* to add additional plottables provided by this NuGet package
/// </summary>
public static class AddPlottableExtensions
{
    public static void ScatterGL(this AddPlottable add, double[] xs, double[] ys, SkiaSharp.GRContext context)
    {
        DataSources.ScatterSourceXsYs data = new(xs, ys);
        Plottables.ScatterGL sp = new(data, context);
        add.Plottable(sp);
    }
}
