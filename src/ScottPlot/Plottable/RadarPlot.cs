﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ScottPlot.Drawing;

namespace ScottPlot.Plottable
{
    /// <summary>
    /// A radar chart is a graphical method of displaying multivariate data in the form of 
    /// a two-dimensional chart of three or more quantitative variables represented on axes 
    /// starting from the same point.
    /// </summary>
    public class RadarPlot : IPlottable
    {
        private double[,] Norm;
        private double NormMax;
        private double[] NormMaxes;
        public string[] CategoryLabels;
        public string[] GroupLabels;
        public Color[] FillColors;
        public Color[] LineColors;
        public Color WebColor = Color.Gray;
        public readonly bool IndependentAxes;
        public bool IsVisible { get; set; } = true;
        public int XAxisIndex { get; set; } = 0;
        public int YAxisIndex { get; set; } = 0;
        public Drawing.Font Font = new Drawing.Font();
        public bool ShowAxisValues { get; set; } = true;
        public RadarAxis AxisType { get; set; } = RadarAxis.Circle;

        public RadarPlot(double[,] values, Color[] lineColors, Color[] fillColors, bool independentAxes, double[] maxValues = null)
        {
            this.LineColors = lineColors;
            this.FillColors = fillColors;
            this.IndependentAxes = independentAxes;

            Update(values, maxValues);
        }

        public override string ToString() =>
            $"PlottableRadar with {PointCount} points and {Norm.GetUpperBound(1) + 1} categories.";

        /// <summary>
        /// Updates the internal data array <see cref="Norm"/> that holds the values to be plotted. It also computes the normalization values for the axis, which are later needed in <see cref="Render(PlotDimensions, Bitmap, bool)"/>.
        /// </summary>
        /// <param name="values">Values to be plotted. Rows correspond to the <see cref="GroupLabels"/> property and columns to the <see cref="CategoryLabels"/> property</param>
        /// <param name="maxValues">Only needed for user-normalizing (scaling) the axes. If <see cref="IndependentAxes"/> is set to <see langword="false"/>, then only the first element of this array is considered (all axis is normalized by the same factor). If <see cref="IndependentAxes"/> is set to <see langword="true"/>, then its elements correspond to each axis (categories) and are normalized independently</param>
        public void Update(double[,] values, double[] maxValues = null)
        {
            // The passed values are copied into the internal array 'Norm', which stores the plot's data.
            // Consider (@swharden and @bclehmann) if this is needed or desirable: do we want to allow the user to update its "value" array and call Render, (similarly to SignalPlot)?
            // If so, then the Array.Copy could be left out, and move the normalization at the beginning of the Render() method.
            Norm = new double[values.GetLength(0), values.GetLength(1)];
            Array.Copy(values, 0, Norm, 0, values.Length);
            
            // Normalize values, which are needed later to render the plot.
            // Consider moving this code to the very beginning of Render() method, just before the "using" statements.
            if (this.IndependentAxes)
                NormMaxes = NormalizeSeveralInPlace(Norm, maxValues);
            else
                NormMax = NormalizeInPlace(Norm, maxValues);
        }

        public void ValidateData(bool deep = false)
        {
            if (GroupLabels != null && GroupLabels.Length != Norm.GetLength(0))
                throw new InvalidOperationException("group names must match size of values");

            if (CategoryLabels != null && CategoryLabels.Length != Norm.GetLength(1))
                throw new InvalidOperationException("category names must match size of values");
        }

        /// <summary>
        /// Normalize a 2D array by dividing all values by the maximum value.
        /// </summary>
        /// <returns>maximum value in the array before normalization</returns>
        private double NormalizeInPlace(double[,] input, double[] maxValues = null)
        {
            double max;
            if (maxValues != null && maxValues.Length == 1)
            {
                max = maxValues[0];
            }
            else
            {
                max = input[0, 0];
                for (int i = 0; i < input.GetLength(0); i++)
                    for (int j = 0; j < input.GetLength(1); j++)
                        max = Math.Max(max, input[i, j]);
            }

            for (int i = 0; i < input.GetLength(0); i++)
                for (int j = 0; j < input.GetLength(1); j++)
                    input[i, j] /= max;

            return max;
        }

        /// <summary>
        /// Normalize each row of a 2D array independently by dividing all values by the maximum value.
        /// </summary>
        /// <returns>maximum value in each row of the array before normalization</returns>
        private double[] NormalizeSeveralInPlace(double[,] input, double[] maxValues = null)
        {
            double[] maxes;
            if (maxValues != null && input.GetLength(1) == maxValues.Length)
            {
                maxes = maxValues;
            }
            else
            {
                maxes = new double[input.GetLength(1)];
                for (int i = 0; i < input.GetLength(1); i++)
                {
                    double max = input[0, i];
                    for (int j = 0; j < input.GetLength(0); j++)
                    {
                        max = Math.Max(input[j, i], max);
                    }
                    maxes[i] = max;
                }
            }

            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    input[i, j] /= maxes[j];
                }
            }

