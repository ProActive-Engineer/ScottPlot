﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace ScottPlot
{

    internal class PlottableSignal : Plottable
    {
        public double[] ys;
        public double sampleRate;
        public double samplePeriod;
        public Pen pen;
        public Brush brush;

        public PlottableSignal(double[] ys, double sampleRate, Color color, float linewidth)
        {
            this.ys = ys;
            this.sampleRate = sampleRate;
            this.samplePeriod = 1.0 / sampleRate;
            pointCount = ys.Length;
            brush = new SolidBrush(color);
            pen = new Pen(color, linewidth)
            {
                // this prevents sharp corners
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round,
                LineJoin = System.Drawing.Drawing2D.LineJoin.Round
            };
        }

        public override string ToString()
        {
            return $"PlottableSignal with {pointCount} points";
        }

        public override double[] GetLimits()
        {
            double[] limits = new double[4];
            limits[0] = 0;
            limits[1] = samplePeriod * ys.Length;
            limits[2] = ys.Min();
            limits[3] = ys.Max();
            return limits;
        }

        public override void Render(Settings settings)
        {

            double dataSpanUnits = ys.Length * samplePeriod;
            double columnSpanUnits = settings.xAxisSpan / settings.dataSize.Width;
            double columnPointCount = (columnSpanUnits / dataSpanUnits) * ys.Length;
            double offsetUnits = settings.axis[0];
            double offsetPoints = offsetUnits / samplePeriod;
            int visibleIndex1 = (int)(offsetPoints);
            int visibleIndex2 = (int)(offsetPoints + columnPointCount * (settings.dataSize.Width + 1));
            int visiblePointCount = visibleIndex2 - visibleIndex1;

            List<Point> linePoints = new List<Point>(settings.dataSize.Width * 2 + 1);
            for (int xPx = 0; xPx < settings.dataSize.Width; xPx++)
            {
                // determine data indexes for this pixel column
                int index1 = (int)(offsetPoints + columnPointCount * xPx);
                int index2 = (int)(offsetPoints + columnPointCount * (xPx + 1));

                // skip invalid data index values
                if ((index2 < 0) || (index1 > ys.Length - 1))
                    continue;
                if (index1 < 0)
                    index1 = 0;
                if (index2 > ys.Length - 1)
                    index2 = ys.Length - 1;

                // get the min and max value for this column
                double lowestValue = ys[index1];
                double highestValue = ys[index1];
                for (int i = index1; i < index2; i++)
                {
                    if (ys[i] < lowestValue)
                        lowestValue = ys[i];
                    if (ys[i] > highestValue)
                        highestValue = ys[i];
                }
                int yPxHigh = settings.GetPoint(0, lowestValue).Y;
                int yPxLow = settings.GetPoint(0, highestValue).Y;

                // adjust order of points to enhance anti-aliasing
                if ((linePoints.Count < 2) || (yPxLow < linePoints[linePoints.Count - 1].Y))
                {
                    linePoints.Add(new Point(xPx, yPxLow));
                    linePoints.Add(new Point(xPx, yPxHigh));
                }
                else
                {
                    linePoints.Add(new Point(xPx, yPxHigh));
                    linePoints.Add(new Point(xPx, yPxLow));
                }
            }

            SmoothingMode originalDataAntiAliasMode = settings.gfxData.SmoothingMode;
            if (settings.antiAliasData)
                settings.gfxData.SmoothingMode = SmoothingMode.AntiAlias;
            else
                settings.gfxData.SmoothingMode = SmoothingMode.None;

            if (linePoints.Count > 0)
            {
                settings.gfxData.DrawLines(pen, linePoints.ToArray());
                if (visiblePointCount < settings.dataSize.Width)
                {
                    for (int i = visibleIndex1; i < visibleIndex2; i++)
                    {
                        Point point = settings.GetPoint(samplePeriod * i, ys[i]);
                        int markerWidth = 4;
                        int markerWidthHalf = markerWidth / 2;
                        Rectangle rect = new Rectangle(point.X - markerWidthHalf, point.Y - markerWidthHalf, markerWidth, markerWidth);
                        settings.gfxData.DrawEllipse(pen, rect);
                    }
                }
            }

            settings.gfxData.SmoothingMode = originalDataAntiAliasMode;

        }
    }


    internal class PlottableScatter : Plottable
    {
        public double[] xs;
        public double[] ys;
        public float markerSize;
        public Pen pen;
        public Brush brush;

        public PlottableScatter(double[] xs, double[] ys, Color color, float lineWidth, float markerSize)
        {
            if (xs.Length != ys.Length)
                throw new Exception("Xs and Ys must have same length");

            this.xs = xs;
            this.ys = ys;
            this.markerSize = markerSize;
            pointCount = xs.Length;

            pen = new Pen(color, lineWidth)
            {
                // this prevents sharp corners
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round,
                LineJoin = System.Drawing.Drawing2D.LineJoin.Round
            };

            brush = new SolidBrush(color);
        }

        public override string ToString()
        {
            return $"PlottableScatter with {pointCount} points";
        }

        public override double[] GetLimits()
        {
            double[] limits = new double[4];
            limits[0] = xs.Min();
            limits[1] = xs.Max();
            limits[2] = ys.Min();
            limits[3] = ys.Max();
            return limits;
        }

        public override void Render(Settings settings)
        {
            Point[] points = new Point[xs.Length];
            for (int i = 0; i < xs.Length; i++)
                points[i] = settings.GetPoint(xs[i], ys[i]);

            if (pen.Width > 0)
                settings.gfxData.DrawLines(pen, points);

            if (markerSize > 0)
                for (int i = 0; i < points.Length; i++)
                    settings.gfxData.FillEllipse(brush, points[i].X - markerSize / 2, points[i].Y - markerSize / 2, markerSize, markerSize);

        }
    }

    public class PlottableText : Plottable
    {
        public double x;
        public double y;
        public string text;
        public Brush brush;
        public Font font;

        public PlottableText(string text, double x, double y, Color color, string fontName = "Arial", float fontSize = 12, bool bold = false)
        {
            this.text = text;
            this.x = x;
            this.y = y;
            brush = new SolidBrush(color);
            FontStyle fontStyle = (bold == true) ? FontStyle.Bold : FontStyle.Regular;
            font = new Font(fontName, (float)fontSize, fontStyle);

            pointCount = 1;
        }

        public override string ToString()
        {
            return $"PlottableText \"{text}\" at ({x}, {y})";
        }

        public override double[] GetLimits()
        {
            return new double[] { x, x, y, y };
        }

        public override void Render(Settings settings)
        {
            settings.gfxData.DrawString(text, font, brush, settings.GetPoint(x, y));
        }
    }

    public class PlottableAxLine : Plottable
    {
        public double position;
        public bool vertical;
        public string orientation;
        public Pen pen;

        public PlottableAxLine(double position, bool vertical, Color color, float lineWidth)
        {
            this.position = position;
            this.vertical = vertical;
            orientation = (vertical) ? "vertical" : "horizontal";
            pen = new Pen(color, lineWidth);
            pointCount = 1;
        }

        public override string ToString()
        {
            return $"PlottableAxLine ({orientation}) at {position}";
        }

        public override double[] GetLimits()
        {
            return new double[] { 0, 0, 0, 0 };
        }

        public override void Render(Settings settings)
        {
            Point pt1, pt2;

            if (vertical)
            {
                pt1 = settings.GetPoint(position, settings.axis[2]);
                pt2 = settings.GetPoint(position, settings.axis[3]);
            }
            else
            {
                pt1 = settings.GetPoint(settings.axis[0], position);
                pt2 = settings.GetPoint(settings.axis[1], position);
            }

            settings.gfxData.DrawLine(pen, pt1, pt2);
        }
    }
}
