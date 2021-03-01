﻿using ScottPlot.Drawing;
using System;
using System.Drawing;
using System.Data;
using System.Linq;

namespace ScottPlot.Plottable
{
    public enum BarStyle
    {
        Bar,
        Lollipop,
        ClevelandDot
    }

    /// <summary>
    /// Bar plots display a series of bars. 
    /// Positions are defined by Xs.
    /// Heights are defined by Ys (relative to BaseValue and YOffsets).
    /// </summary>
    public class BarPlot : IPlottable
    {
        // data
        public double[] Xs;
        public double XOffset;
        public double[] Ys;
        public double[] YErrors;
        public double[] YOffsets;

        // customization
        public bool IsVisible { get; set; } = true;
        public int XAxisIndex { get; set; } = 0;
        public int YAxisIndex { get; set; } = 0;
        public string Label;
        public Color FillColor = Color.Green;
        public Color FillColorNegative = Color.Red;
        public Color FillColorHatch = Color.Blue;
        public HatchStyle HatchStyle = HatchStyle.None;
        public Color ErrorColor = Color.Black;
        public float ErrorLineWidth = 1;
        public double ErrorCapSize = .4;
        public Color BorderColor = Color.Black;
        public float BorderLineWidth = 1;
        public BarStyle DisplayStyle = BarStyle.Bar;
        public float LollipopRadius = 5;
        public Color ClevelandColor1 = Color.Green;
        public Color ClevelandColor2 = Color.Red;
        public string ClevelandLabel1 = "";
        public string ClevelandLabel2 = "";

        public readonly Drawing.Font Font = new Drawing.Font();
        public string FontName { set => Font.Name = value; }
        public float FontSize { set => Font.Size = value; }
        public bool FontBold { set => Font.Bold = value; }
        public Color FontColor { set => Font.Color = value; }

        public double BarWidth = .8;
        public double BaseValue = 0;
        public bool VerticalOrientation = true;
        public bool HorizontalOrientation { get => !VerticalOrientation; set => VerticalOrientation = !value; }
        public bool ShowValuesAboveBars;

        public BarPlot(double[] xs, double[] ys, double[] yErr, double[] yOffsets)
        {
            if (ys is null || ys.Length == 0)
                throw new InvalidOperationException("ys must be an array that contains elements");

            Ys = ys;
            Xs = xs ?? DataGen.Consecutive(ys.Length);
            YErrors = yErr ?? DataGen.Zeros(ys.Length);
            YOffsets = yOffsets ?? DataGen.Zeros(ys.Length);
        }

        public AxisLimits GetAxisLimits()
        {
            double valueMin = double.PositiveInfinity;
            double valueMax = double.NegativeInfinity;
            double positionMin = double.PositiveInfinity;
            double positionMax = double.NegativeInfinity;

            for (int i = 0; i < Xs.Length; i++)
            {
                if (DisplayStyle != BarStyle.ClevelandDot)
                {
                    valueMin = Math.Min(valueMin, Ys[i] - YErrors[i] + YOffsets[i]);
                    valueMax = Math.Max(valueMax, Ys[i] + YErrors[i] + YOffsets[i]);
                }
                else // For Cleveland Dot Plots the YOffset is rendered as a dot
                {
                    valueMin = new double[] { valueMin, Ys[i] - YErrors[i] + YOffsets[i], YOffsets[i] }.Min();
                    valueMax = new double[] { valueMin, Ys[i] + YErrors[i] + YOffsets[i], YOffsets[i] }.Max();
                }
                positionMin = Math.Min(positionMin, Xs[i]);
                positionMax = Math.Max(positionMax, Xs[i]);
            }

            valueMin = Math.Min(valueMin, BaseValue);
            valueMax = Math.Max(valueMax, BaseValue);

            if (ShowValuesAboveBars)
                valueMax += (valueMax - valueMin) * .1; // increase by 10% to accomodate label

            positionMin -= BarWidth / 2;
            positionMax += BarWidth / 2;

            positionMin += XOffset;
            positionMax += XOffset;

            return VerticalOrientation ?
                new AxisLimits(positionMin, positionMax, valueMin, valueMax) :
                new AxisLimits(valueMin, valueMax, positionMin, positionMax);
        }

        public void ValidateData(bool deep = false)
        {
            Validate.AssertHasElements("xs", Xs);
            Validate.AssertHasElements("ys", Ys);
            Validate.AssertHasElements("yErr", YErrors);
            Validate.AssertHasElements("yOffsets", YOffsets);
            Validate.AssertEqualLength("xs, ys, yErr, and yOffsets", Xs, Ys, YErrors, YOffsets);

            if (deep)
            {
                Validate.AssertAllReal("xs", Xs);
                Validate.AssertAllReal("ys", Ys);
                Validate.AssertAllReal("yErr", YErrors);
                Validate.AssertAllReal("yOffsets", YOffsets);
            }
        }

