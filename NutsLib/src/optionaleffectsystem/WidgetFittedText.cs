using OpenTK.Mathematics;

namespace NutsLib;

/// <summary>
/// Renders a line of text centered and fitted on the bounds.
/// </summary>
public class WidgetFittedText : Widget
{
    private readonly TextObject text;

    public WidgetFittedText(Widget? parent, Gui gui, string initialText, Vector4 color) : base(parent, gui)
    {
        text = new TextObject(initialText, VanillaThemes.Font, 50, color)
        {
            Shadow = true
        };

        OnResize += () =>
        {
            text.SetScaleFromWidget(this, 0.8f, 0.6f);
        };
    }

    public void SetText(string newText)
    {
        text.Text = newText;
    }

    public override void OnRender(float dt, ShaderGui shader)
    {
        text.RenderCenteredLine(XCenter, YCenter, shader, true);
    }
}