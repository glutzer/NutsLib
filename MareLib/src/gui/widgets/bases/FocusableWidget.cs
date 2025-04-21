using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Interface for FocusableWidget.
/// </summary>
public interface IFocusable
{
    bool Focused { get; set; }
    void Focus();
    void Unfocus();
}

/// <summary>
/// A widget that can be focused, transferring focus from the previous one in a hierachy, with tab.
/// Must register events first.
/// </summary>
public class FocusableWidget : Widget, IFocusable
{
    public bool Focused { get; set; }
    public Gui? gui;

    public FocusableWidget(Widget? parent) : base(parent)
    {
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.KeyDown += FocusKeyDown;
        gui = guiEvents.gui;
    }

    private void FocusKeyDown(KeyEvent obj)
    {
        if (!obj.Handled && Focused && obj.KeyCode == (int)GlKeys.Tab)
        {
            TransferFocus(obj.CtrlPressed);
            obj.Handled = true;
        }
    }

    private void TransferFocus(bool backwards)
    {
        if (gui == null) return;

        bool foundIndex = false;

        // Find first widget after this one, call focus on it.

        if (backwards)
        {
            foreach (IFocusable widget in gui.ForWidgets<IFocusable>())
            {
                if (foundIndex == true)
                {
                    widget.Focus();
                    return;
                }

                if (this == widget)
                {
                    foundIndex = true;
                }
            }
        }
        else
        {
            foreach (IFocusable widget in gui.ForWidgetsReverse<IFocusable>())
            {
                if (foundIndex == true)
                {
                    widget.Focus();
                    return;
                }

                if (this == widget)
                {
                    foundIndex = true;
                }
            }
        }

        // None found, unfocus.
        Unfocus();
    }

    public virtual void Focus()
    {
        if (gui == null) return;

        // Un-focus widgets in gui.
        foreach (IFocusable widget in gui.ForWidgetsReverse<IFocusable>())
        {
            widget.Unfocus();
        }

        // Focus this widget.
        Focused = true;
    }

    public virtual void Unfocus()
    {
        Focused = false;
    }
}