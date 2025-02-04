using OpenTK.Mathematics;

namespace MareLib;

public class WidgetTextLine : Widget
{
    private readonly TextObject textObject;
    private readonly bool center;

    public WidgetTextLine(Gui gui, Bounds bounds, Font font, string text, int fontSize, Vector4 color, bool center = true) : base(gui, bounds)
    {
        this.center = center;
        textObject = new TextObject(text, font, fontSize, color);
    }

    public override void OnRender(float dt, MareShader shader)
    {
        if (center)
        {
            textObject.RenderCenteredLine(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2), shader, true);
        }
        else
        {
            textObject.RenderLine(bounds.X, bounds.Y + (bounds.Height / 2), shader, 0, true);
        }
    }

    public void SetText(string text)
    {
        textObject.Text = text;
    }
}