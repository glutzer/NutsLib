using OpenTK.Mathematics;
using Vintagestory.API.Client;

namespace NutsLib;

/// <summary>
/// Provides a simple background, captures mouse events.
/// Does not dispose textures.
/// </summary>
public class WidgetSliceBackground : Widget
{
    private readonly NineSliceTexture texture;
    public override int SortPriority => 1; // Sort above.
    private Vector4 color;

    public int SliceScale { get; set; } = 1;

    public WidgetSliceBackground(Widget? parent, Gui gui, NineSliceTexture texture, Vector4 color) : base(parent, gui)
    {
        this.texture = texture;
        this.color = color;
    }

    public override void OnRender(float dt, ShaderGui shader)
    {
        shader.Uniform("color", color);
        RenderTools.RenderNineSlice(texture, shader, X, Y, Width, Height, SliceScale);
        shader.Uniform("color", Vector4.One);
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseMove += GuiEvents_MouseMove;

        guiEvents.MouseWheel += e =>
        {
            if (!e.IsHandled && IsInAllBounds(Gui.MouseX, Gui.MouseY)) e.SetHandled();
        };
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (IsInAllBounds(obj) && !obj.Handled)
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