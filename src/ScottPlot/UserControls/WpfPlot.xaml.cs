﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ScottPlot
{
    /// <summary>
    /// Interaction logic for ScottPlotWPF.xaml
    /// </summary>
    public partial class WpfPlot : UserControl
    {
        public Plot plt = new Plot();

        private DispatcherTimer timer;

        private bool currentlyRendering = false;

        public WpfPlot()
        {
            InitializeComponent();
            timer = new DispatcherTimer(); // WPF supports only DispatcherTimer
            timer.Tick += (o, arg) =>
            {
                timer.Stop(); // AutoReset = false
                Render(skipIfCurrentlyRendering: false);
            };
            if (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv")
                Tools.DesignerModeDemoPlot(plt);
            CanvasPlot_SizeChanged(null, null);
        }

        public void Render(bool skipIfCurrentlyRendering = false, bool lowQuality = false)
        {
            if (!(skipIfCurrentlyRendering && currentlyRendering))
            {
                if (timer.IsEnabled)
                    timer.Stop();
                currentlyRendering = true;
                imagePlot.Source = Tools.bmpImageFromBmp(plt.GetBitmap(true, lowQuality));
                currentlyRendering = false;
            }
        }

        private void CanvasPlot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double dpiScaleX = plt.GetSettings().gfxFigure.DpiX / 96;
            double dpiScaleY = plt.GetSettings().gfxFigure.DpiY / 96;
            plt.Resize(
                width: (int)(canvasPlot.ActualWidth * dpiScaleX),
                height: (int)(canvasPlot.ActualHeight * dpiScaleY)
                );
            Render(skipIfCurrentlyRendering: false);
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            plt.mouseTracker.MouseDown(e.GetPosition(this));
            CaptureMouse();
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            plt.mouseTracker.MouseMove(e.GetPosition(this));
            if ((Mouse.LeftButton == MouseButtonState.Pressed) || (Mouse.RightButton == MouseButtonState.Pressed))
                Render(skipIfCurrentlyRendering: true, plt.mouseTracker.lowQualityWhileInteracting);
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            plt.mouseTracker.MouseUp(e.GetPosition(this));
            if (plt.mouseTracker.lowQualityWhileInteracting && plt.mouseTracker.mouseUpHQRenderDelay > 0)
            {
                Render(false, true);
                timer.Interval = TimeSpan.FromMilliseconds(plt.mouseTracker.mouseUpHQRenderDelay);
                timer.Start();
            }
            else
                Render(skipIfCurrentlyRendering: false);
            ReleaseMouseCapture();
        }
    }
}
