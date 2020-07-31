﻿using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ScottPlot.Avalonia
{
    public class AvaPlotViewer : Window
    {
        [Obsolete("The zero parameter constructor is not to be called")]
        public AvaPlotViewer()
        {
            throw new NotSupportedException("The zero parameter constructor is not to be called");

            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public AvaPlotViewer(ScottPlot.Plot plot, int windowWidth = 600, int windowHeight = 400, string windowTitle = "ScottPlot Viewer")
        {
            this.InitializeComponent();

            Width = windowWidth;
            Height = windowHeight;
            Title = windowTitle;

            this.Find<AvaPlot>("avaPlot1").backend.Reset(plot);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
