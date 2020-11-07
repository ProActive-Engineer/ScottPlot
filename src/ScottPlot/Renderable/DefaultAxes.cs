﻿namespace ScottPlot.Renderable
{
    public class DefaultBottomAxis : Axis
    {
        public DefaultBottomAxis()
        {
            Edge = Edge.Bottom;
            PixelSize = 40;
            Ticks.MajorGridEnable = true;
        }
    }

    public class DefaultTopAxis : Axis
    {
        public DefaultTopAxis()
        {
            Edge = Edge.Top;
            PixelSize = 40;
            Title.Font.Bold = true;
            Title.IsVisible = true;

            Ticks.MajorTickEnable = false;
            Ticks.MinorTickEnable = false;
            Ticks.MajorLabelEnable = false;
            Ticks.MajorGridEnable = false;
        }
    }

    public class DefaultLeftAxis : Axis
    {
        public DefaultLeftAxis()
        {
            Edge = Edge.Left;
            PixelSize = 60;
            Ticks.MajorGridEnable = true;
        }
    }

    public class DefaultRightAxis : Axis
    {
        public DefaultRightAxis()
        {
            Edge = Edge.Right;
            PixelSize = 60;

            Ticks.MajorTickEnable = false;
            Ticks.MinorTickEnable = false;
            Ticks.MajorLabelEnable = false;
            Title.IsVisible = false;
            Ticks.MajorGridEnable = false;
        }
    }
}
