﻿using ScottPlot.Renderable;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ScottPlot.Drawing;
using System.Globalization;
using System.Diagnostics;

namespace ScottPlot
{
    /// <summary>
    /// This module holds state for figure dimensions, axis limits, plot contents, and styling options.
    /// A plot can be duplicated by copying the full stae of this settings module.
    /// </summary>
    public class Settings
    {
        public bool AllAxesHaveBeenSet => Axes.All(x => x.Dims.HasBeenSet);

        public int Width => (int)XAxis.Dims.FigureSizePx;
        public int Height => (int)YAxis.Dims.FigureSizePx;
        public float DataOffsetX => XAxis.Dims.DataOffsetPx;
        public float DataOffsetY => YAxis.Dims.DataOffsetPx;
        public float DataWidth => XAxis.Dims.DataSizePx;
        public float DataHeight => YAxis.Dims.DataSizePx;

        /// <summary>
        /// Adjust data padding based on axis size
        /// </summary>
        public void TightenLayout()
        {
            float padLeft = Axes.Where(x => x.Edge == Edge.Left).Select(x => x.PixelSize).Sum();
            float padRight = Axes.Where(x => x.Edge == Edge.Right).Select(x => x.PixelSize).Sum();
            float padBottom = Axes.Where(x => x.Edge == Edge.Bottom).Select(x => x.PixelSize).Sum();
            float padTop = Axes.Where(x => x.Edge == Edge.Top).Select(x => x.PixelSize).Sum();
            XAxis.Dims.SetPadding(padLeft, padRight);
            YAxis.Dims.SetPadding(padTop, padBottom);
        }

        // plottables
        public readonly List<IRenderable> Plottables = new List<IRenderable>();
        public Color GetNextColor() { return PlottablePalette.GetColor(Plottables.Count); }

        // settings the user can customize
        public readonly FigureBackground FigureBackground = new FigureBackground();
        public readonly DataBackground DataBackground = new DataBackground();
        public readonly BenchmarkMessage BenchmarkMessage = new BenchmarkMessage();
        public readonly ErrorMessage ErrorMessage = new ErrorMessage();
        public readonly Legend CornerLegend = new Legend();
        public readonly ZoomRectangle ZoomRectangle = new ZoomRectangle();
        public Palette PlottablePalette = Palette.Category10;

        public List<Axis> Axes = new List<Axis>() {
            new DefaultLeftAxis(),
            new DefaultRightAxis(),
            new DefaultBottomAxis(),
            new DefaultTopAxis()
        };

        public Axis YAxis => Axes[0];
        public Axis YAxis2 => Axes[1];
        public Axis XAxis => Axes[2];
        public Axis XAxis2 => Axes[3];
        public Axis[] PrimaryAxes => Axes.Take(4).ToArray();

        public Axis GetXAxis(int xAxisIndex) => Axes.Where(x => x.IsHorizontal && x.AxisIndex == xAxisIndex).First();
        public Axis GetYAxis(int yAxisIndex) => Axes.Where(x => x.IsVertical && x.AxisIndex == yAxisIndex).First();

        /*
         * ##################################################################################
         * # OLD SETTINGS WHICH I AM WORKING TO STRANGLE
         * 
         */

        public double GetPixelX(double locationX) => XAxis.Dims.GetPixel(locationX);
        public double GetPixelY(double locationY) => YAxis.Dims.GetPixel(locationY);
        public PointF GetPixel(double locationX, double locationY) => new PointF((float)GetPixelX(locationX), (float)GetPixelY(locationY));

        public double GetLocationX(double pixelX) => XAxis.Dims.GetUnit((float)pixelX);
        public double GetLocationY(double pixelY) => YAxis.Dims.GetUnit((float)pixelY);
        public PointF GetLocation(double pixelX, double pixelY) => new PointF((float)GetLocationX(pixelX), (float)GetLocationY(pixelY));

        public PlotDimensions2D GetPlotDimensions(int xAxisIndex, int yAxisIndex)
        {
            var xAxis = GetXAxis(xAxisIndex);
            var yAxis = GetYAxis(yAxisIndex);

            // determine figure dimensions based on primary X and Y axis
            var figureSize = new SizeF(XAxis.Dims.FigureSizePx, YAxis.Dims.FigureSizePx);
            var dataSize = new SizeF(XAxis.Dims.DataSizePx, YAxis.Dims.DataSizePx);
            var dataOffset = new PointF(XAxis.Dims.DataOffsetPx, YAxis.Dims.DataOffsetPx);

            // determine figure dimensions based on specific X and Y axes
            //var figureSize = new SizeF(xAxis.Dims.FigureSizePx, yAxis.Dims.FigureSizePx);
            //var dataSize = new SizeF(xAxis.Dims.DataSizePx, yAxis.Dims.DataSizePx);
            //var dataOffset = new PointF(xAxis.Dims.DataOffsetPx, yAxis.Dims.DataOffsetPx);

            // determine axis limits based on specific X and Y axes
            (double xMin, double xMax) = xAxis.Dims.RationalLimits();
            (double yMin, double yMax) = yAxis.Dims.RationalLimits();
            AxisLimits2D limits = new AxisLimits2D(xMin, xMax, yMin, yMax);

            return new PlotDimensions2D(figureSize, dataSize, dataOffset, limits);
        }

        public void Resize(int width, int height)
        {
            foreach (Axis axis in Axes)
                axis.Dims.Resize(axis.IsHorizontal ? width : height);
        }

        public void ResetAxes()
        {
            foreach (Axis axis in Axes)
                axis.Dims.ResetLimits();
        }

