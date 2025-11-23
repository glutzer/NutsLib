namespace NutsLib;

public class WidgetVanillaScrollBar : WidgetBaseScrollBar
{
    public WidgetVanillaScrollBar(Widget? parent, Gui gui, Widget? scrollWidget, int stepsPerPage = 10) : base(parent, gui, scrollWidget, stepsPerPage)
    {
    }

    protected override void RenderCursor(int x, int y, int width, int height, ShaderGui shader, EnumButtonState barState)
    {
        RenderTools.RenderNineSlice(VanillaThemes.OutsetTexture, shader, x, y, width, height);
    }

    protected override void RenderBackground(int x, int y, int width, int height, ShaderGui shader)
    {
        RenderTools.RenderNineSlice(VanillaThemes.InsetTexture, shader, x, y, width, height);
    }
}