using Vintagestory.API.Client;

namespace NutsLib;

/// <summary>
/// Button that can be toggled, and released.
/// </summary>
public class WidgetBaseToggleableButton : WidgetBaseButton
{
    private readonly Action<bool> onToggle;
    protected bool enabled;
    protected bool allowManualRelease;

    public WidgetBaseToggleableButton(Widget? parent, Gui gui, Action<bool> onToggle, bool currentValue, bool allowManualRelease = true) : base(parent, gui, null!)
    {
        this.onToggle = onToggle;
        this.allowManualRelease = allowManualRelease;
        enabled = currentValue;
        SetCallback(Toggle);
    }

    public void Release(bool doCallback = true)
    {
        if (enabled)
        {
            enabled = false;

            if (doCallback)
            {
                onToggle(false);
            }
        }
    }

    private void Toggle()
    {
        if (enabled && allowManualRelease)
        {
            onToggle(false);
            enabled = false;

        }
        else
        {
            onToggle(true);
            enabled = true;
        }
    }

    protected override void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (enabled && !allowManualRelease)
        {
            state = EnumButtonState.Normal;
            return;
        }
        base.GuiEvents_MouseDown(obj);
    }

    protected override void GuiEvents_MouseUp(MouseEvent obj)
    {
        if (enabled && !allowManualRelease)
        {
            state = EnumButtonState.Normal;
            return;
        }
        base.GuiEvents_MouseUp(obj);
    }
}