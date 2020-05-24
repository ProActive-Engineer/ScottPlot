﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScottPlot.Demo.WinForms.WinFormsDemos
{
    public partial class ShowValueOnHover2 : Form
    {
        public ShowValueOnHover2()
        {
            InitializeComponent();
        }

        ScottPlot.PlottableScatterHighlight sph;
        private void ShowValueOnHover2_Load(object sender, EventArgs e)
        {
            int pointCount = 100;
            Random rand = new Random(0);
            double[] xs = DataGen.Consecutive(pointCount, 0.1);
            double[] ys = DataGen.NoisySin(rand, pointCount);

            sph = formsPlot1.plt.PlotScatterHighlight(xs, ys);
        }

        private void formsPlot1_MouseMove(object sender, MouseEventArgs e)
        {
            double mouseX = formsPlot1.plt.CoordinateFromPixelX(e.Location.X);
            double mouseY = formsPlot1.plt.CoordinateFromPixelY(e.Location.Y);

            var (closestX, closestY) = sph.GetPointNearest(mouseX, mouseY);

            label1.Text = $"Closest point to ({mouseX:N2}, {mouseY:N2}) " +
                $"is index {-1} ({closestX:N2}, {closestY:N2})";

            sph.HighlightClear();
            sph.HighlightPointNearest(mouseX, mouseY);
            formsPlot1.Render(skipIfCurrentlyRendering: true);
        }
    }
}
