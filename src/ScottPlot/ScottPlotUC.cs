﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScottPlot
{
    public partial class ScottPlotUC : UserControl
    {

        public Figure fig = new ScottPlot.Figure(123, 123);

        public class SignalData
        {
            public double[] values;
            public double sampleRate;
            public double xSpacing;
            public double offsetX;
            public double offsetY;
            public float lineWidth;
            public Color lineColor;
            public string label;
            
            public SignalData(double[] values, double sampleRate, double offsetX = 0, double offsetY = 0, Color? lineColor = null, float lineWidth=1, string label = null)
            {
                this.values = values;
                this.sampleRate = sampleRate;
                this.xSpacing = 1.0 / sampleRate;
                this.offsetX = offsetX;
                this.offsetY = offsetY;
                if (lineColor == null) lineColor = Color.Red;
                this.lineColor = (Color)lineColor;
                this.lineWidth = lineWidth;
                this.label = label;
            }
        }

        public class XYData
        {
            public double[] Xs;
            public double[] Ys;
            public float lineWidth;
            public Color lineColor;
            public float markerSize;
            public Color markerColor;
            public string label;

            public XYData(double[] Xs, double[] Ys, float lineWidth = 1, Color? lineColor = null, float markerSize=3, Color? markerColor=null, string label = null)
            {
                this.Xs = Xs;
                this.Ys = Ys;
                this.lineWidth = lineWidth;
                this.markerSize = markerSize;
                this.label = label;
                if (lineColor == null) lineColor = Color.Red;
                this.lineColor = (Color)lineColor;
                if (markerColor == null) markerColor = Color.Red;
                this.markerColor = (Color)markerColor;
            }
        }

        public List<SignalData> signalDataList = new List<SignalData>();
        public List<XYData> xyDataList = new List<XYData>();
        
        public void PlotXY(double[] Xs, double[] Ys, Color? color = null)
        {
            xyDataList.Add(new XYData(Xs, Ys, lineColor: color, markerColor: color));
            fig.ClearGraph();
            Redraw();
        }

        public void PlotSignal(double[] values, double sampleRate, Color? color = null, double offsetX = 0, double offsetY = 0)
        {
            signalDataList.Add(new SignalData(values, sampleRate, lineColor: color, offsetX: offsetX, offsetY: offsetY));
            fig.ClearGraph();
            Redraw();
        }

        public void Clear()
        {
            xyDataList.Clear();
            signalDataList.Clear();
        }

        public ScottPlotUC()
        {
            InitializeComponent();

            // add a mousewheel scroll handler
            pictureBox1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseWheel);

            // style the plot area
            fig.styleForm();
            fig.Zoom(.8, .8);
            fig.title = "ScottPlot User Control";
        }

        public void AxisSetToData()
        {
            double x1 = 0, x2 = 0, y1 = 0, y2 = 0;

            foreach (XYData xyData in xyDataList)
            {
                if (x1 == x2)
                {
                    // this is the first data we are scaling to, so just copy its bounds
                    x1 = xyData.Xs.Min();
                    x2 = xyData.Xs.Max();
                    y1 = xyData.Ys.Min();
                    y2 = xyData.Ys.Max();
                } else
                {
                    // we've seen some data before, so only take it if it expands the axes
                    x1 = Math.Min(x1, xyData.Xs.Min());
                    x2 = Math.Max(x2, xyData.Xs.Max());
                    y1 = Math.Min(y1, xyData.Ys.Min());
                    y2 = Math.Max(y2, xyData.Ys.Max());
                }
            }
            foreach (SignalData signalData in signalDataList)
            {
                if (x1 == x2)
                {
                    // this is the first data we are scaling to, so just copy its bounds
                    x1 = signalData.offsetX;
                    x2 = signalData.offsetX + signalData.values.Length * signalData.xSpacing;
                    y1 = signalData.values.Min() + signalData.offsetY;
                    y2 = signalData.values.Max() + signalData.offsetY;
                }
                else
                {
                    // we've seen some data before, so only take it if it expands the axes
                    x1 = Math.Min(x1, signalData.offsetX);
                    x2 = Math.Max(x2, signalData.offsetX + signalData.values.Length * signalData.xSpacing);
                    y1 = Math.Min(y1, signalData.values.Min() + signalData.offsetY);
                    y2 = Math.Max(y2, signalData.values.Max() + signalData.offsetY);
                }
            }
            fig.Axis(x1, x2, y1, y2);
            fig.Zoom(null, .9);
            Redraw(true);
        }

        public void UpdateSize()
        {
            fig.Resize(pictureBox1.Width, pictureBox1.Height);
            Redraw(true);
        }

        public void Redraw(bool redrawFrameToo=false)
        {
            fig.Benchmark(showBenchmark);
            if (redrawFrameToo) fig.RedrawFrame();
            else fig.ClearGraph();

            // plot XY points
            foreach (XYData xyData in xyDataList)
            {
                fig.PlotLines(xyData.Xs, xyData.Ys, xyData.lineWidth, xyData.lineColor);
                fig.PlotScatter(xyData.Xs, xyData.Ys, xyData.markerSize, xyData.markerColor);
            }

            // plot signals
            foreach (SignalData signalData in signalDataList)
            {
                fig.PlotSignal(signalData.values, signalData.xSpacing, signalData.offsetX, signalData.offsetY, signalData.lineWidth, signalData.lineColor);
            }             
            
            pictureBox1.Image = fig.Render();
        }





        /* mouse interaction */
        
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) fig.MousePanStart(e.X, e.Y); // left-click-drag pans
            else if (e.Button == MouseButtons.Right) fig.MouseZoomStart(e.X, e.Y); // right-click-drag zooms
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) fig.MousePanEnd();
            else if (e.Button == MouseButtons.Right) fig.MouseZoomEnd();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle) AxisSetToData(); // middle click to reset view
        }

        private void pictureBox1_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            double mag = 1.2;
            if (e.Delta>0) fig.Zoom(mag, mag);
            else fig.Zoom(1.0 / mag, 1.0 / mag);
            Redraw();
        }

        public bool showBenchmark = false;
        private void pictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.showBenchmark = !this.showBenchmark; // double-click graph to display benchmark stats
            Redraw();
        }

        public bool busyDrawingPlot = false;
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (fig.MouseIsDragging() && busyDrawingPlot == false)
            {
                fig.MouseMove(e.X, e.Y);
                busyDrawingPlot = true;
                UpdateSize();
                Application.DoEvents();
                busyDrawingPlot = false;
            }
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            UpdateSize();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
