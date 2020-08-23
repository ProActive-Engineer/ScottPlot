﻿using ScottPlot.Config;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ScottPlot.Renderable
{
    public class Legend : IRenderable
    {
        public Direction Location = Direction.SE;
        public float Padding = 5;

        public Color FillColor = Color.White;
        public Color OutlineColor = Color.Black;
        public Color ShadowColor = Color.FromArgb(50, Color.Black);
        public float ShadowOffsetX = 2;
        public float ShadowOffsetY = 2;

        public string FontName = Fonts.GetDefaultFontName();
        public float FontSize = 14;
        public Color FontColor = Color.Black;

        public Bitmap GetBitmap(Settings settings)
        {
            return null;
        }

        public void Render(Settings settings)
        {
            var items = GetLegendItems(settings);

            // symbol size based on font size
            float symbolWidth = 40 * FontSize / 12;
            float symbolPad = FontSize / 3;

            using (var fillBrush = new SolidBrush(FillColor))
            using (var shadowBrush = new SolidBrush(ShadowColor))
            using (var textBrush = new SolidBrush(FontColor))
            using (var outlinePen = new Pen(OutlineColor))
            using (var gfx = Graphics.FromImage(settings.bmpFigure))
            using (var font = new Font(FontName, FontSize, GraphicsUnit.Pixel))
            {
                // TODO: respect global anti-aliasing settings
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // determine maximum label size and use it to define legend size
                float maxLabelWidth = 0;
                float maxLabelHeight = 0;
                for (int i = 0; i < items.Length; i++)
                {
                    var labelSize = gfx.MeasureString(items[i].label, font);
                    maxLabelWidth = Math.Max(maxLabelWidth, labelSize.Width);
                    maxLabelHeight = Math.Max(maxLabelHeight, labelSize.Height);
                }
                float width = symbolWidth + maxLabelWidth + symbolPad;
                float height = maxLabelHeight * items.Length;

                // determine where to place the legend
                (float locationX, float locationY) = GetLocationPx(settings, width, height);

                RectangleF rectShadow = new RectangleF(locationX + ShadowOffsetX, locationY + ShadowOffsetY, width, height);
                gfx.FillRectangle(shadowBrush, rectShadow);

                RectangleF rectFill = new RectangleF(locationX, locationY, width, height);
                gfx.FillRectangle(fillBrush, rectFill);
                gfx.DrawRectangle(outlinePen, Rectangle.Round(rectFill));

                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    float verticalOffset = i * maxLabelHeight;

                    // draw text
                    gfx.DrawString(item.label, font, textBrush, locationX + symbolWidth, locationY + verticalOffset);

                    // draw line
                    outlinePen.Color = item.color;
                    outlinePen.Width = 1;
                    float lineY = locationY + verticalOffset + maxLabelHeight / 2;
                    float lineX1 = locationX + symbolPad;
                    float lineX2 = lineX1 + symbolWidth - symbolPad * 2;
                    gfx.DrawLine(outlinePen, lineX1, lineY, lineX2, lineY);

                    // draw marker
                    float lineXcenter = (lineX1 + lineX2) / 2;
                    float markerWidth = settings.legend.font.Size / 2;
                    PointF markerPoint = new PointF(lineXcenter, lineY);
                    MarkerTools.DrawMarker(gfx, markerPoint, item.markerShape, markerWidth, item.color);
                }
            }
        }

        public static LegendItem[] GetLegendItems(Settings settings)
        {
            var items = new List<LegendItem>();

            foreach (Plottable plottable in settings.plottables)
            {
                var legendItems = plottable.GetLegendItems();

                if (legendItems is null)
                    continue;

                foreach (var plottableItem in legendItems)
                    if (plottableItem.label != null)
                        items.Add(plottableItem);
            }

            if (settings.legend.reverseOrder)
                items.Reverse();

            return items.ToArray();
        }

        private (float x, float y) GetLocationPx(Settings settings, float width, float height)
        {
            float leftX = settings.dataOrigin.X + Padding;
            float rightX = settings.dataOrigin.X + settings.dataSize.Width - Padding - width;
            float centerX = settings.dataOrigin.X + settings.dataSize.Width / 2 - width / 2;

            float topY = settings.dataOrigin.Y + Padding;
            float bottomY = settings.dataOrigin.Y + settings.dataSize.Height - Padding - height;
            float centerY = settings.dataOrigin.Y + settings.dataSize.Height / 2 - height / 2;

            switch (Location)
            {
                case Direction.NW:
                    return (leftX, topY);
                case Direction.N:
                    return (centerX, topY);
                case Direction.NE:
                    return (rightX, topY);
                case Direction.E:
                    return (rightX, centerY);
                case Direction.SE:
                    return (rightX, bottomY);
                case Direction.S:
                    return (centerX, bottomY);
                case Direction.SW:
                    return (leftX, bottomY);
                case Direction.W:
                    return (leftX, centerY);
                case Direction.C:
                    return (centerX, centerY);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
