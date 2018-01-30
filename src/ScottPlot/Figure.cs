﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

/* ScottPlot is a class library intended to make it easy to graph large datasets in high speed.
 * 
 * Although features like mouse click-and-drag to zoom and pan are included for easy interactive GUI
 * integration, ScottPlot can be run entirely within console applications as well.
 * 
 * KEY TERMS:
 *      Figure - a Figure object is mostly what the user will interact with. It contains a Frame and a Graph.
 *      Frame - the frame is everything behind the data (the axis labels, grid lines, tick marks, etc).
 *      Graph - the part of the frame which gets drawn on when graphs are plotted.
 *      Axis - information about a single dimension (X vs Y) including the current min/max and pixel scaling.
 *  
 * THEORY OF OPERATION / USE OVERVIEW:
 *      * Create a Figure (telling it the size of the image)
 *      * Resize() can change the size in the future
 *      * Set colors as desired
 *      * Set the axis labels and title as desired
 *      * Zoom() and Pan() can be used to adjust window
 *      * Adjust the axis limits to window the data you wish to show
 *      * RedrawFrame() and now you are ready to add data
 *          * ClearGraph() to start a new data plot (erasing the last one)
 *          * Plot() methods accumulate drawings on the plot
 *      * Access the assembled image at any time with Render()
 *
 */
namespace ScottPlot
{
    
    // contains the frame and the graph!
    public class Figure
    {
        private Point graphPos = new Point(0, 0);

        private Bitmap bmpFrame;
        private Graphics gfxFrame;
        
        public Bitmap bmpGraph { get; set; }
        public Graphics gfxGraph;
        public Axis xAxis = new Axis(-10, 10, 100, false);
        public Axis yAxis = new Axis(-10, 10, 100, true);

        public Color colorBg;
        public Color colorAxis;
        public Color colorGrid;
        public Color colorGraph;

        // the user can set these
        const string font = "Arial";
        Font fontTicks = new Font(font, 9, FontStyle.Regular);
        Font fontTitle = new Font(font, 20, FontStyle.Bold);
        Font fontAxis = new Font(font, 12, FontStyle.Bold);

        public string yLabel = "";
        public string xLabel = "";
        public string title = "";

        public int padL = 50, padT = 47, padR = 50, padB = 47;

        public System.Diagnostics.Stopwatch stopwatch;

        // A figure object contains what's needed to draw scale bars and axis labels around a graph.
        // The graph itself is its own object which lives inside the figure.
        public Figure(int width, int height)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            stopwatch.Stop();
            stopwatch.Reset();

            styleWeb();
            Resize(width, height);

            // default to anti-aliasing on
            gfxGraph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gfxFrame.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            RedrawFrame();
            ClearGraph();
        }
        
        /// <summary>
        /// Resize the entire figure (in pixels)
        /// </summary>
        public void Resize(int width, int height)
        {
            // figure resized, so resize the frame bitmap
            bmpFrame = new Bitmap(width, height);
            gfxFrame = Graphics.FromImage(bmpFrame);
            
            // now re-calculate the graph size based on the padding
            Pad(null, null, null, null);

            // now resize the graph bitmap
            bmpGraph = new Bitmap(bmpFrame.Width - padL - padR, bmpFrame.Height - padT - padB);
            gfxGraph = Graphics.FromImage(bmpGraph);

            // now resize axis to the new pad dimensions
            xAxis.ResizePx(bmpGraph.Width);
            yAxis.ResizePx(bmpGraph.Height);
        }

        /// <summary>
        /// Change the padding between the edge of the graph and edge of the figure
        /// </summary>
        void Pad(int? left, int? right, int? top, int? bottom)
        {
            if (left != null) padL = (int)left;
            if (right != null) padR = (int)right;
            if (top != null) padT = (int)top;
            if (bottom != null) padB = (int)bottom;
            graphPos = new Point(padL, padT);
        }

