using OpenTK.Mathematics;

namespace NutsLib;

public class WidgetVanillaSlider : WidgetBaseSlider
{
    protected TextObject displayText = new("", VanillaThemes.Font, Gui.Scaled(4f), VanillaThemes.WhitishTextColor)
    {
        Shadow = true
    };

    public WidgetVanillaSlider(Widget? parent, Gui gui, Action<int> onNewValue, int steps, bool onlyCallOnRelease = false) : base(parent, gui, onNewValue, steps, onlyCallOnRelease)
    {
    }

    protected override void RenderCursor(int x, int y, int width, int height, ShaderGui shader)
    {
        shader.Color = VanillaThemes.BlueProgress;
        RenderTools.RenderNineSlice(VanillaThemes.OutsetTexture, shader, X, Y, x - X, height);

        Vector4 color = Vector4.One;

        if (state == EnumButtonState.Hovered) color *= 1.1f;

        shader.Color = color;

        if (state == EnumButtonState.Active)
        {
            RenderTools.RenderNineSlice(VanillaThemes.InsetTexture, shader, x, y, width, height);
        }
        else
        {
            RenderTools.RenderNineSlice(VanillaThemes.OutsetTexture, shader, x, y, width, height);
        }

        shader.ResetColor();

        // Render display text.
        if (state != EnumButtonState.Normal)
        {
            int textWidth = displayText.PixelLength;
            RenderTools.RenderNineSlice(VanillaThemes.OutsetTexture, shader, x - (textWidth / 2) + (width / 2) - Gui.Scaled(4), y - Gui.Scaled(9), textWidth + Gui.Scaled(8), Gui.Scaled(7));
            displayText.Text = cursorStep.ToString();
            displayText.RenderCenteredLine(x + (width / 2), y - Gui.Scaled(4), shader);
        }
    }

    protected override void RenderBackground(int x, int y, int width, int height, ShaderGui shader)
    {
        RenderTools.RenderNineSlice(VanillaThemes.InsetTexture, shader, x, y, width, height);
    }
}