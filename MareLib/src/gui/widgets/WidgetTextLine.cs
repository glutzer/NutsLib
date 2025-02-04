using OpenTK.Mathematics;

namespace MareLib;

public class WidgetTextLine : Widget
{
    private readonly TextObject textObject;
    private readonly bool center;

    public WidgetTextLine(Widget? parent, Font font, string text, int fontSize, Vector4 color, bool center = true) : base(parent)
    {
        this.center = center;
        textObject = new TextObject(text, font, fontSize, color);
    }

    public override void OnRender(float dt, MareShader shader)
    {
        if (center)
        {
            textObject.RenderCenteredLine(X + (Width / 2), Y + (Height / 2), shader, true);
        }
        else
        {
            textObject.RenderLine(X, Y + (Height / 2), shader, 0, true);
        }
    }

    public void SetText(string text)
    {
        textObject.Text = text;
    }
}