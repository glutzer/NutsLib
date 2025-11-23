namespace NutsLib;

public class WidgetVanillaTitle : WidgetBaseDraggableTitle
{
    private readonly TextObject text;

    public WidgetVanillaTitle(Widget? parent, Gui gui, Widget draggableWidget, string title) : base(parent, gui, draggableWidget)
    {
        text = new TextObject(title, VanillaThemes.Font, 1f, VanillaThemes.WhitishTextColor)
        {
            Shadow = true
        };
    }

    public WidgetVanillaTitle SetTitle(string title)
    {
        text.Text = title;
        return this;
    }

    public override void OnRender(float dt, ShaderGui shader)
    {
        RenderTools.RenderNineSlice(VanillaThemes.OutsetTexture, shader, X, Y, Width, Height);

        text.SetScaleFromWidget(this, 0.8f, 0.6f);
        text.RenderLine(X + (Width * 0.05f), YCenter, shader, centerVertically: true);
    }
}