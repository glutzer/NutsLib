using System;
using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Implementation of a button missing: rendering, textures.
/// </summary>
public class WidgetBaseButton : Widget
{
    protected EnumButtonState state = EnumButtonState.Normal;
    protected Action onClick;

    public WidgetBaseButton(Widget? parent, Action onClick) : base(parent)
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

    protected virtual void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (IsInAllBounds(obj) && !obj.Handled)
        {
            if (state != EnumButtonState.Active) state = EnumButtonState.Hovered;
            obj.Handled = true;
        }
        else
        {
            if (state != EnumButtonState.Active) state = EnumButtonState.Normal;
        }
    }

    protected virtual void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && IsInsideAndClip(obj))
        {
            obj.Handled = true;
            state = EnumButtonState.Active;
        }
    }

    protected virtual void GuiEvents_MouseUp(MouseEvent obj)
    {
        if (state != EnumButtonState.Active) return;

        if (IsInsideAndClip(obj))
        {
            onClick();
            state = EnumButtonState.Hovered;
        }
        else
        {
            state = EnumButtonState.Normal;
        }
    }
}