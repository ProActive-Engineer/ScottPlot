﻿using ScottPlot.Style;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScottPlot
{
    internal static class PaintExtensions
    {
        public static void SetStroke(this SKPaint paint, Stroke stroke)
        {
            paint.StrokeWidth = (float)stroke.Width;
            paint.Color = stroke.Color.ToSKColor();
            paint.Style = SKPaintStyle.Stroke;
        }

        public static void SetFill(this SKPaint paint, Fill fill, byte alpha = 255)
        {
            paint.Color = fill.Color.WithAlpha(alpha).ToSKColor();
            paint.PathEffect = null; // TODO: Make this respect HatchStyle
            paint.Style = SKPaintStyle.Fill;
        }
    }
}
