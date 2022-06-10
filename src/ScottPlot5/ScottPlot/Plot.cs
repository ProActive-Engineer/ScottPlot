﻿using System.Diagnostics;
using SkiaSharp;

namespace ScottPlot;

public class Plot
{
    // TOOD: don't store min/max state in these
    readonly HorizontalAxis XAxis = new();
    readonly VerticalAxis YAxis = new();
    readonly List<IPlottable> Plottables = new();

    /// <summary>
    /// Any state stored across renders can be stored here.
    /// </summary>
    private RenderInformation LastRenderInfo;

    public Plot()
    {
        CoordinateRect bounds = new(-10, 10, -10, 10);
        SetAxisLimits(bounds);
    }

    public void Add(IPlottable plottable)
    {
        Plottables.Add(plottable);
    }

    public void Clear()
    {
        Plottables.Clear();
    }

    public IPlottable[] GetPlottables()
    {
        return Plottables.ToArray();
    }

    public void SetAxisLimits(CoordinateRect rect)
    {
        XAxis.Min = rect.XMin;
        XAxis.Max = rect.XMax;
        YAxis.Min = rect.YMin;
        YAxis.Max = rect.YMax;
    }

    public CoordinateRect GetAxisLimits()
    {
        return new CoordinateRect(XAxis.Min, XAxis.Max, YAxis.Min, YAxis.Max);
    }

    public Pixel GetPixel(Coordinate coord, PixelRect rect)
    {
        float x = XAxis.GetPixel(coord.X, rect.Left, rect.Right);
        float y = YAxis.GetPixel(coord.Y, rect.Bottom, rect.Top);
        return new Pixel(x, y);
    }

    /// <summary>
    /// Return the coordinate for a specific pixel on the most recent render.
    /// </summary>
    public Coordinate GetCoordinate(Pixel pixel)
    {
        PixelRect dataRect = LastRenderInfo.DataRect;
        double x = XAxis.GetCoordinate(pixel.X, dataRect.Left, dataRect.Right);
        double y = YAxis.GetCoordinate(pixel.Y, dataRect.Bottom, dataRect.Top);
        return new Coordinate(x, y);
    }

    private PixelRect GetDataAreaRect(PixelRect figureRect)
    {
        // NOTE: eventually this will measure strings to calculate the ideal layout
        PixelPadding DataAreaPadding = new(40, 10, 30, 20);
        return figureRect.Contract(DataAreaPadding);
    }

    public RenderInformation Render(SKSurface surface)
    {
        Stopwatch SW = Stopwatch.StartNew();
        RenderInformation renderInfo = new();

        // analyze axes, determine ticks, measure strings, etc. to calculate layout
        renderInfo.FigureRect = PixelRect.FromSKRect(surface.Canvas.LocalClipBounds);
        renderInfo.DataRect = GetDataAreaRect(renderInfo.FigureRect);
        renderInfo.ElapsedLayout = SW.Elapsed;
        SW.Restart();

        // perform all renders
        RenderBackground(surface);
        RenderPlottables(surface, renderInfo.DataRect);
        RenderAxes(surface, renderInfo.DataRect);
        renderInfo.ElapsedRender = SW.Elapsed;

        LastRenderInfo = renderInfo;
        return renderInfo;
    }

    private void RenderBackground(SKSurface surface)
    {
        surface.Canvas.Clear(SKColors.Navy);
    }

    private void RenderPlottables(SKSurface surface, PixelRect dataRect)
    {
        foreach (var plottable in Plottables)
        {
            // TODO: dont store min/max state inside the axes themselves
            plottable.Render(surface, dataRect, XAxis, YAxis);
        }
    }

    private void RenderAxes(SKSurface surface, PixelRect dataRect)
    {
        using SKPaint paint = new()
        {
            IsAntialias = true,
            StrokeWidth = 1,
            Style = SKPaintStyle.Stroke,
        };

        paint.Color = SKColors.Yellow;
        surface.Canvas.DrawRect(dataRect.ToSKRect(), paint);
    }

    public byte[] GetImageBytes(int width, int height, SKEncodedImageFormat format = SKEncodedImageFormat.Png, int quality = 100)
    {
        SKImageInfo info = new(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        SKSurface surface = SKSurface.Create(info);
        Render(surface);
        SKImage snap = surface.Snapshot();
        SKData data = snap.Encode(format, quality);
        byte[] bytes = data.ToArray();
        return bytes;
    }

    public void SaveImage(int width, int height, string path, int quality = 100)
    {
        SKEncodedImageFormat format;

        if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            format = SKEncodedImageFormat.Png;
        else if (path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            format = SKEncodedImageFormat.Jpeg;
        else if (path.EndsWith(".jepg", StringComparison.OrdinalIgnoreCase))
            format = SKEncodedImageFormat.Jpeg;
        else if (path.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
            format = SKEncodedImageFormat.Bmp;
        else
            throw new ArgumentException("unsupported image format");

        byte[] bytes = GetImageBytes(width, height, format, quality);
        File.WriteAllBytes(path, bytes);
    }
}
