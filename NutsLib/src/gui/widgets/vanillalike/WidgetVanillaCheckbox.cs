using OpenTK.Mathematics;

namespace NutsLib;

public class WidgetVanillaCheckbox : WidgetBaseToggleableButton
{
    public WidgetVanillaCheckbox(Widget? parent, Gui gui, Action<bool> onClick, bool currentValue) : base(parent, gui, onClick, currentValue, true)
    {
    }

    protected override void OnMousedOver()
    {
        MainAPI.Capi.Gui.PlaySound("menubutton");
    }

    protected override void OnClicked()
    {
        MainAPI.Capi.Gui.PlaySound("menubutton_press");
    }

    public override void OnRender(float dt, ShaderGui shader)
    {
        Vector4 color = Vector4.One;

        if (state == EnumButtonState.Hovered)
        {
            color.Xyz *= 1.1f;
        }
        else if (state == EnumButtonState.Active)
        {
            color.Xyz *= 0.9f;
        }

        shader.Color = color;
        RenderTools.RenderNineSlice(VanillaThemes.InsetTexture, shader, X, Y, Width, Height);

        if (enabled)
        {
            color = VanillaThemes.BlueProgress;

            if (state == EnumButtonState.Hovered)
            {
                color.Xyz *= 1.1f;
            }
            else if (state == EnumButtonState.Active)
            {
                color.Xyz *= 0.9f;
            }

            shader.Color = color;
            RenderTools.RenderNineSlice(VanillaThemes.OutsetTexture, shader, X + (Width * 0.1f), Y + (Height * 0.1f), Width * 0.8f, Height * 0.8f);
        }

        shader.ResetColor();
    }
}