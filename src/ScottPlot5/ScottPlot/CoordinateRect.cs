﻿namespace ScottPlot;

/// <summary>
/// Describes a rectangle in 2D coordinate space.
/// </summary>
public struct CoordinateRect
{
    public double XMin { get; set; }
    public double YMin { get; set; }
    public double XMax { get; set; }
    public double YMax { get; set; }

    public double XCenter => (XMax + XMin) / 2;
    public double YCenter => (YMax + YMin) / 2;
    public double Width => XMax - XMin;
    public double Height => YMax - YMin;
    public double Area => Width * Height;
    public bool HasArea => (Area != 0 && !double.IsNaN(Area) && !double.IsInfinity(Area));

    public CoordinateRect(Coordinates pt1, Coordinates pt2)
    {
        XMin = Math.Min(pt1.X, pt2.X);
        XMax = Math.Max(pt1.X, pt2.X);
        YMin = Math.Min(pt1.Y, pt2.Y);
        YMax = Math.Max(pt1.Y, pt2.Y);
    }

    public CoordinateRect(double xMin, double xMax, double yMin, double yMax)
    {
        XMin = xMin;
        XMax = xMax;
        YMin = yMin;
        YMax = yMax;
    }

    public static CoordinateRect Empty => new(double.NaN, double.NaN, double.NaN, double.NaN);

    public CoordinateRange XRange => new(XMin, XMax);

    public CoordinateRange YRange => new(YMin, YMax);

    public override string ToString()
    {
        return $"PixelRect: XMin={XMin} XMax={XMax} YMin={YMin} YMax={YMax}";
    }
}