        public void Render(PlotDimensions dims, Bitmap bmp, bool lowQuality = false)
        {
            using (Graphics gfx = GDI.Graphics(bmp, dims, lowQuality))
            {
                for (int barIndex = 0; barIndex < Ys.Length; barIndex++)
                {
                    if (VerticalOrientation)
                        RenderBarVertical(dims, gfx, Xs[barIndex] + XOffset, Ys[barIndex], YErrors[barIndex], YOffsets[barIndex]);
                    else
                        RenderBarHorizontal(dims, gfx, Xs[barIndex] + XOffset, Ys[barIndex], YErrors[barIndex], YOffsets[barIndex]);
                }
            }
        }

        private void RenderBarFromRect(RectangleF rect, bool negative, Graphics gfx)
        {
            float centerPx = HorizontalOrientation ? rect.Y + rect.Height / 2 : rect.X + rect.Width / 2;
            using (var outlinePen = new Pen(BorderColor, BorderLineWidth))
            using (var fillPen = new Pen(negative ? FillColorNegative : FillColor))
            using (var fillBrush = GDI.Brush(negative ? FillColorNegative : FillColor, FillColorHatch, HatchStyle))
            {
                switch (DisplayStyle)
                {
                    case BarStyle.Lollipop:

                        if (HorizontalOrientation)
                        {
                            gfx.FillEllipse(fillBrush, negative ? rect.X : rect.X + rect.Width, centerPx - LollipopRadius / 2, LollipopRadius, LollipopRadius);
                            gfx.DrawLine(fillPen, rect.X, centerPx, rect.X + rect.Width, centerPx);
                        }
                        else
                        {
                            gfx.FillEllipse(fillBrush, centerPx - LollipopRadius / 2, !negative ? rect.Y : rect.Y + rect.Height, LollipopRadius, LollipopRadius);
                            gfx.DrawLine(fillPen, centerPx, rect.Y, centerPx, rect.Y + rect.Height);
                        }
                        break;

                    case BarStyle.ClevelandDot:
                        using (var dot1Brush = GDI.Brush(ClevelandColor1))
                        using (var dot2Brush = GDI.Brush(ClevelandColor2))
                        {
                            if (HorizontalOrientation)
                            {
                                gfx.FillEllipse(dot2Brush, negative ? rect.X : rect.X + rect.Width, centerPx - LollipopRadius / 2, LollipopRadius, LollipopRadius);
                                gfx.FillEllipse(dot1Brush, negative ? rect.X + rect.Width : rect.X, centerPx - LollipopRadius / 2, LollipopRadius, LollipopRadius); // Ensure the first dot is drawn overtop the second
                                gfx.DrawLine(fillPen, rect.X, centerPx, rect.X + rect.Width, centerPx);
                            }
                            else
                            {
                                gfx.FillEllipse(dot2Brush, centerPx - LollipopRadius / 2, !negative ? rect.Y : rect.Y + rect.Height, LollipopRadius, LollipopRadius);
                                gfx.FillEllipse(dot1Brush, centerPx - LollipopRadius / 2, !negative ? rect.Y + rect.Height : rect.Y, LollipopRadius, LollipopRadius); // Ensure the first dot is drawn overtop the second
                                gfx.DrawLine(fillPen, centerPx, rect.Y, centerPx, rect.Y + rect.Height);
                            }
                        }

                        break;

                    case BarStyle.Bar:
                    default:
                        gfx.FillRectangle(fillBrush, rect.X, rect.Y, rect.Width, rect.Height);
                        if (BorderLineWidth > 0)
                        {
                            gfx.DrawRectangle(outlinePen, rect.X, rect.Y, rect.Width, rect.Height);
                        }
                        break;
                }
            }
        }

