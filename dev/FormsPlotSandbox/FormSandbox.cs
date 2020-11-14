﻿using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormsPlotSandbox
{
    public partial class FormSandbox : Form
    {
        public FormSandbox()
        {
            InitializeComponent();
        }

        private void FormSandbox_Load(object sender, EventArgs e)
        {
            Random rand = new Random(0);
            var plt1 = formsPlot1.plt.PlotSignal(new double[] { 1, 42, 23, 51}, sampleRate: .1);

            var plt2 = formsPlot1.plt.PlotSignal(new double[] { 54, 123, 72, 543 }, sampleRate: .5);
            plt2.HorizontalAxisIndex = 1;
            plt2.VerticalAxisIndex = 1;

            // TODO: figure out a simpler way to expose these objects
            formsPlot1.plt.GetSettings(false).YAxis.Title.Label = "Primary Vertical Axis";
            formsPlot1.plt.GetSettings(false).YAxis.Title.IsVisible = true;
            formsPlot1.plt.GetSettings(false).YAxis.Ticks.MajorTickEnable = true;
            formsPlot1.plt.GetSettings(false).YAxis.Ticks.MinorTickEnable = true;
            formsPlot1.plt.GetSettings(false).YAxis.Ticks.MajorLabelEnable = true;
            formsPlot1.plt.GetSettings(false).YAxis.Configure(color: plt1.color);

            formsPlot1.plt.GetSettings(false).YAxis2.Title.Label = "Secondary Vertical Axis";
            formsPlot1.plt.GetSettings(false).YAxis2.Title.IsVisible = true;
            formsPlot1.plt.GetSettings(false).YAxis2.Ticks.MajorTickEnable = true;
            formsPlot1.plt.GetSettings(false).YAxis2.Ticks.MinorTickEnable = true;
            formsPlot1.plt.GetSettings(false).YAxis2.Ticks.MajorLabelEnable = true;
            formsPlot1.plt.GetSettings(false).YAxis2.Configure(color: plt2.color);

            formsPlot1.plt.GetSettings(false).XAxis.Title.Label = "Primary Horizontal Axis";
            formsPlot1.plt.GetSettings(false).XAxis.Ticks.MajorTickEnable = true;
            formsPlot1.plt.GetSettings(false).XAxis.Ticks.MinorTickEnable = true;
            formsPlot1.plt.GetSettings(false).XAxis.Ticks.MajorLabelEnable = true;
            formsPlot1.plt.GetSettings(false).XAxis.Configure(color: plt1.color);

            formsPlot1.plt.GetSettings(false).XAxis2.Title.Label = "Secondary Horizontal Axis";
            formsPlot1.plt.GetSettings(false).XAxis2.Title.Font.Bold = false;
            formsPlot1.plt.GetSettings(false).XAxis2.Ticks.MajorTickEnable = true;
            formsPlot1.plt.GetSettings(false).XAxis2.Ticks.MinorTickEnable = true;
            formsPlot1.plt.GetSettings(false).XAxis2.Ticks.MajorLabelEnable = true;
            formsPlot1.plt.GetSettings(false).XAxis2.Configure(color: plt2.color);

            //formsPlot1.plt.Axis(-10, 110, -5, 25, 0, 0);
            //formsPlot1.plt.Axis(-1, 11, -100, 100, 1, 1);

            formsPlot1.Render();
        }
    }
}
