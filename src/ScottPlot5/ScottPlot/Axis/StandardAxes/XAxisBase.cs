﻿namespace ScottPlot.Axis.StandardAxes;

public abstract class XAxisBase
{
    public double Left { get; set; } = 0;
    public double Right { get; set; } = 0;
    public double Width => Right - Left;
    public bool HasBeenSet { get; set; } = false;
    public CoordinateRange Range => new(Left, Right);

    public float GetPixel(double position, PixelRect dataArea)
    {
        double pxPerUnit = dataArea.Width / Width;
        double unitsFromLeftEdge = position - Left;
        float pxFromEdge = (float)(unitsFromLeftEdge * pxPerUnit);
        return dataArea.Left + pxFromEdge;
    }

    public double GetCoordinate(float pixel, PixelRect dataArea)
    {
        double pxPerUnit = dataArea.Width / Width;
        float pxFromLeftEdge = pixel - dataArea.Left;
        double unitsFromEdge = pxFromLeftEdge / pxPerUnit;
        return Left + unitsFromEdge;
    }
}
