using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Provides a simple background, captures mouse events.
/// </summary>
public class BackgroundWidgetSkia : Widget
{
    private readonly NineSliceTexture texture;
    public override int SortPriority => 1; // Sort above.

    public BackgroundWidgetSkia(Gui gui, Bounds bounds, NineSliceTexture texture) : base(gui, bounds)
    {
        this.texture = texture;
    }

    public override void OnRender(float dt, MareShader shader)
    {
        RenderTools.RenderNineSlice(texture, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseMove += GuiEvents_MouseMove;
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (bounds.IsInAllBounds(obj))
        {
            obj.Handled = true;
        }
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && bounds.IsInAllBounds(obj))
        {
            obj.Handled = true;
        }
    }
}