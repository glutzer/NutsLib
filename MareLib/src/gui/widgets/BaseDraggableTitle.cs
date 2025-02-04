using OpenTK.Mathematics;
using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Draggable title that moves the passed in bounds to that fixed point.
/// </summary>
public class BaseDraggableTitle : Widget
{
    public Widget draggableWidget;
    private bool held;

    private Vector2i startedDraggingFixed;
    private Vector2i startedDraggingMouse;

    public BaseDraggableTitle(Widget? parent, Widget draggableWidget) : base(parent)
    {
        this.draggableWidget = draggableWidget;
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseMove += GuiEvents_MouseMove;
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseUp += GuiEvents_MouseUp;
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        held = false;
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && IsInAllBounds(obj))
        {
            held = true;
            startedDraggingFixed = draggableWidget.GetFixedPos();
            startedDraggingMouse = new Vector2i(obj.X, obj.Y);
            obj.Handled = true;
        }
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (IsInAllBounds(obj)) obj.Handled = true;

        if (!held) return;

        Vector2i offset = new(obj.X - startedDraggingMouse.X, obj.Y - startedDraggingMouse.Y);
        offset /= Scale;

        Vector2i newFixed = startedDraggingFixed + offset;

        draggableWidget.Move(newFixed.X, newFixed.Y);
    }
}