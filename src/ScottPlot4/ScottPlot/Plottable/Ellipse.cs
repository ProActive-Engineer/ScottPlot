﻿using ScottPlot.Drawing;
using System;
using System.Drawing;

namespace ScottPlot.Plottable
{
    public class Ellipse : IPlottable, IHasColor, IHasArea
    {
        /// <summary>
        /// Horizontal center of the circle (axis units)
        /// </summary>
        double X { get; }

        /// <summary>
        /// Vertical center of the circle (axis units)
        /// </summary>
        double Y { get; }

        /// <summary>
        /// Horizontal radius (axis units)
        /// </summary>
        public double RadiusX { get; set; }

        /// <summary>
        /// Vertical radius (axis units)
        /// </summary>
        public double RadiusY { get; set; }

        /// <summary>
        /// Rotation of the ellipse (degrees)
        /// </summary>
        public float Rotation { get; set; } = 0;

        /// <summary>
        /// Outline color
        /// </summary>
        public Color BorderColor { get; set; } = Color.Black;

        /// <summary>
        /// Outline thickness (pixel units)
        /// </summary>
        public float BorderLineWidth { get; set; } = 2;

        private bool HasBorder => (BorderLineStyle != LineStyle.None) && (BorderColor != Color.Transparent);

        /// <summary>
        /// Outline line style
        /// </summary>
        public LineStyle BorderLineStyle { get; set; } = LineStyle.Solid;

        /// <summary>
        /// Fill color
        /// </summary>
        public Color Color { get; set; } = Color.Transparent;

        /// <summary>
        /// Fill pattern
        /// </summary>
        public HatchStyle HatchStyle { get; set; } = HatchStyle.None;

        /// <summary>
        /// Alternate color for fill pattern
        /// </summary>
        public Color HatchColor { get; set; } = Color.Black;

        /// <summary>
        /// Text to appear in the legend
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Create an ellipse centered at (x, y) with the given horizontal and vertical radius
        /// </summary>
        public Ellipse(double x, double y, double xRadius, double yRadius)
        {
            X = x;
            Y = y;
            RadiusX = xRadius;
            RadiusY = yRadius;
        }

        // These default values are fine for most cases
        public bool IsVisible { get; set; } = true;
        public int XAxisIndex { get; set; } = 0;
        public int YAxisIndex { get; set; } = 0;

        public void ValidateData(bool deep = false) { }

        // Return an empty array for plottables that do not appear in the legend
        public LegendItem[] GetLegendItems()
        {
            if (string.IsNullOrWhiteSpace(Label))
                return LegendItem.None;

            LegendItem item = new(this)
            {
                label = Label,
                color = Color,
                borderColor = BorderColor,
                borderLineStyle = BorderLineStyle,
                borderWith = BorderLineWidth,
            };

            return LegendItem.Single(item);
        }

        // This method returns the bounds of the data
        public AxisLimits GetAxisLimits()
        {
            return new AxisLimits(
                xMin: X - RadiusX,
                xMax: X + RadiusX,
                yMin: Y - RadiusY,
                yMax: Y + RadiusY);
        }

        // This method describes how to plot the data on the cart.
        public void Render(PlotDimensions dims, System.Drawing.Bitmap bmp, bool lowQuality = false)
        {
            // Use ScottPlot's GDI helper functions to create System.Drawing objects
            using var gfx = ScottPlot.Drawing.GDI.Graphics(bmp, dims, lowQuality);
            using var pen = ScottPlot.Drawing.GDI.Pen(BorderColor, BorderLineWidth, BorderLineStyle);
            using var brush = ScottPlot.Drawing.GDI.Brush(Color, HatchColor, HatchStyle);

            // Use 'dims' methods to convert between axis coordinates and pixel positions
            float xPixel = dims.GetPixelX(X);
            float yPixel = dims.GetPixelY(Y);

            // Use 'dims' to determine how large the radius is in pixel units
            float xRadiusPixels = dims.GetPixelX(X + RadiusX) - xPixel;
            float yRadiusPixels = dims.GetPixelY(Y + RadiusY) - yPixel;

            gfx.TranslateTransform(xPixel, yPixel);
            gfx.RotateTransform(Rotation);

            double rotationRads = Rotation * Math.PI / 180;
            float xScale = (float)(xRadiusPixels * Math.Cos(rotationRads) + yRadiusPixels * Math.Sin(rotationRads));
            float yScale = (float)(yRadiusPixels * Math.Cos(rotationRads) + xRadiusPixels * Math.Sin(rotationRads));
            gfx.ScaleTransform(xScale, yScale);

            RectangleF rect = new(
                x: -1,
                y: -1f,
                width: 2,
                height: 2);

            // Render data by drawing on the Graphics object
            if (Color != Color.Transparent)
                gfx.FillEllipse(brush, rect);

            // Otherwise the pen width will be scaled as well
            using System.Drawing.Drawing2D.Matrix invertScaleMatrix = new();
            invertScaleMatrix.Scale(1 / xScale, 1 / yScale);
            pen.Transform = invertScaleMatrix;

            gfx.DrawEllipse(pen, rect);
        }
    }
}
