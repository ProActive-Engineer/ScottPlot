﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ScottPlot
{
    public class Renderer
    {
        public static void FigureClear(Settings settings)
        {
            if (settings.gfxFigure != null)
                settings.gfxFigure.Clear(settings.misc.figureBackgroundColor);
        }

        public static void DataBackground(Settings settings)
        {
            if (settings.gfxData != null)
                settings.gfxData.Clear(settings.misc.dataBackgroundColor);
        }

        public static void DataGrid(Settings settings)
        {
            if (!settings.grid.visible)
                return;

            Pen pen = new Pen(settings.grid.color);

            for (int i = 0; i < settings.ticks.tickCollectionX.tickPositionsMajor.Length; i++)
            {
                double value = settings.ticks.tickCollectionX.tickPositionsMajor[i];
                double unitsFromAxisEdge = value - settings.axes.x.min;
                int xPx = (int)(unitsFromAxisEdge * settings.xAxisScale);
                settings.gfxData.DrawLine(pen, xPx, 0, xPx, settings.dataSize.Height);
            }

            for (int i = 0; i < settings.ticks.tickCollectionY.tickPositionsMajor.Length; i++)
            {
                double value = settings.ticks.tickCollectionY.tickPositionsMajor[i];
                double unitsFromAxisEdge = value - settings.axes.y.min;
                int yPx = settings.dataSize.Height - (int)(unitsFromAxisEdge * settings.yAxisScale);
                settings.gfxData.DrawLine(pen, 0, yPx, settings.dataSize.Width, yPx);
            }
        }

        public static void DataPlottables(Settings settings)
        {
            if (settings.gfxData == null)
                return;

            for (int i = 0; i < settings.plottables.Count; i++)
            {
                Plottable pltThing = settings.plottables[i];
                try
                {
                    pltThing.Render(settings);
                }
                catch (OverflowException)
                {
                    Debug.WriteLine($"OverflowException plotting: {pltThing}");
                }
            }
        }

        public static void CreateLegendBitmap(Settings settings)
        {
            LegendTools.DrawLegend(settings);
        }

        public static void PlaceLegendOntoFigure(Settings settings)
        {
            if (settings.gfxFigure == null || settings.gfxLegend == null)
                return;
            if (settings.legendLocation != ScottPlot.legendLocation.none)
            {
                Point legendLocation = new Point(settings.dataOrigin.X + settings.legendFrame.Location.X,
                settings.dataOrigin.Y + settings.legendFrame.Location.Y);
                settings.gfxFigure.DrawImage(settings.bmpLegend, legendLocation);
            }
        }

        public static void PlaceDataOntoFigure(Settings settings)
        {
            if (settings.gfxFigure == null || settings.bmpData == null)
                return;

            settings.gfxFigure.DrawImage(settings.bmpData, settings.dataOrigin);
        }

        public static void FigureLabels(Settings settings, bool drawDebugRectangles = false)
        {
            if (settings.gfxFigure == null)
                return;

            int dataCenterX = settings.dataSize.Width / 2 + settings.dataOrigin.X;
            int dataCenterY = settings.dataSize.Height / 2 + settings.dataOrigin.Y;

            // title
            Point titlePoint = new Point(dataCenterX - (int)settings.title.width / 2, settings.layout.padOnAllSides);
            settings.gfxFigure.DrawString(settings.title.text, settings.title.font, new SolidBrush(settings.title.color), titlePoint, settings.misc.sfNorthWest);
            if (drawDebugRectangles)
                settings.gfxFigure.DrawRectangle(Pens.Magenta, titlePoint.X, titlePoint.Y, (int)settings.title.width, (int)settings.title.height);

            // horizontal axis label
            Point xLabelPoint = new Point(dataCenterX - (int)settings.xLabel.width / 2, settings.figureSize.Height - settings.layout.padOnAllSides - (int)settings.xLabel.height);
            settings.gfxFigure.DrawString(settings.xLabel.text, settings.xLabel.font, new SolidBrush(settings.xLabel.color), xLabelPoint, settings.misc.sfNorthWest);
            if (drawDebugRectangles)
                settings.gfxFigure.DrawRectangle(Pens.Magenta, xLabelPoint.X, xLabelPoint.Y, (int)settings.xLabel.width, (int)settings.xLabel.height);

            // vertical axis label
            Point yLabelPoint = new Point(-dataCenterY - (int)settings.yLabel.width / 2, settings.layout.padOnAllSides);
            settings.gfxFigure.RotateTransform(-90);
            settings.gfxFigure.DrawString(settings.yLabel.text, settings.yLabel.font, new SolidBrush(settings.yLabel.color), yLabelPoint, settings.misc.sfNorthWest);
            if (drawDebugRectangles)
                settings.gfxFigure.DrawRectangle(Pens.Magenta, yLabelPoint.X, yLabelPoint.Y, (int)settings.yLabel.width, (int)settings.yLabel.height);
            settings.gfxFigure.ResetTransform();
        }

        public static void FigureTicks(Settings settings)
        {
            if (settings.dataSize.Width < 1 || settings.dataSize.Height < 1)
                return;

            settings.ticks.tickCollectionX = new TickCollection(settings, false, settings.ticks.tickDateTimeX);
            settings.ticks.tickCollectionY = new TickCollection(settings, true, settings.ticks.tickDateTimeY);

            RenderTicksOnLeft(settings);
            RenderTicksOnBottom(settings);
            RenderTickMultipliers(settings);
        }

        public static void FigureFrames(Settings settings)
        {
            if (settings.dataSize.Width < 1 || settings.dataSize.Height < 1)
                return;

            Point tl = new Point(settings.dataOrigin.X - 1, settings.dataOrigin.Y - 1);
            Point tr = new Point(settings.dataOrigin.X + settings.dataSize.Width, settings.dataOrigin.Y - 1);
            Point bl = new Point(settings.dataOrigin.X - 1, settings.dataOrigin.Y + settings.dataSize.Height);
            Point br = new Point(settings.dataOrigin.X + settings.dataSize.Width, settings.dataOrigin.Y + settings.dataSize.Height);

            if (settings.layout.displayAxisFrames)
            {
                Pen axisFramePen = new Pen(settings.ticks.tickColor);
                if (settings.layout.displayFrameByAxis[0])
                    settings.gfxFigure.DrawLine(axisFramePen, tl, bl);
                if (settings.layout.displayFrameByAxis[1])
                    settings.gfxFigure.DrawLine(axisFramePen, tr, br);
                if (settings.layout.displayFrameByAxis[2])
                    settings.gfxFigure.DrawLine(axisFramePen, bl, br);
                if (settings.layout.displayFrameByAxis[3])
                    settings.gfxFigure.DrawLine(axisFramePen, tl, tr);
            }
        }

        public static void Benchmark(Settings settings)
        {
            if (settings.benchmark.visible)
            {
                int debugPadding = 3;
                PointF textLocation = new PointF(settings.dataSize.Width + settings.dataOrigin.X, settings.dataSize.Height + settings.dataOrigin.Y);
                textLocation.X -= settings.benchmark.width + debugPadding;
                textLocation.Y -= settings.benchmark.height + debugPadding;
                RectangleF textRect = new RectangleF(textLocation, settings.benchmark.size);
                settings.gfxFigure.FillRectangle(new SolidBrush(settings.benchmark.colorBackground), textRect);
                settings.gfxFigure.DrawRectangle(new Pen(settings.benchmark.colorBorder), Rectangle.Round(textRect));
                settings.gfxFigure.DrawString(settings.benchmark.text, settings.benchmark.font, new SolidBrush(settings.benchmark.color), textLocation);
            }
        }

        public static void RenderTicksOnLeft(Settings settings)
        {
            if (!settings.ticks.displayTicksY)
                return;

            Pen pen = new Pen(settings.ticks.tickColor);
            Brush brush = new SolidBrush(settings.ticks.tickColor);

            for (int i = 0; i < settings.ticks.tickCollectionY.tickPositionsMajor.Length; i++)
            {
                double value = settings.ticks.tickCollectionY.tickPositionsMajor[i];
                string text = settings.ticks.tickCollectionY.tickLabels[i];

                double unitsFromAxisEdge = value - settings.axes.y.min;
                int xPx = settings.dataOrigin.X - 1;
                int yPx = (int)(unitsFromAxisEdge * settings.yAxisScale);
                yPx = settings.figureSize.Height - yPx - settings.layout.paddingBySide[2];

                settings.gfxFigure.DrawLine(pen, xPx, yPx, xPx - settings.ticks.tickSize, yPx);
                settings.gfxFigure.DrawString(text, settings.ticks.tickFont, brush, xPx - settings.ticks.tickSize, yPx, settings.misc.sfEast);
            }

            if (settings.ticks.displayTicksYminor && settings.ticks.tickCollectionY.tickPositionsMinor != null)
            {
                foreach (var value in settings.ticks.tickCollectionY.tickPositionsMinor)
                {
                    double unitsFromAxisEdge = value - settings.axes.y.min;
                    int xPx = settings.dataOrigin.X - 1;
                    int yPx = (int)(unitsFromAxisEdge * settings.yAxisScale);
                    yPx = settings.figureSize.Height - yPx - settings.layout.paddingBySide[2];
                    settings.gfxFigure.DrawLine(pen, xPx, yPx, xPx - settings.ticks.tickSize / 2, yPx);
                }
            }

        }

        public static void RenderTicksOnBottom(Settings settings)
        {
            if (!settings.ticks.displayTicksX)
                return;

            Pen pen = new Pen(settings.ticks.tickColor);
            Brush brush = new SolidBrush(settings.ticks.tickColor);

            for (int i = 0; i < settings.ticks.tickCollectionX.tickPositionsMajor.Length; i++)
            {
                double value = settings.ticks.tickCollectionX.tickPositionsMajor[i];
                string text = settings.ticks.tickCollectionX.tickLabels[i];

                double unitsFromAxisEdge = value - settings.axes.x.min;
                int xPx = (int)(unitsFromAxisEdge * settings.xAxisScale) + settings.layout.paddingBySide[0];
                int yPx = settings.figureSize.Height - settings.layout.paddingBySide[2];

                settings.gfxFigure.DrawLine(pen, xPx, yPx, xPx, yPx + settings.ticks.tickSize);
                settings.gfxFigure.DrawString(text, settings.ticks.tickFont, brush, xPx, yPx + settings.ticks.tickSize, settings.misc.sfNorth);
            }

            if (settings.ticks.displayTicksXminor && settings.ticks.tickCollectionX.tickPositionsMinor != null)
            {
                foreach (var value in settings.ticks.tickCollectionX.tickPositionsMinor)
                {
                    double unitsFromAxisEdge = value - settings.axes.x.min;
                    int xPx = (int)(unitsFromAxisEdge * settings.xAxisScale) + settings.layout.paddingBySide[0];
                    int yPx = settings.figureSize.Height - settings.layout.paddingBySide[2];
                    settings.gfxFigure.DrawLine(pen, xPx, yPx, xPx, yPx + settings.ticks.tickSize / 2);
                }
            }
        }

        private static void RenderTickMultipliers(Settings settings)
        {
            Brush brush = new SolidBrush(settings.ticks.tickColor);

            if ((settings.ticks.tickCollectionX.cornerLabel != "") && settings.ticks.displayTicksX)
            {
                SizeF multiplierLabelXsize = settings.gfxFigure.MeasureString(settings.ticks.tickCollectionX.cornerLabel, settings.ticks.tickFont);
                settings.gfxFigure.DrawString(settings.ticks.tickCollectionX.cornerLabel, settings.ticks.tickFont, brush,
                    settings.dataOrigin.X + settings.dataSize.Width,
                    settings.dataOrigin.Y + settings.dataSize.Height + multiplierLabelXsize.Height,
                    settings.misc.sfNorthEast);
            }

            if ((settings.ticks.tickCollectionY.cornerLabel != "") && settings.ticks.displayTicksY)
            {
                settings.gfxFigure.DrawString(settings.ticks.tickCollectionY.cornerLabel, settings.ticks.tickFont, brush,
                    settings.dataOrigin.X,
                    settings.dataOrigin.Y,
                    settings.misc.sfSouthWest);
            }
        }

        public static void MouseZoomRectangle(Settings settings)
        {
            if (!settings.mouseZoomRectangleIsHappening)
                return;

            int[] xs = new int[] { settings.mouseZoomDownLocation.X, settings.mouseZoomCurrentLocation.X };
            int[] ys = new int[] { settings.mouseZoomDownLocation.Y, settings.mouseZoomCurrentLocation.Y };
            Rectangle rect = new Rectangle(xs.Min(), ys.Min(), xs.Max() - xs.Min(), ys.Max() - ys.Min());
            rect.X -= settings.dataOrigin.X;
            rect.Y -= settings.dataOrigin.Y;

            Pen outline = new Pen(Color.FromArgb(100, 255, 0, 0));
            Brush fill = new SolidBrush(Color.FromArgb(50, 255, 0, 0));

            settings.gfxData.DrawRectangle(outline, rect);
            settings.gfxData.FillRectangle(fill, rect);
        }
    }
}
