using MareLib;
using OpenTK.Mathematics;
using System;

namespace Equimancy;

public class ButtonWidget : WidgetBaseButton
{
    private readonly NineSliceTexture texture;
    private readonly NineSliceTexture pressedTexture;
    private readonly NineSliceTexture hoveredTexture;
    private readonly TextObject textObject;

    public ButtonWidget(Gui gui, Bounds bounds, Action onClick, string text, int fontScale) : base(gui, bounds, onClick)
    {
        texture = EqGui.Button;
        pressedTexture = EqGui.ButtonPressed;
        hoveredTexture = EqGui.ButtonHovered;
        textObject = new TextObject(text, FontRegistry.GetFont("celestia"), fontScale, Vector4.One);
    }

    public override void OnRender(float dt, MareShader shader)
    {
        switch (state)
        {
            case EnumButtonState.Normal:
                RenderTools.RenderNineSlice(texture, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                break;
            case EnumButtonState.Active:
                RenderTools.RenderNineSlice(pressedTexture, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                break;
            case EnumButtonState.Hovered:
                RenderTools.RenderNineSlice(hoveredTexture, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                break;
        }

        // ???
        textObject.RenderCenteredLine(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2), shader, true);
    }
}