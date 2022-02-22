﻿using Microsoft.Maui.Graphics;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics.Skia;
using System.IO;

namespace ScottPlot;

public class Plot
{
    /// <summary>
    /// List of objects drawn on the plot every time a render is requested.
    /// </summary>
    public readonly List<IPlottable> Plottables = new();

    /// <summary>
    /// This object holds all the information needed to render a plot at an arbitrary size:
    /// Figure size, data area size, axis limits, tick generators, etc.
    /// It may be manipulated or replaced by the user.
    /// </summary>
    public PlotConfig Config { get; set; } = PlotConfig.Default;

    /// <summary>
    /// This object stores performance information for previous renders.
    /// </summary>
    public readonly RenderStats Stats = new();

    public Plot()
    {
    }

    #region add/remove plottables

    public void Clear() => Plottables.Clear();

    public Plottables.ScatterArray<double> AddScatter(double[] xs, double[] ys, Color? color = null)
    {
        color ??= Config.Style.Palette.GetColor(Plottables.Count);

        Plottables.ScatterArray<double> sp = new(xs, ys)
        {
            LineColor = color,
            MarkerColor = color,
        };

        Plottables.Add(sp);
        return sp;
    }

    #endregion

    #region Axis Manipulation

    public void Autoscale()
    {
        CoordinateRect totalLimits = CoordinateRect.AllNan();
        foreach (IPlottable plottable in Plottables)
        {
            CoordinateRect limits = plottable.GetDataLimits();
            totalLimits = totalLimits.Expand(limits);
        }

        if (!totalLimits.HasFiniteWidth)
            totalLimits = totalLimits.WithX(-10, 10);

        if (!totalLimits.HasFiniteHeight)
            totalLimits = totalLimits.WithY(-10, 10);

        Console.WriteLine($"LIMITS: {totalLimits}");
        Config = Config.WithAxisLimits(totalLimits);
    }

    #endregion

    #region rendering

    public void Draw(ICanvas canvas) =>
        Config = Draw(canvas, Config, true, Plottables.ToArray(), Stats);

    public void Draw(ICanvas canvas, PlotConfig config) =>
        Config = Draw(canvas, config, false, Plottables.ToArray(), Stats);

    private static PlotConfig Draw(ICanvas canvas, PlotConfig config, bool tightenLayout, IPlottable[] plottables, RenderStats stats)
    {
        if (!config.FigureRect.HasPositiveArea)
            return config;

        Stopwatch sw = Stopwatch.StartNew();
        DrawFigureBackground(canvas, config);
        if (tightenLayout)
            config = config.WithTightLayout(canvas);
        if (!config.DataRect.HasPositiveArea)
            return config;
        Tick[] allTicks = config.GenerateAllTicks();
        DrawDataBackground(canvas, config);
        DrawGridLines(canvas, config, allTicks);
        DrawPlottables(canvas, config, plottables);
        DrawAxisLabelsAndTicks(canvas, config, allTicks);
        DrawSpines(canvas, config);
        sw.Stop();

        stats.AddRenderTime(sw.Elapsed);
        stats.Draw(canvas, config);

        return config;
    }

    private static void DrawFigureBackground(ICanvas canvas, PlotConfig config)
    {
        canvas.FillColor = config.Style.FigureBackgroundColor;
        canvas.FillRectangle(config.FigureRect.RectangleF);
    }

    private static void DrawDataBackground(ICanvas canvas, PlotConfig config)
    {
        canvas.FillColor = config.Style.DataBackgroundColor;
        canvas.FillRectangle(config.DataRect.RectangleF);
    }

    private static void DrawGridLines(ICanvas canvas, PlotConfig config, Tick[] allTicks)
    {
        foreach (Axes.IAxis axis in config.Axes)
        {
            Tick[] axisTicks = allTicks.Where(tick => tick.Edge == axis.Edge).ToArray();
            axis.DrawGridLines(canvas, config, axisTicks);
        }
    }

    private static void DrawPlottables(ICanvas canvas, PlotConfig config, IPlottable[] plottables)
    {
        foreach (IPlottable plottable in plottables)
        {
            canvas.SaveState(); // because we can't trust each plottable to do this
            plottable.Draw(canvas, config);
            canvas.RestoreState();
        }
    }

    private static void DrawAxisLabelsAndTicks(ICanvas canvas, PlotConfig config, Tick[] allTicks)
    {
        foreach (Axes.IAxis axis in config.Axes)
        {
            Tick[] axisTicks = allTicks.Where(tick => tick.Edge == axis.Edge).ToArray();
            axis.DrawTicks(canvas, config, axisTicks);
            axis.DrawAxisLabel(canvas, config);
        }
    }

    private static void DrawSpines(ICanvas canvas, PlotConfig config)
    {
        foreach (Axes.IAxis axis in config.Axes)
        {
            axis.DrawSpine(canvas, config);
        }
    }

    #endregion

    #region IO

    public string SaveFig(string path)
    {
        return SaveFig(path, Config);
    }

    public string SaveFig(string path, PlotConfig layout)
    {
        using SkiaBitmapExportContext context = new((int)layout.Width, (int)layout.Height, layout.DisplayScale);

        Draw(context.Canvas, layout);

        string fullPath = Path.GetFullPath(path);
        using FileStream fs = new(fullPath, FileMode.Create);
        context.WriteToStream(fs);
        return fullPath;
    }

    #endregion

    #region Testing and Development

    public bool Benchmark(bool enable = true)
    {
        Stats.IsVisible = enable;
        return Stats.IsVisible;
    }

    public bool BenchmarkToggle() => Stats.IsVisible = !Stats.IsVisible;

    #endregion
}