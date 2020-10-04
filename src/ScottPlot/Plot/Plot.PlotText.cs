﻿/* Code here extends Plot module with methods to construct plottables.
 *   - Plottables created here are added to the plottables list and returned.
 *   - Long lists of optional arguments (matplotlib style) are permitted.
 *   - Use one line per argument to simplify the tracking of changes.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ScottPlot
{
    public partial class Plot
    {
        public PlottableText PlotText(
            string text,
            double x,
            double y,
            Color? color = null,
            string fontName = null,
            double fontSize = 12,
            bool bold = false,
            string label = null,
            TextAlignment alignment = TextAlignment.middleLeft,
            double rotation = 0,
            bool frame = false,
            Color? frameColor = null
            )
        {
            if (color == null)
                color = settings.GetNextColor();

            if (fontName == null)
                fontName = Config.Fonts.GetDefaultFontName();

            if (frameColor == null)
                frameColor = Color.White;

            fontName = Config.Fonts.GetValidFontName(fontName);

            PlottableText plottableText = new PlottableText(
                text: text,
                x: x,
                y: y,
                color: (Color)color,
                fontName: fontName,
                fontSize: fontSize,
                bold: bold,
                label: label,
                alignment: alignment,
                rotation: rotation,
                frame: frame,
                frameColor: (Color)frameColor
                );

            Add(plottableText);
            return plottableText;
        }

        public PlottableAnnotation PlotAnnotation(
            string label,
            double xPixel = 10,
            double yPixel = 10,
            double fontSize = 12,
            string fontName = "Segoe UI",
            Color? fontColor = null,
            double fontAlpha = 1,
            bool fill = true,
            Color? fillColor = null,
            double fillAlpha = .2,
            double lineWidth = 1,
            Color? lineColor = null,
            double lineAlpha = 1,
            bool shadow = false
            )
        {

            fontColor = (fontColor is null) ? Color.Black : fontColor.Value;
            fillColor = (fillColor is null) ? Color.Yellow : fillColor.Value;
            lineColor = (lineColor is null) ? Color.Black : lineColor.Value;

            fontColor = Color.FromArgb((int)(255 * fontAlpha), fontColor.Value.R, fontColor.Value.G, fontColor.Value.B);
            fillColor = Color.FromArgb((int)(255 * fillAlpha), fillColor.Value.R, fillColor.Value.G, fillColor.Value.B);
            lineColor = Color.FromArgb((int)(255 * lineAlpha), lineColor.Value.R, lineColor.Value.G, lineColor.Value.B);

            var plottable = new PlottableAnnotation(
                    xPixel: xPixel,
                    yPixel: yPixel,
                    label: label,
                    fontSize: fontSize,
                    fontName: fontName,
                    fontColor: fontColor.Value,
                    fill: fill,
                    fillColor: fillColor.Value,
                    lineWidth: lineWidth,
                    lineColor: lineColor.Value,
                    shadow: shadow
                );

            Add(plottable);
            return plottable;
        }

    }
}
