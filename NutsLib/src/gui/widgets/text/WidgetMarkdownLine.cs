namespace NutsLib;

/// <summary>
/// A single formatted line, including indecipherable spans. Clips to its bounds.
/// </summary>
public class WidgetMarkdownLine : Widget
{
    private readonly TextObjectGroup text = new();
    private readonly int fontScale;
    private readonly bool center;
    private readonly bool shadow;

    public WidgetMarkdownLine(Widget? parent, Gui gui, string markdown, int fontScale = 16, bool center = false, bool shadow = true) : base(parent, gui)
    {
        this.fontScale = fontScale;
        this.center = center;
        this.shadow = shadow;
        SetText(markdown);
    }

    public void SetText(string markdown)
    {
        text.textObjects = Markdown.ConvertMarkdownLine(markdown, fontScale);
        foreach (TextObject part in text.textObjects) part.Shadow = shadow;
    }

    public override void OnRender(float dt, ShaderGui shader)
    {
        RenderTools.PushScissor(this);
        if (center) text.RenderCenteredLine(XCenter, YCenter, shader, true);
        else text.RenderLine(X, YCenter, shader, centerVertically: true);
        RenderTools.PopScissor();
    }
}
