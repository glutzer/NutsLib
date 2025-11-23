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

    private void Toggle()
    {
        if (enabled && allowManualRelease)
        {
            enabled = false;
            onToggle(false);
        }
        else
        {
            enabled = true;
            onToggle(true);
        }
    }
}