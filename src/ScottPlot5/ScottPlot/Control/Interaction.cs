﻿using ScottPlot.Axis;

namespace ScottPlot.Control;

/// <summary>
/// This class contains logic to perform plot manipulations in response to UI actions.
/// To customize how user inputs are interpreted, inherit and override functions in this class.
/// To customize behavior of actions, replace properties of <see cref="Actions"/> with custom delegates.
/// To customize UI inputs, assign desired button and key properties of <see cref="Inputs"/>.
/// </summary>
public class Interaction
{
    private readonly IPlotControl Control;

    /// <summary>
    /// Buttons and keys in this object can be overwritten to customize actions for specific user input events.
    /// (e.g., make left-click-drag zoom instead of pan)
    /// </summary>
    public InputBindings Inputs = InputBindings.Standard();

    /// <summary>
    /// Delegates in this object can be overwritten with custom functions that manipulate the plot.
    /// (e.g., changing the sensitivity of click-drag-zooming)
    /// </summary>
    public PlotActions Actions = PlotActions.Standard();

    private readonly KeyboardState Keyboard = new();
    private readonly MouseState Mouse = new();
    private bool LockX => Inputs.ShouldLockX(Keyboard.PressedKeys);
    private bool LockY => Inputs.ShouldLockY(Keyboard.PressedKeys);
    private bool IsZoomingRectangle = false;

    public Interaction(IPlotControl control)
    {
        Control = control;
    }

    /// <summary>
    /// Return the last observed location of the mouse in coordinate units
    /// </summary>
    public Coordinates GetMouseCoordinates(IXAxis? xAxis = null, IYAxis? yAxis = null)
    {
        return Control.Plot.GetCoordinate(Mouse.LastPosition, xAxis, yAxis);
    }

    public virtual void OnMouseMove(Pixel newPosition)
    {
        Mouse.LastPosition = newPosition;

        if (Mouse.PressedButtons.Any() && Mouse.IsDragging(newPosition))
        {
            MouseDrag(
                from: Mouse.MouseDownPosition,
                to: newPosition,
                button: Mouse.PressedButtons.First(),
                keys: Keyboard.PressedKeys,
                start: Mouse.MouseDownAxisLimits);
        }
    }

    protected virtual void MouseDrag(Pixel from, Pixel to, MouseButton button, IEnumerable<Key> keys, AxisLimits start)
    {
        bool lockY = Inputs.ShouldLockY(keys);
        bool lockX = Inputs.ShouldLockX(keys);
        LockedAxes locks = new(lockX, LockY);

        MouseDrag drag = new(start, from, to);

        if (Inputs.ShouldZoomRectangle(button, keys))
        {
            Actions.DragZoomRectangle(Control, drag, locks);
            IsZoomingRectangle = true;
        }
        else if (button == Inputs.DragPanButton)
        {
            Actions.DragPan(Control, drag, locks);
        }
        else if (button == Inputs.DragZoomButton)
        {

            Actions.DragZoom(Control, drag, locks);
        }
    }

    public virtual void KeyUp(Key key)
    {
        Keyboard.Up(key);
    }

    public virtual void KeyDown(Key key)
    {
        Keyboard.Down(key);
    }

    public virtual void MouseDown(Pixel position, MouseButton button)
    {
        Mouse.Down(position, button, Control.Plot.GetAxisLimits());
    }

    public virtual void MouseUp(Pixel position, MouseButton button)
    {

        bool isDragging = Mouse.IsDragging(position);

        bool droppedZoomRectangle =
            isDragging &&
            Inputs.ShouldZoomRectangle(button, Keyboard.PressedKeys) &&
            IsZoomingRectangle;

        if (droppedZoomRectangle)
        {
            Actions.ZoomRectangleApply(Control);
            IsZoomingRectangle = false;
        }

        // this covers the case where an extremely tiny zoom rectangle was made
        if ((isDragging == false) && (button == Inputs.ClickAutoAxisButton))
        {
            Actions.AutoScale(Control);
        }

        if (button == Inputs.DragZoomRectangleButton)
        {
            Actions.ZoomRectangleClear(Control);
            IsZoomingRectangle = false;
        }

        Mouse.Up(button);
    }

    public virtual void DoubleClick()
    {
        Actions.ToggleBenchmark(Control);
    }

    public virtual void MouseWheelVertical(Pixel pixel, float delta)
    {
        MouseWheelDirection direction = delta > 0 ? MouseWheelDirection.Up : MouseWheelDirection.Down;

        if (Inputs.ZoomInWheelDirection.HasValue && Inputs.ZoomInWheelDirection == direction)
        {
            Actions.ZoomIn(Control, pixel, new LockedAxes(LockX, LockY));
        }
        else if (Inputs.ZoomOutWheelDirection.HasValue && Inputs.ZoomOutWheelDirection == direction)
        {
            Actions.ZoomOut(Control, pixel, new LockedAxes(LockX, LockY));
        }
        else if (Inputs.PanUpWheelDirection.HasValue && Inputs.PanUpWheelDirection == direction)
        {
            Actions.PanUp(Control);
        }
        else if (Inputs.PanDownWheelDirection.HasValue && Inputs.PanDownWheelDirection == direction)
        {
            Actions.PanDown(Control);
        }
        else if (Inputs.PanRightWheelDirection.HasValue && Inputs.PanRightWheelDirection == direction)
        {
            Actions.PanRight(Control);
        }
        else if (Inputs.PanLeftWheelDirection.HasValue && Inputs.PanLeftWheelDirection == direction)
        {
            Actions.PanLeft(Control);
        }
        else
        {
            return;
        }
    }
}
