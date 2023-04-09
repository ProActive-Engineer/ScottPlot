﻿using ScottPlot.Drawing;
using System;
using System.Drawing;

namespace ScottPlot.Plottable
{
    /// <summary>
    /// Text placed at a location relative to the data area that does not move when the axis limits change
    /// </summary>
    public class Annotation : IPlottable
    {
        /// <summary>
        /// Horizontal location (in pixel units) relative to the data area
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Vertical position (in pixel units) relative to the data area
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Text displayed in the annotation
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Distance (pixels) the shadow will be to the right of the box
        /// </summary>
        public float ShadowOffsetX { get; set; } = 5;

        /// <summary>
        /// Distance (pixels) the shadow will be below the box
        /// </summary>
        public float ShadowOffsetY { get; set; } = 5;

        /// <summary>
        /// Font for the annotation text
        /// </summary>
        public readonly Drawing.Font Font = new();

        /// <summary>
        /// If true, the rectangle behind the text will be filled with <see cref="BackgroundColor"/>
        /// </summary>
        public bool Background { get; set; } = true;

        /// <summary>
        /// Color of the rectangle drawn beneath the annotation if <see cref="Background"/> is true
        /// </summary>
        public Color BackgroundColor { get; set; } = Color.Yellow;

        /// <summary>
        /// If true, a rectangular shadow will be drawn behind the background rectangle filled with <see cref="ShadowColor"/>
        /// </summary>
        public bool Shadow { get; set; } = true;

        /// <summary>
        /// Color of the rectangle drawn beneath the annotation if <see cref="Shadow"/> is true.
        /// Semitransparent colors are recommended for shadows.
        /// </summary>
        public Color ShadowColor { get; set; } = Color.FromArgb(25, Color.Black);

        /// <summary>
        /// If true, the rectangle around the text will be drawn according to <see cref="BorderWidth"/> and <see cref="BorderColor"/>
        /// </summary>
        public bool Border { get; set; } = true;

        /// <summary>
        /// Thickness (in pixels) of the rectangular outline to draw around the text using <see cref="BorderColor"/>
        /// </summary>
        public float BorderWidth { get; set; } = 1;

        /// <summary>
        /// Color of the rectangular outline drawn around the text
        /// </summary>
        public Color BorderColor { get; set; } = Color.Black;

        public bool IsVisible { get; set; } = true;
        public int XAxisIndex { get; set; } = 0;
        public int YAxisIndex { get; set; } = 0;
        public AxisLimits GetAxisLimits() => AxisLimits.NoLimits;
        public LegendItem[] GetLegendItems() => LegendItem.None;
        public void ValidateData(bool deep = false) { }

        public void Render(PlotDimensions dims, Bitmap bmp, bool lowQuality = false)
        {
            if (!IsVisible)
                return;

            using var gfx = GDI.Graphics(bmp, dims, lowQuality, false);
            using var font = GDI.Font(Font);
            using var fontBrush = new SolidBrush(Font.Color);
            using var shadowBrush = new SolidBrush(ShadowColor);
            using var backgroundBrush = new SolidBrush(BackgroundColor);
            using var borderPen = new Pen(BorderColor, BorderWidth);

            SizeF size = GDI.MeasureString(gfx, Label, font);
            double x = (X >= 0) ? X : dims.DataWidth + X - size.Width;
            double y = (Y >= 0) ? Y : dims.DataHeight + Y - size.Height;
            PointF location = new((float)x + dims.DataOffsetX, (float)y + dims.DataOffsetY);

            RectangleF backgroundRect = new(location.X, location.Y, size.Width, size.Height);
            RectangleF shadowRect = new(location.X + ShadowOffsetX, location.Y + ShadowOffsetY, size.Width, size.Height);

            if (Background && Shadow)
                gfx.FillRectangle(shadowBrush, shadowRect);

            if (Background)
                gfx.FillRectangle(backgroundBrush, backgroundRect);

            if (Border)
                gfx.DrawRectangle(borderPen, backgroundRect);

            gfx.DrawString(Label, font, fontBrush, location);
        }
    }
}
