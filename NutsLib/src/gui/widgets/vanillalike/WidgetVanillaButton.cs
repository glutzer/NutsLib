using OpenTK.Mathematics;

namespace NutsLib;

public class WidgetVanillaButton : WidgetBaseButton
{
    private readonly TextObject textLine = new("", VanillaThemes.Font, 1, Vector4.One);
    private readonly EnumButtonState previous;

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

    public override void OnRender(float dt, ShaderGui shader)
    {
        if (state == EnumButtonState.Hovered)
        {
            shader.Color = new Vector4(1.1f);
        }

        RenderTools.RenderNineSlice(state == EnumButtonState.Active ? VanillaThemes.InsetTexture : VanillaThemes.OutsetTexture, shader, X, Y, Width, Height);

        shader.ResetColor();

        textLine.color = state == EnumButtonState.Active ? VanillaThemes.YellowColor : VanillaThemes.WhitishGreyColor;

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