            return maxes;
        }

        public LegendItem[] GetLegendItems()
        {
            if (GroupLabels is null)
                return null;

            List<LegendItem> legendItems = new List<LegendItem>();
            for (int i = 0; i < GroupLabels.Length; i++)
            {
                var item = new LegendItem()
                {
                    label = GroupLabels[i],
                    color = FillColors[i],
                    lineWidth = 10,
                    markerShape = MarkerShape.none
                };
                legendItems.Add(item);
            }

            return legendItems.ToArray();
        }

        public AxisLimits GetAxisLimits() =>
            (GroupLabels != null) ? new AxisLimits(-3.5, 3.5, -3.5, 3.5) : new AxisLimits(-2.5, 2.5, -2.5, 2.5);

        public int PointCount { get => Norm.Length; }

        public void Render(PlotDimensions dims, Bitmap bmp, bool lowQuality = false)
        {
            int numGroups = Norm.GetUpperBound(0) + 1;
            int numCategories = Norm.GetUpperBound(1) + 1;
            double sweepAngle = 2 * Math.PI / numCategories;
            double minScale = new double[] { dims.PxPerUnitX, dims.PxPerUnitX }.Min();
            PointF origin = new PointF(dims.GetPixelX(0), dims.GetPixelY(0));
            double[] radii = new double[] { 0.25 * minScale, 0.5 * minScale, 1 * minScale };

            using (Graphics gfx = GDI.Graphics(bmp, dims, lowQuality))
            using (Pen pen = GDI.Pen(WebColor))
            using (Brush brush = GDI.Brush(Color.Black))
            using (StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center })
            using (StringFormat sf2 = new StringFormat())
            using (System.Drawing.Font font = GDI.Font(Font))
            using (Brush fontBrush = GDI.Brush(Font.Color))
            {
                for (int i = 0; i < radii.Length; i++)
                {
                    double hypotenuse = (radii[i] / radii[radii.Length - 1]);

                    if (AxisType == RadarAxis.Circle)
                    {
                        gfx.DrawEllipse(pen, (int)(origin.X - radii[i]), (int)(origin.Y - radii[i]), (int)(radii[i] * 2), (int)(radii[i] * 2));
                    }
                    else if (AxisType == RadarAxis.Polygon)
                    {
                        PointF[] points = new PointF[numCategories];
                        for (int j = 0; j < numCategories; j++)
                        {
                            float x = (float)(hypotenuse * Math.Cos(sweepAngle * j - Math.PI / 2) * minScale + origin.X);
                            float y = (float)(hypotenuse * Math.Sin(sweepAngle * j - Math.PI / 2) * minScale + origin.Y);

                            points[j] = new PointF(x, y);
                        }
                        gfx.DrawPolygon(pen, points);
                    }
                    if (ShowAxisValues)
                    {
                        if (IndependentAxes)
                        {
                            for (int j = 0; j < numCategories; j++)
                            {
                                float x = (float)(hypotenuse * Math.Cos(sweepAngle * j - Math.PI / 2) * minScale + origin.X);
                                float y = (float)(hypotenuse * Math.Sin(sweepAngle * j - Math.PI / 2) * minScale + origin.Y);

                                sf2.Alignment = x < origin.X ? StringAlignment.Far : StringAlignment.Near;
                                sf2.LineAlignment = y < origin.Y ? StringAlignment.Far : StringAlignment.Near;

                                double val = NormMaxes[j] * radii[i] / minScale;
                                gfx.DrawString($"{val:f1}", font, fontBrush, x, y, sf2);
                            }
                        }
                        else
                        {
                            double val = NormMax * radii[i] / minScale;
                            gfx.DrawString($"{val:f1}", font, fontBrush, origin.X, (float)(-radii[i] + origin.Y), sf2);
                        }
                    }
                }

                for (int i = 0; i < numCategories; i++)
                {
                    PointF destination = new PointF((float)(1.1 * Math.Cos(sweepAngle * i - Math.PI / 2) * minScale + origin.X), (float)(1.1 * Math.Sin(sweepAngle * i - Math.PI / 2) * minScale + origin.Y));
                    gfx.DrawLine(pen, origin, destination);

                    if (CategoryLabels != null)
                    {
                        PointF textDestination = new PointF(
                            (float)(1.3 * Math.Cos(sweepAngle * i - Math.PI / 2) * minScale + origin.X),
                            (float)(1.3 * Math.Sin(sweepAngle * i - Math.PI / 2) * minScale + origin.Y));

                        if (Math.Abs(textDestination.X - origin.X) < 0.1)
                            sf.Alignment = StringAlignment.Center;
                        else
                            sf.Alignment = dims.GetCoordinateX(textDestination.X) < 0 ? StringAlignment.Far : StringAlignment.Near;
                        gfx.DrawString(CategoryLabels[i], font, fontBrush, textDestination, sf);
                    }
                }

                for (int i = 0; i < numGroups; i++)
                {
                    PointF[] points = new PointF[numCategories];
                    for (int j = 0; j < numCategories; j++)
                        points[j] = new PointF(
                            (float)(Norm[i, j] * Math.Cos(sweepAngle * j - Math.PI / 2) * minScale + origin.X),
                            (float)(Norm[i, j] * Math.Sin(sweepAngle * j - Math.PI / 2) * minScale + origin.Y));

                    ((SolidBrush)brush).Color = FillColors[i];
                    pen.Color = LineColors[i];
                    gfx.FillPolygon(brush, points);
                    gfx.DrawPolygon(pen, points);
                }
            }
        }
    }
}
