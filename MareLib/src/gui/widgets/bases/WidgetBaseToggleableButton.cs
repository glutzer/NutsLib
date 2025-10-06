using System;
using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Button that can be toggled, and released.
/// </summary>
public class WidgetBaseToggleableButton : Widget
{
    protected EnumButtonState state = EnumButtonState.Normal;
    protected Action<bool> onClick;
    protected bool allowRelease;

    public WidgetBaseToggleableButton(Widget? parent, Action<bool> onClick, bool allowRelease, bool currentValue) : base(parent)
    {
        this.onClick = onClick;
        this.allowRelease = allowRelease;
        state = currentValue ? EnumButtonState.Active : EnumButtonState.Normal;
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseMove += GuiEvents_MouseMove;
        guiEvents.MouseDown += GuiEvents_MouseDown;
    }

    public void SetCallback(Action<bool> onClick)
    {
        this.onClick = onClick;
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (!IsInAllBounds(obj))
        {
            if (state == EnumButtonState.Hovered) state = EnumButtonState.Normal;
            return;
        }

        obj.Handled = true;
        if (state == EnumButtonState.Active) return;

        state = IsInAllBounds(obj) ? EnumButtonState.Hovered : EnumButtonState.Normal;
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && IsInAllBounds(obj))
        {
            obj.Handled = true;

            if (state != EnumButtonState.Active)
            {
                onClick(true);
                state = EnumButtonState.Active;
            }
            else if (allowRelease)
            {
                onClick(false);
                state = EnumButtonState.Hovered;
            }
        }
    }

    /// <summary>
    /// Force release of button, even if not allow release.
    /// </summary>
    public void Release(bool doEvent = false)
    {
        if (state == EnumButtonState.Active)
        {
            state = EnumButtonState.Normal;
            if (doEvent) onClick(false);
        }
    }
}