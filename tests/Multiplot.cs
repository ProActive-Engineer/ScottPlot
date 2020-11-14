﻿using NUnit.Framework;
using ScottPlot;
using System;

namespace ScottPlotTests
{
    class Multiplot
    {
        private ScottPlot.MultiPlot SampleMultiPlot()
        {
            var multiplot = new ScottPlot.MultiPlot(width: 800, height: 600, rows: 2, cols: 2);

            // plot an increasng spread of data in each subplot
            Random rand = new Random(0);
            int pointCount = 100;
            for (int i = 0; i < multiplot.subplots.Length; i += 1)
            {
                double zoom = Math.Pow(i + 1, 2);
                multiplot.subplots[i].Title($"#{i}");
                multiplot.subplots[i].PlotScatter(
                        xs: ScottPlot.DataGen.Random(rand, pointCount, multiplier: zoom, offset: -.5 * zoom),
                        ys: ScottPlot.DataGen.Random(rand, pointCount, multiplier: zoom, offset: -.5 * zoom)
                    );
            }
            return multiplot;
        }

        private void DisplayAxisInfo(ScottPlot.MultiPlot multiplot)
        {
            for (int i = 0; i < multiplot.subplots.Length; i += 1)
                Console.WriteLine($"Subplot index {i} {multiplot.subplots[i].AxisLimits()}");
        }

        [Test]
        public void Test_MultiPlot_DefaultScales()
        {
            ScottPlot.MultiPlot multiplot = SampleMultiPlot();

            string name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string filePath = System.IO.Path.GetFullPath(name + ".png");
            multiplot.SaveFig(filePath);
            Console.WriteLine($"Saved {filePath}");

            DisplayAxisInfo(multiplot);
        }

        [Test]
        public void Test_MultiPlot_MatchAxis()
        {
            ScottPlot.MultiPlot multiplot = SampleMultiPlot();

            // update the lower left (index 2) plot to use the scale of the lower right (index 3)
            multiplot.subplots[2].MatchAxis(multiplot.subplots[3]);
            multiplot.subplots[2].Title("#2 (matched to #3)");

            string name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string filePath = System.IO.Path.GetFullPath(name + ".png");
            multiplot.SaveFig(filePath);
            Console.WriteLine($"Saved {filePath}");
            DisplayAxisInfo(multiplot);

            var matchedAxisLimits = multiplot.subplots[2].AxisLimits();
            Assert.Greater(matchedAxisLimits.xMax, matchedAxisLimits.xMin);
            Assert.Greater(matchedAxisLimits.yMax, matchedAxisLimits.yMin);
        }

        [Test]
        public void Test_MultiPlot_MatchJustOneAxis()
        {
            ScottPlot.MultiPlot multiplot = SampleMultiPlot();

            multiplot.subplots[1].MatchAxis(multiplot.subplots[3], horizontal: false);
            multiplot.subplots[1].MatchLayout(multiplot.subplots[3], horizontal: false);
            multiplot.subplots[1].Title("#1 (matched vertical to #3)");

            multiplot.subplots[2].MatchAxis(multiplot.subplots[3], vertical: false);
            multiplot.subplots[2].MatchLayout(multiplot.subplots[3], vertical: false);
            multiplot.subplots[2].Title("#2 (matched hoizontal to #3)");

            string name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string filePath = System.IO.Path.GetFullPath(name + ".png");
            multiplot.SaveFig(filePath);
            Console.WriteLine($"Saved {filePath}");

            DisplayAxisInfo(multiplot);
            var matchedVerticalLimits = multiplot.subplots[1].AxisLimits();
            var matchedHorizontalLimits = multiplot.subplots[1].AxisLimits();

            Assert.Greater(matchedVerticalLimits.xMax, matchedVerticalLimits.xMin);
            Assert.Greater(matchedVerticalLimits.yMax, matchedVerticalLimits.yMin);

            Assert.Greater(matchedHorizontalLimits.xMax, matchedHorizontalLimits.xMin);
            Assert.Greater(matchedHorizontalLimits.yMax, matchedHorizontalLimits.yMin);
        }
    }
}
