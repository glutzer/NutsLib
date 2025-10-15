using System;
using Vintagestory.API.Client;

namespace NutsLib;

/// <summary>
/// Implementation of button logic.
/// Clicks when releasing on button.
/// </summary>
public class WidgetBaseButton : Widget
{
    protected EnumButtonState state = EnumButtonState.Normal;
    protected Action onClick;

    public WidgetBaseButton(Widget? parent, Gui gui, Action onClick) : base(parent, gui)
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
        if (!obj.Handled && IsInAllBounds(obj))
        {
            obj.Handled = true;
            state = EnumButtonState.Active;
        }
    }

    protected virtual void GuiEvents_MouseUp(MouseEvent obj)
    {
        if (state != EnumButtonState.Active) return;

        if (IsInAllBounds(obj))
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