        /// <summary>
        /// Clear the frame and redraw it from scratch.
        /// </summary>
        public void RedrawFrame()
        {
            
            gfxFrame.Clear(colorBg);

            // prepare things useful for drawing
            Pen penAxis = new Pen(new SolidBrush(colorAxis));
            Pen penGrid = new Pen(colorGrid) { DashPattern = new float[] { 4, 4 } };
            Brush brush = new SolidBrush(colorAxis);
            StringFormat sfCenter = new StringFormat();
            sfCenter.Alignment = StringAlignment.Center;
            StringFormat sfRight = new StringFormat();
            sfRight.Alignment = StringAlignment.Far;
            int posB = bmpGraph.Height + padT;
            int posCx = bmpGraph.Width / 2 + padL;
            int posCy = bmpGraph.Height / 2 + padT;

            int tick_size_minor = 2;
            int tick_size_major = 5;

            // draw the data rectangle and ticks
            gfxFrame.DrawRectangle(penAxis, graphPos.X - 1, graphPos.Y - 1, bmpGraph.Width + 1, bmpGraph.Height + 1);
            gfxFrame.FillRectangle(new SolidBrush(colorGraph), graphPos.X, graphPos.Y, bmpGraph.Width, bmpGraph.Height);
            foreach (Tick tick in xAxis.minorTicks)
                gfxFrame.DrawLine(penAxis, new Point(padL + tick.pixel, posB + 1), new Point(padL + tick.pixel, posB + 1 + tick_size_minor));
            foreach (Tick tick in yAxis.minorTicks)
                gfxFrame.DrawLine(penAxis, new Point(padL - 1, padT + tick.pixel), new Point(padL - 1 - tick_size_minor, padT + tick.pixel));
            foreach (Tick tick in xAxis.majorTicks)
            {
                gfxFrame.DrawLine(penGrid, new Point(padL + tick.pixel, padT), new Point(padL + tick.pixel, padT + bmpGraph.Height - 1));
                gfxFrame.DrawLine(penAxis, new Point(padL + tick.pixel, posB + 1), new Point(padL + tick.pixel, posB + 1 + tick_size_major));
                gfxFrame.DrawString(tick.label, fontTicks, brush, new Point(tick.pixel + padL, posB + 7), sfCenter);
            }
            foreach (Tick tick in yAxis.majorTicks)
            {
                gfxFrame.DrawLine(penGrid, new Point(padL, padT + tick.pixel), new Point(padL + bmpGraph.Width, padT + tick.pixel));
                gfxFrame.DrawLine(penAxis, new Point(padL - 1, padT + tick.pixel), new Point(padL - 1 - tick_size_major, padT + tick.pixel));
                gfxFrame.DrawString(tick.label, fontTicks, brush, new Point(padL-6, tick.pixel + padT-7), sfRight);
            }

            // draw labels
            gfxFrame.DrawString(xLabel, fontAxis, brush, new Point(posCx, posB+24), sfCenter);
            gfxFrame.DrawString(title, fontTitle, brush, new Point(bmpFrame.Width/2,8), sfCenter);
            gfxFrame.TranslateTransform(gfxFrame.VisibleClipBounds.Size.Width, 0);
            gfxFrame.RotateTransform(-90);
            gfxFrame.DrawString(yLabel, fontAxis, brush, new Point(-posCy, -bmpFrame.Width+2), sfCenter);
            gfxFrame.ResetTransform();

            // now that the frame is re-drawn, reset the graph
            ClearGraph();
        }

        /// <summary>
        /// Copy the empty graph area from the frame onto the graph object
        /// </summary>
        public void ClearGraph()
        {
            gfxGraph.DrawImage(bmpFrame, new Point(-padL, -padT));
        }
        
        public string RenderTimeMessage()
        {
            double ms = this.stopwatch.ElapsedTicks * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
            double hz = 1.0 / ms * 1000.0;
            return string.Format("{0:0.00 ms} {1:0.00 Hz}", ms, hz);
        }

        /// <summary>
        /// Return a merged bitmap of the frame with the graph added into it
        /// </summary>
        public Bitmap Render()
        {
            Bitmap bmpMerged = new Bitmap(bmpFrame);
            Graphics gfx = Graphics.FromImage(bmpMerged);
            gfx.DrawImage(bmpGraph, graphPos);

            // draw stamp message
            if (this.stopwatch.ElapsedTicks>0)
            {
                Font fontStamp = new Font(font, 8, FontStyle.Regular);
                //SolidBrush brushStamp = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
                SolidBrush brushStamp = new SolidBrush(colorAxis);
                Point pointStamp = new Point(bmpFrame.Width - padR - 2, bmpFrame.Height - padB - 14);
                StringFormat sfRight = new StringFormat();
                sfRight.Alignment = StringAlignment.Far;
                gfx.DrawString(RenderTimeMessage(), fontStamp, brushStamp, pointStamp, sfRight);

            }

            return bmpMerged;
        }