        public void AxisSet(double? xMin, double? xMax, double? yMin, double? yMax, int xAxisIndex, int yAxisIndex)
        {
            foreach (Axis axis in Axes)
            {
                if (axis.IsHorizontal && axis.AxisIndex == xAxisIndex)
                    axis.Dims.SetAxis(xMin, xMax);
                if (axis.IsVertical && axis.AxisIndex == yAxisIndex)
                    axis.Dims.SetAxis(yMin, yMax);
            }
        }

        public double[] AxisLimitsArray(int xAxisIndex, int yAxisIndex)
        {
            var xAxis = GetXAxis(xAxisIndex);
            var yAxis = GetYAxis(yAxisIndex);
            return new double[] { xAxis.Dims.Min, xAxis.Dims.Max, yAxis.Dims.Min, yAxis.Dims.Max };
        }

        public void AxesPanPx(int dxPx, int dyPx)
        {
            foreach (Axis axis in Axes)
                axis.Dims.PanPx(axis.IsHorizontal ? dxPx : dyPx);
        }

        public void AxesZoomPx(int xPx, int yPx, bool lockRatio = false)
        {
            // TODO: equal axes
            foreach (Axis axis in Axes)
            {
                double deltaPx = axis.IsHorizontal ? xPx : yPx;
                double delta = deltaPx * axis.Dims.UnitsPerPx;
                double deltaFrac = delta / (Math.Abs(delta) + axis.Dims.Span);
                axis.Dims.Zoom(Math.Pow(10, deltaFrac));
            }
        }

        public void AxisAutoUnsetAxes()
        {
            if (Axes.Any(x => x.Dims.HasBeenSet == false && x.AxisIndex == 0))
                AxisAuto(xAxisIndex: 0, yAxisIndex: 0);
            if (Axes.Any(x => x.Dims.HasBeenSet == false && x.AxisIndex == 1))
                AxisAuto(xAxisIndex: 1, yAxisIndex: 1);
        }

        public void AxisAuto(
            double horizontalMargin = .1,
            double verticalMargin = .1,
            bool xExpandOnly = false,
            bool yExpandOnly = false,
            bool autoX = true,
            bool autoY = true,
            int xAxisIndex = 0,
            int yAxisIndex = 0
            )
        {
            var xAxis = GetXAxis(xAxisIndex);
            var yAxis = GetYAxis(yAxisIndex);

            var oldLimits = new AxisLimits2D(xAxis.Dims.Min, xAxis.Dims.Max, yAxis.Dims.Min, yAxis.Dims.Max);
            var newLimits = new AxisLimits2D();

            foreach (var plottable in Plottables)
            {
                if (plottable is IUsesAxes p)
                {
                    if (p.HorizontalAxisIndex != xAxisIndex || p.VerticalAxisIndex != yAxisIndex)
                        continue;

                    var (xMin, xMax, yMin, yMax) = p.GetAxisLimits();
                    if (autoX && !double.IsNaN(xMin))
                    {
                        if (double.IsNaN(newLimits.XMin)) newLimits.XMin = xMin;
                        newLimits.XMin = Math.Min(newLimits.XMin, xMin);
                    }
                    if (autoX && !double.IsNaN(xMax))
                    {
                        if (double.IsNaN(newLimits.XMax)) newLimits.XMax = xMax;
                        newLimits.XMax = Math.Max(newLimits.XMax, xMax);
                    }
                    if (autoY && !double.IsNaN(yMin))
                    {
                        if (double.IsNaN(newLimits.YMin)) newLimits.YMin = yMin;
                        newLimits.YMin = Math.Min(newLimits.YMin, yMin);
                    }
                    if (autoY && !double.IsNaN(yMax))
                    {
                        if (double.IsNaN(newLimits.YMax)) newLimits.YMax = yMax;
                        newLimits.YMax = Math.Max(newLimits.YMax, yMax);
                    }
                }
            }

            // TODO: equal axis
            /*
            if (axes.equalAxes)
            {
                var xUnitsPerPixel = newLimits.xSpan / (DataWidth * (1 - horizontalMargin));
                var yUnitsPerPixel = newLimits.ySpan / (DataHeight * (1 - verticalMargin));
                axes.Set(newLimits);
                if (yUnitsPerPixel > xUnitsPerPixel)
                    axes.Zoom((1 - horizontalMargin) * xUnitsPerPixel / yUnitsPerPixel, 1 - verticalMargin);
                else
                    axes.Zoom(1 - horizontalMargin, (1 - verticalMargin) * yUnitsPerPixel / xUnitsPerPixel);
                return;
            }
            */

            if (xExpandOnly)
            {
                xAxis.Dims.SetAxis(newLimits.XMin, newLimits.XMax);
                yAxis.Dims.SetAxis(oldLimits.YMin, oldLimits.YMax);
            }
            else if (yExpandOnly)
            {
                xAxis.Dims.SetAxis(oldLimits.XMin, oldLimits.XMax);
                yAxis.Dims.SetAxis(newLimits.YMin, newLimits.YMax);
            }
            else
            {
                xAxis.Dims.SetAxis(newLimits.XMin, newLimits.XMax);
                yAxis.Dims.SetAxis(newLimits.YMin, newLimits.YMax);
            }

            double zoomFracX = yExpandOnly ? 1 : 1 - horizontalMargin;
            double zoomFracY = xExpandOnly ? 1 : 1 - verticalMargin;

            xAxis.Dims.Zoom(zoomFracX);
            yAxis.Dims.Zoom(zoomFracY);
        }
    }
}
