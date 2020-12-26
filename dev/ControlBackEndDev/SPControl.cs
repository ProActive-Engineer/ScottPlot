﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace ControlBackEndDev
{
    public partial class SPControl : UserControl
    {
        public readonly ControlBackEnd CBE;
        public ScottPlot.Plot Plot => CBE.Plot;
        private readonly Dictionary<ScottPlot.Cursor, Cursor> Cursors;
        public ContextMenuStrip RightClickMenu;

        public SPControl()
        {
            InitializeComponent();
            RightClickMenu = DefaultRightClickMenu;
            Cursors = new Dictionary<ScottPlot.Cursor, Cursor>()
            {
                [ScottPlot.Cursor.Arrow] = System.Windows.Forms.Cursors.Arrow,
                [ScottPlot.Cursor.WE] = System.Windows.Forms.Cursors.SizeWE,
                [ScottPlot.Cursor.NS] = System.Windows.Forms.Cursors.SizeNS,
                [ScottPlot.Cursor.All] = System.Windows.Forms.Cursors.SizeAll,
                [ScottPlot.Cursor.Crosshair] = System.Windows.Forms.Cursors.Cross,
                [ScottPlot.Cursor.Hand] = System.Windows.Forms.Cursors.Hand,
                [ScottPlot.Cursor.Question] = System.Windows.Forms.Cursors.Help,
            };

            CBE = new ControlBackEnd(Width, Height);
            CBE.BitmapChanged += new EventHandler(OnBitmapChanged);
            CBE.BitmapUpdated += new EventHandler(OnBitmapUpdated);
            CBE.CursorChanged += new EventHandler(OnCursorChanged);
            CBE.RightClicked += new EventHandler(OnRightClicked);
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
        }

        public void Render(bool lowQuality = false) => CBE.Render(lowQuality);

        private void OnBitmapUpdated(object sender, EventArgs e) => pictureBox1.Invalidate();
        private void OnBitmapChanged(object sender, EventArgs e) => pictureBox1.Image = CBE.GetLatestBitmap();
        private void OnCursorChanged(object sender, EventArgs e) => Cursor = Cursors[CBE.Cursor];
        private void OnRightClicked(object sender, EventArgs e) => RightClickMenu.Show(Cursor.Position);
        private void OnSizeChanged(object sender, EventArgs e) => CBE.Resize(Width, Height);

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) => CBE.MouseDown(GetInputState(e));
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e) => CBE.MouseUp(GetInputState(e));
        private void pictureBox1_DoubleClick(object sender, EventArgs e) => CBE.DoubleClick();
        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e) => CBE.MouseWheel(GetInputState(e), e.Delta > 0);
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) => CBE.MouseMove(GetInputState(e));

        private InputState GetInputState(MouseEventArgs e) =>
            new InputState()
            {
                X = e.X,
                Y = e.Y,
                LeftDown = e.Button == MouseButtons.Left,
                RightDown = e.Button == MouseButtons.Right,
                MiddleDown = e.Button == MouseButtons.Middle,
                ShiftDown = ModifierKeys.HasFlag(Keys.Shift),
                CtrlDown = ModifierKeys.HasFlag(Keys.Control),
                AltDown = ModifierKeys.HasFlag(Keys.Alt),
            };

        private void RightClickMenu_Copy_Click(object sender, EventArgs e) => Clipboard.SetImage(Plot.Render());
        private void RightClickMenu_Help_Click(object sender, EventArgs e) => Process.Start("https://swharden.com/scottplot");
        private void RightClickMenu_AutoAxis_Click(object sender, EventArgs e) { Plot.AxisAuto(); Render(); }
        private void RightClickMenu_SaveImage_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                FileName = "ScottPlot.png",
                Filter = "PNG Files (*.png)|*.png;*.png" +
                         "|JPG Files (*.jpg, *.jpeg)|*.jpg;*.jpeg" +
                         "|BMP Files (*.bmp)|*.bmp;*.bmp" +
                         "|All files (*.*)|*.*"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
                Plot.SaveFig(sfd.FileName);
        }
    }
}
