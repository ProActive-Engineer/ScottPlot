﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ScottPlot.Demo.PlotTypes
{
    class AxisLine
    {
        public class Quickstart : IPlotDemo
        {
            public string name { get; } = "Axis Line Quickstart";
            public string description { get; } = "Horizontal and vertical lines can be placed using HLine() and VLine(). Styling can be customized using arguments.";

            public void Render(Plot plt)
            {
                GenericPlots.SinAndCos(plt);

                plt.PlotHLine(y: .85, label: "HLine");
                plt.PlotVLine(x: 23, label: "VLine");
                plt.PlotVLine(x: 33, label: "VLine too", color: Color.Magenta, lineWidth: 3, lineStyle: LineStyle.Dot);
                plt.Legend();
            }
        }

        public class Draggable : IPlotDemo
        {
            public string name { get; } = "Draggable Axis Lines";
            public string description { get; } = "Use arguments to enable draggable lines (with optional limits).";

            public void Render(Plot plt)
            {
                GenericPlots.SinAndCos(plt);

                plt.PlotHLine(y: .85, draggable: true, dragLimitLower: -1, dragLimitUpper: +1);
                plt.PlotVLine(x: 23, draggable: true, dragLimitLower: 0, dragLimitUpper: 50);
            }
        }
    }
}
