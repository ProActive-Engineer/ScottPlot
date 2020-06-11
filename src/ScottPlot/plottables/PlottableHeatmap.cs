﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ScottPlot.Config;

namespace ScottPlot
{


    public class PlottableHeatmap : Plottable
    {
        private int width;
        private int height;
        private double[] intensitiesNormalized;
        private Config.ColorMaps.Colormaps colorMap;
        public string label;
        private double[] axisOffsets;
        private double[] axisMultipliers;
        private double? scaleMin;
        private double? scaleMax;

        private Bitmap bmp;
        private Bitmap scale;
        private double min;
        private double max;
        private SolidBrush brush;
        private Pen pen;

        public PlottableHeatmap(double[,] intensities, Config.ColorMaps.Colormaps colorMap, string label, double[] axisOffsets, double[] axisMultipliers, double? scaleMin, double? scaleMax)
        {
            this.width = intensities.GetLength(1);
            this.height = intensities.GetLength(0);
            double[] intensitiesFlattened = Flatten(intensities);
            this.min = intensitiesFlattened.Min();
            this.max = intensitiesFlattened.Max();
            this.brush = new SolidBrush(Color.Black);
            this.pen = new Pen(brush);
            this.axisOffsets = axisOffsets;
            this.axisMultipliers = axisMultipliers;
            this.colorMap = colorMap;
            this.label = label;
            this.scaleMin = scaleMin;
            this.scaleMax = scaleMax;


            this.intensitiesNormalized = Normalize(intensitiesFlattened, scaleMin, scaleMax);

            int[] flatARGB = IntensityToColor(this.intensitiesNormalized, colorMap);

            bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            scale = new Bitmap(1, 256, PixelFormat.Format32bppArgb);

            int[] scaleRGBA = IntensityToColor(Normalize(Enumerable.Range(0, scale.Height).Select(i => (double)i).Reverse().ToArray(), scaleMin, scaleMax), colorMap);

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            Rectangle rectScale = new Rectangle(0, 0, scale.Width, scale.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            BitmapData scaleBmpData = scale.LockBits(rectScale, ImageLockMode.ReadWrite, scale.PixelFormat);

            Marshal.Copy(flatARGB, 0, bmpData.Scan0, flatARGB.Length);
            Marshal.Copy(scaleRGBA, 0, scaleBmpData.Scan0, scaleRGBA.Length);
            bmp.UnlockBits(bmpData);
            scale.UnlockBits(scaleBmpData);
        }

        private double[] Normalize(double[] input, double? scaleMin = null, double? scaleMax = null)
        {
            double min = input.Min();
            double max = input.Max();

            if (scaleMin.HasValue && scaleMin.Value < min)
            {
                min = scaleMin.Value;
            }

            if (scaleMax.HasValue && scaleMax.Value > max)
            {
                max = scaleMax.Value;
            }

            double[] normalized = input.AsParallel().AsOrdered().Select(i => (i - min) / (max - min)).ToArray();
            if (scaleMin.HasValue)
            {
                double threshold = (scaleMin.Value - min) / (max - min);
                normalized = normalized.AsParallel().AsOrdered().Select(i => i < threshold ? threshold : i).ToArray();
            }

            if (scaleMax.HasValue)
            {
                double threshold = (scaleMax.Value - min) / (max - min);
                normalized = normalized.AsParallel().AsOrdered().Select(i => i > threshold ? threshold : i).ToArray();
            }

            return normalized;
        }

        private double[] Invert(double[] input)
        {
            return input.Select(i => 1 - i).ToArray();
        }

        private T[] Flatten<T>(T[,] toFlatten)
        {
            return toFlatten.Cast<T>().ToArray();
        }


        public override LegendItem[] GetLegendItems()
        {
            var singleLegendItem = new LegendItem(label, System.Drawing.Color.Gray, lineWidth: 10, markerShape: MarkerShape.none); //Colours in the legend is kinda difficult...
            return new LegendItem[] { singleLegendItem };
        }

        public override AxisLimits2D GetLimits()
        {
            return new AxisLimits2D(-10, bmp.Width, -5, bmp.Height);
        }

        public override int GetPointCount()
        {
            return intensitiesNormalized.Length;
        }

        private int[] IntensityToColor(double[] intensities, Config.ColorMaps.Colormaps colorMap)
        {
            switch (colorMap)
            {
                case Config.ColorMaps.Colormaps.grayscale:
                    return new Config.ColorMaps.Grayscale().IntensitiesToARGB(intensities);
                case Config.ColorMaps.Colormaps.grayscaleInverted:
                    return new Config.ColorMaps.GrayscaleInverted().IntensitiesToARGB(intensities);
                case Config.ColorMaps.Colormaps.viridis:
                    return new Config.ColorMaps.Viridis().IntensitiesToARGB(intensities);
                case Config.ColorMaps.Colormaps.magma:
                    return new Config.ColorMaps.Magma().IntensitiesToARGB(intensities);
                case Config.ColorMaps.Colormaps.inferno:
                    return new Config.ColorMaps.Inferno().IntensitiesToARGB(intensities);
                case Config.ColorMaps.Colormaps.plasma:
                    return new Config.ColorMaps.Plasma().IntensitiesToARGB(intensities);
                case Config.ColorMaps.Colormaps.turbo:
                    return new Config.ColorMaps.Turbo().IntensitiesToARGB(intensities);
                default:
                    throw new ArgumentException("Colormap not supported");
            }
        }

        public override void Render(Settings settings)
        {
            var interpMode = settings.gfxData.InterpolationMode;
            settings.gfxData.InterpolationMode = InterpolationMode.NearestNeighbor;
            double minScale = settings.xAxisScale < settings.yAxisScale ? settings.xAxisScale : settings.yAxisScale;
            settings.gfxData.DrawImage(bmp, (float)settings.GetPixelX(0), (float)(settings.GetPixelY(0) - (height * minScale)), (float)(width * minScale), (float)(height * minScale));
            RenderScale(settings);
            RenderAxis(settings, minScale);
            settings.gfxData.InterpolationMode = interpMode;
        }

        private void RenderScale(Settings settings)
        {
            Rectangle scaleRect = new Rectangle(settings.figureSize.Width - 150, settings.layout.xLabelHeight, 30, settings.layout.plot.Height - settings.layout.xLabelHeight - settings.layout.xScaleHeight - settings.layout.titleHeight);

            Rectangle scaleRectOutline = scaleRect;
            scaleRectOutline.Width /= 2;
            var interpMode = settings.gfxFigure.InterpolationMode;

            settings.gfxFigure.InterpolationMode = InterpolationMode.NearestNeighbor; //This is necessary for the scale (as its a 1 pixel wide image)
            settings.gfxFigure.DrawImage(scale, scaleRect);
            settings.gfxFigure.DrawRectangle(pen, scaleRectOutline);
            string maxString = scaleMax.HasValue ? $"{(scaleMax.Value < max ? "≥ " : "")}{ scaleMax.Value:f3}" : $"{max:f3}";
            string minString = scaleMin.HasValue ? $"{(scaleMin.Value > min ? "≤ " : "")}{scaleMin.Value:f3}" : $"{min:f3}";

            settings.gfxFigure.DrawString(maxString, new Font(FontFamily.GenericSansSerif, 12), brush, new Point(scaleRect.X + 30, scaleRect.Top));
            settings.gfxFigure.DrawString(minString, new Font(FontFamily.GenericSansSerif, 12), brush, new Point(scaleRect.X + 30, scaleRect.Bottom), new StringFormat() { LineAlignment = StringAlignment.Far });

            settings.gfxFigure.InterpolationMode = interpMode;
        }

        private void RenderAxis(Settings settings, double minScale)
        {
            StringFormat right_centre = new StringFormat() { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
            StringFormat centre_top = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
            Font axisFont = new Font(FontFamily.GenericSansSerif, 12);
            double offset = -2;

            settings.gfxData.DrawString($"{axisOffsets[0]:f3}", axisFont, brush, settings.GetPixel(0, offset), centre_top);
            settings.gfxData.DrawString($"{axisOffsets[0] + axisMultipliers[0]:f3}", axisFont, brush, new PointF((float)((width * minScale) + settings.GetPixelX(0)), (float)settings.GetPixelY(offset)), centre_top);

            settings.gfxData.DrawString($"{axisOffsets[1]:f3}", axisFont, brush, settings.GetPixel(offset, 0), right_centre);
            settings.gfxData.DrawString($"{axisOffsets[1] + axisMultipliers[1]:f3}", axisFont, brush, new PointF((float)settings.GetPixelX(offset), (float)(settings.GetPixelY(0) - (height * minScale))), right_centre);
        }
        public override string ToString()
        {
            string label = string.IsNullOrWhiteSpace(this.label) ? "" : $" ({this.label})";
            return $"PlottableHeatmap{label} with {GetPointCount()} points";
        }
    }
}
