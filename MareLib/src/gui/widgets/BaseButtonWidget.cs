using System;
using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Implementation of a button missing: rendering, textures.
/// </summary>
public class BaseButtonWidget : Widget
{
    protected ButtonState state = ButtonState.Normal;
    protected Action onClick;

    public BaseButtonWidget(Gui gui, Bounds bounds, Action onClick) : base(gui, bounds)
    {
        this.onClick = onClick;
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseMove += GuiEvents_MouseMove;
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseUp += GuiEvents_MouseUp;
    }

    public void SetCallback(Action onClick)
    {
        this.onClick = onClick;
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (state == ButtonState.Active) return;
        state = bounds.IsInsideAndClip(obj) ? ButtonState.Hovered : ButtonState.Normal;
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && bounds.IsInsideAndClip(obj))
        {
            obj.Handled = true;
            state = ButtonState.Active;
        }
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        if (state != ButtonState.Active) return;

        if (bounds.IsInsideAndClip(obj))
        {
            onClick();
            state = ButtonState.Hovered;
        }
        else
        {
            state = ButtonState.Normal;
        }
    }
}