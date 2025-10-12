using OpenTK.Mathematics;

namespace NutsLib;

/// <summary>
/// Text line which fits within the bounds.
/// </summary>
public class WidgetTextLine : Widget
{
    private readonly TextObject textObject;
    private readonly bool center;

    public WidgetTextLine(Widget? parent, Font font, string text, Vector4 color, bool center = true, bool shadow = true) : base(parent)
    {
        this.center = center;
        textObject = new TextObject(text, font, 50, color);

        OnResize += () =>
        {
            textObject.SetScaleFromWidget(this, 0.9f, 0.7f);
        };

        textObject.Shadow = shadow;
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