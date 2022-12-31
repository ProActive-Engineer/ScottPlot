﻿using ScottPlot.Plottables;
using ScottPlot.Style;
using SkiaSharp;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ScottPlot;

/// <summary>
/// Common operations using the default rendering system.
/// </summary>
public static class Drawing
{
    public static PixelSize MeasureString(string text, SKPaint paint)
    {
        SKRect bounds = new();
        paint.MeasureText(text, ref bounds);

        float width = bounds.Width;
        float height = bounds.Height;
        return new PixelSize(width, height);
    }

    public static PixelSize MeasureLargestString(string[] strings, SKPaint paint)
    {
        float maxWidth = 0;
        float maxHeight = 0;

        for (int i = 0; i < strings.Length; i++)
        {
            PixelSize tickSize = MeasureString(strings[i], paint);
            maxWidth = Math.Max(maxWidth, tickSize.Width);
            maxHeight = Math.Max(maxHeight, tickSize.Height);
        }

        return new PixelSize(maxWidth, maxHeight);
    }

    public static void DrawLines(SKSurface surface, Pixel[] starts, Pixel[] ends, Color color, float width = 1, bool antiAlias = true)
    {
        if (starts.Length != ends.Length)
            throw new ArgumentException($"{nameof(starts)} and {nameof(ends)} must have same length");

        using SKPaint paint = new()
        {
            Color = color.ToSKColor(),
            IsStroke = true,
            IsAntialias = antiAlias,
            StrokeWidth = width,
        };

        using SKPath path = new();

        for (int i = 0; i < starts.Length; i++)
        {
            path.MoveTo(starts[i].X, starts[i].Y);
            path.LineTo(ends[i].X, ends[i].Y);
        }

        surface.Canvas.DrawPath(path, paint);
    }

    public static void Fillectangle(SKCanvas canvas, PixelRect rect, Color color)
    {
        using SKPaint paint = new()
        {
            Color = color.ToSKColor(),
            IsStroke = false,
            IsAntialias = true,
        };

        canvas.DrawRect(rect.ToSKRect(), paint);
    }

    public static void DrawRectangle(SKCanvas canvas, PixelRect rect, Color color, float thickness = 1)
    {
        using SKPaint paint = new()
        {
            Color = color.ToSKColor(),
            IsStroke = true,
            StrokeWidth = thickness,
            IsAntialias = true,
        };

        canvas.DrawRect(rect.ToSKRect(), paint);
    }

    public static void DrawDebugRectangle(SKCanvas canvas, PixelRect rect, Pixel point, Color color, float thickness = 1)
    {
        using SKPaint paint = new()
        {
            Color = color.ToSKColor(),
            IsStroke = true,
            StrokeWidth = thickness,
            IsAntialias = true,
        };

        canvas.DrawRect(rect.ToSKRect(), paint);
        canvas.DrawLine(rect.BottomLeft.ToSKPoint(), rect.TopRight.ToSKPoint(), paint);
        canvas.DrawLine(rect.TopLeft.ToSKPoint(), rect.BottomRight.ToSKPoint(), paint);

        canvas.DrawCircle(point.ToSKPoint(), 5, paint);

        paint.IsStroke = false;
        paint.Color = paint.Color.WithAlpha(20);
        canvas.DrawRect(rect.ToSKRect(), paint);
    }

    public static void DrawCircle(SKCanvas canvas, Pixel center, Color color, float radius = 5, bool fill = true)
    {
        using SKPaint paint = new()
        {
            Color = color.ToSKColor(),
            IsStroke = !fill,
            IsAntialias = true,
        };

        canvas.DrawCircle(center.ToSKPoint(), radius, paint);
    }

    public static SKBitmap BitmapFromArgbs(uint[] argbs, int width, int height)
    {
        GCHandle handle = GCHandle.Alloc(argbs, GCHandleType.Pinned);

        var imageInfo = new SKImageInfo(width, height);
        var bmp = new SKBitmap(imageInfo);
        bmp.InstallPixels(
            info: imageInfo,
            pixels: handle.AddrOfPinnedObject(),
            rowBytes: imageInfo.RowBytes,
            releaseProc: (IntPtr _, object _) => handle.Free());

        return bmp;
    }
}
