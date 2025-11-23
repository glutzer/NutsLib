using OpenTK.Mathematics;

namespace NutsLib;

public class WidgetVanillaButton : WidgetBaseButton
{
    private readonly TextObject textLine = new("", VanillaThemes.Font, 1, Vector4.One);
    private Vector4 color = Vector4.One;

    public WidgetVanillaButton(Widget? parent, Gui gui, Action onRelease, string text) : base(parent, gui, onRelease)
    {
        textLine.Text = text;
        OnResize += () =>
        {
            textLine.SetScaleFromWidget(this, 0.6f, 0.9f);
        };
    }

    public void SetText(string text)
    {
        textLine.Text = text;
    }

    public WidgetVanillaButton SetColor(Vector4 color)
    {
        this.color = color;
        return this;
    }

    public override void OnRender(float dt, ShaderGui shader)
    {
        Vector4 color = this.color;

        if (state == EnumButtonState.Hovered)
        {
            color.Xyz *= 1.1f;
        }

        shader.Color = color;
        RenderTools.RenderNineSlice(state == EnumButtonState.Active ? VanillaThemes.InsetTexture : VanillaThemes.OutsetTexture, shader, X, Y, Width, Height);
        shader.ResetColor();

        textLine.color = state == EnumButtonState.Active ? VanillaThemes.YellowColor : VanillaThemes.WhitishTextColor;

        textLine.RenderCenteredLine(XCenter, YCenter, shader, true);

        textLine.Shadow = true;
    }

    protected override void OnMousedOver()
    {
        MainAPI.Capi.Gui.PlaySound("menubutton");
    }

    protected override void OnClicked()
    {
        MainAPI.Capi.Gui.PlaySound("menubutton_press");
    }
}