        /// <summary>
        /// Saves the figure in the format based on its extension.
        /// </summary>
        public void Save(string filename)
        {
            string basename = System.IO.Path.GetFileNameWithoutExtension(filename);
            string extension = System.IO.Path.GetExtension(filename).ToLower();

            switch (extension)
            {
                case ".png":
                    Render().Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                    Console.WriteLine("saved as PNG");
                    break;
                case ".jpg":
                    Render().Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                    Console.WriteLine("saved as JPG");
                    break;
                case ".bmp":
                    Render().Save(filename);
                    Console.WriteLine("saved as BMP");
                    break;
                default:
                    Console.WriteLine("format not supported!");
                    break;
            }
        }

        public void Axis(double? x1, double? x2, double? y1, double? y2)
        {
            if (x1 != null) xAxis.min = (double)x1;
            if (x2 != null) xAxis.max = (double)x2;
            if (y1 != null) yAxis.min = (double)y1;
            if (y2 != null) yAxis.max = (double)y2;
            if (x1 != null || x2 != null) xAxis.RecalculateScale();
            if (y1 != null || y2 != null) yAxis.RecalculateScale();
            if (x1 != null || x2 != null || y1 != null || y2 != null) RedrawFrame();
        }

        /// <summary>
        /// Zoom in on the center of Axis by a fraction. 
        /// A fraction of 2 means that the new width will be 1/2 as wide as the old width.
        /// A fraction of 0.1 means the new width will show 10 times more axis length.
        /// </summary>
        public void Zoom(double? xFrac, double? yFrac)
        {
            if (xFrac!=null) xAxis.Zoom((double)xFrac);
            if (yFrac!=null) yAxis.Zoom((double)yFrac);
            RedrawFrame();
        }


        public void styleWeb()
        {
            colorBg = Color.White;
            colorGraph = Color.FromArgb(255, 235, 235, 235);
            colorAxis = Color.Black;
            colorGrid = Color.LightGray;
        }

        public void styleForm()
        {
            colorBg = SystemColors.Control;
            colorGraph = Color.White;
            colorAxis = Color.Black;
            colorGrid = Color.LightGray;
        }












        /// <summary>
        /// Call this before graphing to start a stopwatch. 
        /// Render time will be displayed when the output graph is rendered.
        /// </summary>
        public void BenchmarkThis()
        {
            stopwatch.Restart();
        }

        private Point[] PointsFromArrays(double[] Xs, double[] Ys)
        {
            int pointCount = Math.Min(Xs.Length, Ys.Length);
            Point[] points = new Point[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                points[i] = new Point(xAxis.UnitToPx(Xs[i]), yAxis.UnitToPx(Ys[i]));
            }
            return points;
        }
        
        public void ResizeToData(double[] Xs, double[] Ys, double? zoomX, double? zoomY)
        {
            Axis(Xs.Min(), Xs.Max(), Ys.Min(), Ys.Max());
            Zoom(zoomX, zoomY);
        }

        public void PlotLines(double[] Xs, double[] Ys, float lineWidth, Color lineColor)
        {
            Point[] points = PointsFromArrays(Xs, Ys);
            Pen penLine = new Pen(new SolidBrush(lineColor), lineWidth);
            gfxGraph.DrawLines(penLine, points);
        }

        public void PlotScatter(double[] Xs, double[] Ys, float markerSize, Color markerColor)
        {
            Point[] points = PointsFromArrays(Xs, Ys);
            for (int i=0; i<points.Length; i++)
            {
                gfxGraph.FillEllipse(new SolidBrush(markerColor), 
                                     points[i].X - markerSize / 2, 
                                     points[i].Y - markerSize / 2, 
                                     markerSize, markerSize);
            }
            

        }

    }
}
