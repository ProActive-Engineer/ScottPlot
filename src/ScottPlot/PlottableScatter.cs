﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ScottPlot
{
    public class PlottableScatter : Plottable
    {
        public double[] xs;
        public double[] ys;
        public double[] errorX;
        public double[] errorY;
        public float errorLineWidth;
        public float errorCapSize;
        public float markerSize;
        public Pen penLine;
        public Pen penLineError;
        public Brush brush;

        public PlottableScatter(double[] xs, double[] ys, Color color, double lineWidth, double markerSize, string label,
            double[] errorX, double[] errorY, double errorLineWidth, double errorCapSize)
        {
            if (xs.Length != ys.Length)
                throw new Exception("Xs and Ys must have same length");

            if (errorY != null)
                for (int i = 0; i < errorY.Length; i++)
                    if (errorY[i] < 0)
                        errorY[i] = -errorY[i];

            if (errorX != null)
                for (int i = 0; i < errorX.Length; i++)
                    if (errorX[i] < 0)
                        errorX[i] = -errorX[i];

            this.xs = xs;
            this.ys = ys;
            this.color = color;
            this.markerSize = (float)markerSize;
            this.label = label;
            this.errorX = errorX;
            this.errorY = errorY;
            this.errorLineWidth = (float)errorLineWidth;
            this.errorCapSize = (float)errorCapSize;


            pointCount = xs.Length;

            if (xs.Length != ys.Length)
                throw new ArgumentException("X and Y arrays must have the same length");

            if ((errorX != null) && (xs.Length != errorX.Length))
                throw new ArgumentException("errorX must be the same length as the original data");

            if ((errorY != null) && (xs.Length != errorY.Length))
                throw new ArgumentException("errorY must be the same length as the original data");

            penLine = new Pen(color, (float)lineWidth)
            {
                // this prevents sharp corners
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round,
                LineJoin = System.Drawing.Drawing2D.LineJoin.Round
            };

            penLineError = new Pen(color, (float)errorLineWidth)
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

            if (errorX == null)
            {
                limits[0] = xs.Min();
                limits[1] = xs.Max();
            }
            else
            {
                limits[0] = xs[0] - errorX[0];
                limits[1] = xs[0] + errorX[0];
                for (int i = 1; i < xs.Length; i++)
                {
                    if (xs[i] - errorX[i] < limits[0])
                        limits[0] = xs[i] - errorX[i];
                    if (xs[i] + errorX[i] > limits[0])
                        limits[1] = xs[i] + errorX[i];
                }
            }

            if (errorY == null)
            {
                limits[2] = ys.Min();
                limits[3] = ys.Max();
            }
            else
            {
                limits[2] = ys[0] - errorY[0];
                limits[3] = ys[0] + errorY[0];
                for (int i = 1; i < ys.Length; i++)
                {
                    if (ys[i] - errorY[i] < limits[2])
                        limits[2] = ys[i] - errorY[i];
                    if (ys[i] + errorY[i] > limits[3])
                        limits[3] = ys[i] + errorY[i];
                }
            }
            return limits;
        }

        public override void Render(Settings settings)
        {
            Point[] points = new Point[xs.Length];
            for (int i = 0; i < xs.Length; i++)
                points[i] = settings.GetPixel(xs[i], ys[i]);

            if (errorY != null)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    PointF errorBelow = settings.GetPixel(xs[i], ys[i] - errorY[i]);
                    PointF errorAbove = settings.GetPixel(xs[i], ys[i] + errorY[i]);
                    float xCenter = errorBelow.X;
                    float yTop = errorAbove.Y;
                    float yBot = errorBelow.Y;
                    settings.gfxData.DrawLine(penLineError, xCenter, yBot, xCenter, yTop);
                    settings.gfxData.DrawLine(penLineError, xCenter - errorCapSize, yBot, xCenter + errorCapSize, yBot);
                    settings.gfxData.DrawLine(penLineError, xCenter - errorCapSize, yTop, xCenter + errorCapSize, yTop);
                }
            }

            if (errorX != null)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    PointF errorLeft = settings.GetPixel(xs[i] - errorX[i], ys[i]);
                    PointF errorRight = settings.GetPixel(xs[i] + errorX[i], ys[i]);
                    float yCenter = errorLeft.Y;
                    float xLeft = errorLeft.X;
                    float xRight = errorRight.X;
                    settings.gfxData.DrawLine(penLineError, xLeft, yCenter, xRight, yCenter);
                    settings.gfxData.DrawLine(penLineError, xLeft, yCenter - errorCapSize, xLeft, yCenter + errorCapSize);
                    settings.gfxData.DrawLine(penLineError, xRight, yCenter - errorCapSize, xRight, yCenter + errorCapSize);
                }
            }

            if (penLine.Width > 0)
                settings.gfxData.DrawLines(penLine, points);

            if (markerSize > 0)
                for (int i = 0; i < points.Length; i++)
                    settings.gfxData.FillEllipse(brush, points[i].X - markerSize / 2, points[i].Y - markerSize / 2, markerSize, markerSize);

        }
    }
}
