﻿using Eto.Forms;


namespace ScottPlot.Sandbox.Eto
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new Application(global::Eto.Platforms.WinForms).Run(new MainWindow());
        }
    }
}
