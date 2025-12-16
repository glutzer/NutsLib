using Vintagestory.API.Client;

namespace NutsLib;

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

    public WidgetBaseSlider(Widget? parent, Gui gui, Action<int> onNewValue, int steps, bool onlyCallOnRelease = false) : base(parent, gui)
    {
        this.onNewValue = onNewValue;
        this.steps = steps;
        this.onlyCallOnRelease = onlyCallOnRelease;
    }

    public WidgetBaseSlider SetCurrentStep(int step)
    {
        cursorStep = Math.Clamp(step, 0, steps);
        return this;
    }

    public override void OnRender(float dt, ShaderGui shader)
    {
        RenderBackground(X, Y, Width, Height, shader);

        float percentage = Percentage;
        float percentWidth = Width / 20f;
        float offsetableWidth = Width - percentWidth;
        float offset = offsetableWidth * percentage;

        RenderCursor((int)(X + offset), Y, (int)percentWidth, Height, shader);
    }

    protected virtual void RenderBackground(int x, int y, int width, int height, ShaderGui shader)
    {

    }

    protected virtual void RenderCursor(int x, int y, int width, int height, ShaderGui shader)
    {

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
        else
        {
            state = EnumButtonState.Normal;
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

        state = IsInAllBounds(obj) ? EnumButtonState.Hovered : EnumButtonState.Normal;
    }
}