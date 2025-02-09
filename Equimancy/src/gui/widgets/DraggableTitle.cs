using MareLib;
using OpenTK.Mathematics;

namespace Equimancy;

public class DraggableTitle : WidgetBaseDraggableTitle
{
    public NineSliceTexture background;
    public TextObject text;

    public DraggableTitle(Gui gui, Bounds bounds, Bounds draggableBounds, string title, int fontScale) : base(gui, bounds, draggableBounds)
    {
        background = EqGui.Background;
        text = new TextObject(title, FontRegistry.GetFont("friz"), bounds.Scaled(fontScale), Vector4.One);
    }

    public override void OnRender(float dt, MareShader shader)
    {
        RenderTools.RenderNineSlice(background, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        text.RenderCenteredLine(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2, shader, true);
    }
}