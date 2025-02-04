using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Provides a simple background, captures mouse events.
/// Does not dispose textures.
/// </summary>
public class SliceBackground : Widget
{
    private readonly NineSliceTexture texture;
    public override int SortPriority => 1; // Sort above.

    public SliceBackground(Widget? parent, NineSliceTexture texture) : base(parent)
    {
        this.texture = texture;
    }

    public override void OnRender(float dt, MareShader shader)
    {
        RenderTools.RenderNineSlice(texture, shader, X, Y, Width, Height);
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseMove += GuiEvents_MouseMove;
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (IsInAllBounds(obj))
        {
            obj.Handled = true;
        }
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && IsInAllBounds(obj))
        {
            obj.Handled = true;
        }
    }
}