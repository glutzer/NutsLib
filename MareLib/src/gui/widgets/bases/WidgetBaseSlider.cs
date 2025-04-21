using System;
using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Implementation of a button missing: rendering, textures.
/// Slider that returns an int and the steps of the slider.
/// </summary>
public class WidgetBaseSlider : Widget
{
    protected EnumButtonState state = EnumButtonState.Normal;
    protected Action<int> onNewValue;
    protected int steps;
    protected int cursorStep;
    protected bool onlyCallOnRelease;

    public float Percentage => (float)cursorStep / steps;

    public WidgetBaseSlider(Widget? parent, Action<int> onNewValue, int steps, bool onlyCallOnRelease = false) : base(parent)
    {
        this.onNewValue = onNewValue;
        this.steps = steps;
        this.onlyCallOnRelease = onlyCallOnRelease;
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseWheel += GuiEvents_MouseWheel;
        guiEvents.MouseMove += GuiEvents_MouseMove;
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseUp += GuiEvents_MouseUp;
    }

    protected virtual void GuiEvents_MouseWheel(MouseWheelEventArgs obj)
    {
        if (IsInAllBounds(Gui.MouseX, Gui.MouseY) && !obj.IsHandled)
        {
            int oldCursorStep = cursorStep;
            cursorStep += obj.delta;
            cursorStep = Math.Clamp(cursorStep, 0, steps);

            if (oldCursorStep != cursorStep)
            {
                onNewValue(cursorStep);
            }

            obj.SetHandled();
        }
    }

    protected virtual void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (state == EnumButtonState.Active)
        {
            cursorStep = (int)Math.Round((obj.X - X) / (float)Width * steps);
            cursorStep = Math.Clamp(cursorStep, 0, steps);

            if (!onlyCallOnRelease)
            {
                onNewValue(cursorStep);
            }

            return;
        }

        if (IsInAllBounds(obj) && !obj.Handled)
        {
            state = EnumButtonState.Hovered;
            obj.Handled = true;
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

        if (onlyCallOnRelease)
        {
            onNewValue(cursorStep);
        }

        state = IsInsideAndClip(obj) ? EnumButtonState.Hovered : EnumButtonState.Normal;
    }
}