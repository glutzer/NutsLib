using Vintagestory.API.Client;

namespace NutsLib;

public class GuiEvents
{
    public readonly Gui gui;

    public event Action<MouseEvent>? MouseDown;
    public event Action<MouseEvent>? MouseUp;
    public event Action<MouseEvent>? MouseMove;
    public event Action<MouseWheelEventArgs>? MouseWheel;

    /// Key down event, also calls key press if the key is a char.
    public event Action<KeyEvent>? KeyDown;
    public event Action<KeyEvent>? KeyUp;

    /// Also handle key down when this is called!
    public event Action<KeyEvent>? KeyPress;

    public event Action<float>? BeforeRender;
    public event Action<float>? AfterRender;

    public GuiEvents(Gui gui)
    {
        this.gui = gui;
    }

    public void ClearEvents()
    {
        MouseDown = null;
        MouseUp = null;
        MouseMove = null;
        MouseWheel = null;
        KeyDown = null;
        KeyUp = null;
        KeyPress = null;

        BeforeRender = null;
        AfterRender = null;
    }

    public void TriggerMouseDown(MouseEvent args)
    {
        MouseDown?.Invoke(args);
    }

    public void TriggerMouseUp(MouseEvent args)
    {
        MouseUp?.Invoke(args);
    }

    public void TriggerMouseMove(MouseEvent args)
    {
        MouseMove?.Invoke(args);
    }

    public void TriggerMouseWheel(MouseWheelEventArgs args)
    {
        MouseWheel?.Invoke(args);
    }

    public void TriggerKeyDown(KeyEvent args)
    {
        KeyDown?.Invoke(args);
    }

    public void TriggerKeyUp(KeyEvent args)
    {
        KeyUp?.Invoke(args);
    }

    public void TriggerKeyPress(KeyEvent args)
    {
        KeyPress?.Invoke(args);
    }

    public void TriggerBeforeRender(float dt)
    {
        BeforeRender?.Invoke(dt);
    }

    public void TriggerAfterRender(float dt)
    {
        AfterRender?.Invoke(dt);
    }
}