        private void RenderBarVertical(PlotDimensions dims, Graphics gfx, double position, double value, double valueError, double yOffset)
        {
            // bar body
            float centerPx = dims.GetPixelX(position);
            double edge1 = position - BarWidth / 2;
            double value1 = Math.Min(BaseValue, value) + yOffset;
            double value2 = Math.Max(BaseValue, value) + yOffset;
            double valueSpan = value2 - value1;

            var rect = new RectangleF(
                x: dims.GetPixelX(edge1),
                y: dims.GetPixelY(value2),
                width: (float)(BarWidth * dims.PxPerUnitX),
                height: (float)(valueSpan * dims.PxPerUnitY));

            // errorbar
            double error1 = value > 0 ? value2 - Math.Abs(valueError) : value1 - Math.Abs(valueError);
            double error2 = value > 0 ? value2 + Math.Abs(valueError) : value1 + Math.Abs(valueError);
            float capPx1 = dims.GetPixelX(position - ErrorCapSize * BarWidth / 2);
            float capPx2 = dims.GetPixelX(position + ErrorCapSize * BarWidth / 2);
            float errorPx2 = dims.GetPixelY(error2);
            float errorPx1 = dims.GetPixelY(error1);

            RenderBarFromRect(rect, value < 0, gfx);

            if (ErrorLineWidth > 0 && valueError > 0)
            {
                using (var errorPen = new Pen(ErrorColor, ErrorLineWidth))
                {
                    gfx.DrawLine(errorPen, centerPx, errorPx1, centerPx, errorPx2);
                    gfx.DrawLine(errorPen, capPx1, errorPx1, capPx2, errorPx1);
                    gfx.DrawLine(errorPen, capPx1, errorPx2, capPx2, errorPx2);
                }
            }

            if (ShowValuesAboveBars)
                using (var valueTextFont = GDI.Font(Font))
                using (var valueTextBrush = GDI.Brush(Font.Color))
                using (var sf = new StringFormat() { LineAlignment = StringAlignment.Far, Alignment = StringAlignment.Center })
                    gfx.DrawString(value.ToString(), valueTextFont, valueTextBrush, centerPx, rect.Y, sf);
        }

        private void RenderBarHorizontal(PlotDimensions dims, Graphics gfx, double position, double value, double valueError, double yOffset)
        {
            // bar body
            float centerPx = dims.GetPixelY(position);
            double edge2 = position + BarWidth / 2;
            double value1 = Math.Min(BaseValue, value) + yOffset;
            double value2 = Math.Max(BaseValue, value) + yOffset;
            double valueSpan = value2 - value1;
            var rect = new RectangleF(
                x: dims.GetPixelX(value1),
                y: dims.GetPixelY(edge2),
                height: (float)(BarWidth * dims.PxPerUnitY),
                width: (float)(valueSpan * dims.PxPerUnitX));

            RenderBarFromRect(rect, value < 0, gfx);

            // errorbar
            double error1 = value > 0 ? value2 - Math.Abs(valueError) : value1 - Math.Abs(valueError);
            double error2 = value > 0 ? value2 + Math.Abs(valueError) : value1 + Math.Abs(valueError);
            float capPx1 = dims.GetPixelY(position - ErrorCapSize * BarWidth / 2);
            float capPx2 = dims.GetPixelY(position + ErrorCapSize * BarWidth / 2);
            float errorPx2 = dims.GetPixelX(error2);
            float errorPx1 = dims.GetPixelX(error1);

            if (ErrorLineWidth > 0 && valueError > 0)
            {
                using (var errorPen = new Pen(ErrorColor, ErrorLineWidth))
                {
                    gfx.DrawLine(errorPen, errorPx1, centerPx, errorPx2, centerPx);
                    gfx.DrawLine(errorPen, errorPx1, capPx2, errorPx1, capPx1);
                    gfx.DrawLine(errorPen, errorPx2, capPx2, errorPx2, capPx1);
                }
            }

            if (ShowValuesAboveBars)
                using (var valueTextFont = GDI.Font(Font))
                using (var valueTextBrush = GDI.Brush(Font.Color))
                using (var sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near })
                    gfx.DrawString(value.ToString(), valueTextFont, valueTextBrush, rect.X + rect.Width, centerPx, sf);
        }

        public override string ToString()
        {
            string label = string.IsNullOrWhiteSpace(this.Label) ? "" : $" ({this.Label})";
            return $"PlottableBar{label} with {PointCount} points";
        }

        public int PointCount { get => Ys is null ? 0 : Ys.Length; }

        public LegendItem[] GetLegendItems()
        {
            if (DisplayStyle != BarStyle.ClevelandDot)
            {
                var singleItem = new LegendItem()
                {
                    label = Label,
                    color = FillColor,
                    lineWidth = 10,
                    markerShape = MarkerShape.none,
                    hatchColor = FillColorHatch,
                    hatchStyle = HatchStyle,
                    borderColor = BorderColor,
                    borderWith = BorderLineWidth
                };
                return new LegendItem[] { singleItem };
            }
            else
            {
                var firstDot = new LegendItem()
                {
                    label = ClevelandLabel1,
                    color = ClevelandColor1,
                    lineStyle = LineStyle.None,
                    markerShape = MarkerShape.filledCircle,
                    markerSize = 5,
                };
                var secondDot = new LegendItem()
                {
                    label = ClevelandLabel2,
                    color = ClevelandColor2,
                    lineStyle = LineStyle.None,
                    markerShape = MarkerShape.filledCircle,
                    markerSize = 5,
                };

                return new LegendItem[] { firstDot, secondDot };
            }
        }
    }
}
