﻿using Eto.Forms;
using ScottPlot;
using ScottPlot.Plottables;

namespace ScottPlot.Sandbox.Eto
{
    partial class MainWindow : Form
    {
        private readonly DebugPoint DebugPoint = new();

        public MainWindow()
        {
            InitializeComponent();


            const int N = 51;

            etoPlot.Plot.Add(DebugPoint);
            etoPlot.Plot.AddScatter(Generate.Consecutive(N), Generate.Sin(N), Colors.Blue);
            etoPlot.Plot.AddScatter(Generate.Consecutive(N), Generate.Cos(N), Colors.Red);
            etoPlot.Plot.AddScatter(Generate.Consecutive(N), Generate.Sin(N, 0.5), Colors.Green);
            etoPlot.Refresh();

            etoPlot.MouseMove += EtoPlot_MouseMove;
        }

        private void EtoPlot_MouseMove(object? sender, MouseEventArgs e)
        {
            DebugPoint.Position = etoPlot.Backend.GetMouseCoordinates();
            etoPlot.Refresh();
        }
    }
}