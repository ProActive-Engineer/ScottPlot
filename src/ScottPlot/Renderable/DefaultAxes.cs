﻿namespace ScottPlot.Renderable
{
    public class DefaultBottomAxis : Axis
    {
        public DefaultBottomAxis()
        {
            Edge = Edge.Bottom;
            PixelSize = 40;
            Ticks.MajorGridStyle = LineStyle.Solid;
            TickCollection.verticalAxis = false;
        }
    }

    public class DefaultTopAxis : Axis
    {
        public DefaultTopAxis()
        {
            Edge = Edge.Top;
            PixelSize = 40;
            Title.Font.Bold = true;
            TickCollection.verticalAxis = false;
        }
    }

    public class DefaultLeftAxis : Axis
    {
        public DefaultLeftAxis()
        {
            Edge = Edge.Left;
            PixelSize = 60;
            Ticks.MajorGridStyle = LineStyle.Solid;
            TickCollection.verticalAxis = true;
        }
    }

    public class DefaultRightAxis : Axis
    {
        public DefaultRightAxis()
        {
            Edge = Edge.Right;
            PixelSize = 60;
            TickCollection.verticalAxis = true;
        }
    }
}
