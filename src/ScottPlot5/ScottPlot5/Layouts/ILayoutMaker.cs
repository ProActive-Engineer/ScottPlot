﻿namespace ScottPlot.Layouts;

/// <summary>
/// This interface describes a class that decides how to lay-out a collection of panels around the
/// edges of a figure and create a final layout containing size and position of all panels
/// and also the size and position of the data area.
/// </summary>
public interface ILayoutMaker // TODO: rename interface to something more expressive
{
    public Layout GetLayout(PixelSize figureSize, IEnumerable<IPanel> panels);
}
