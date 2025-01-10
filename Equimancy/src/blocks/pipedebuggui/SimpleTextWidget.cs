using MareLib;
using OpenTK.Mathematics;

namespace Equimancy;

public class SimpleTextWidget : Widget
{
    private readonly TextObject textObject;

    public SimpleTextWidget(Gui gui, Bounds bounds, string text, int fontSize) : base(gui, bounds)
    {
        textObject = new TextObject(text, FontRegistry.GetFont("friz"), fontSize, Vector4.One);
    }

    public override void OnRender(float dt, MareShader shader)
    {
        textObject.RenderCenteredLine(bounds.X, bounds.Y, shader, false);
    }

    public void SetText(string text)
    {
        textObject.Text = text;
    }
}