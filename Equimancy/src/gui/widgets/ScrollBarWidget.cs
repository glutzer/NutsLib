using MareLib;

namespace Equimancy;

internal class ScrollBarWidget : BaseScrollBarWidget
{
    private readonly NineSliceTexture background;
    private readonly NineSliceTexture cursor;
    private readonly NineSliceTexture pressedCursor;
    private readonly NineSliceTexture hoveredCursor;

    public ScrollBarWidget(Gui gui, Bounds bounds, Bounds scrollBounds, int stepsPerPage = 10) : base(gui, bounds, scrollBounds, stepsPerPage)
    {
        background = EqGui.Box;
        cursor = EqGui.Button;
        pressedCursor = EqGui.ButtonPressed;
        hoveredCursor = EqGui.ButtonHovered;
    }

    protected override void RenderBackground(int x, int y, int width, int height, MareShader shader)
    {
        RenderTools.RenderNineSlice(background, shader, x, y, width, height);
    }

    protected override void RenderCursor(int x, int y, int width, int height, MareShader shader, ButtonState barState)
    {
        switch (barState)
        {
            case ButtonState.Normal:
                RenderTools.RenderNineSlice(cursor, shader, x, y, width, height);
                break;
            case ButtonState.Active:
                RenderTools.RenderNineSlice(pressedCursor, shader, x, y, width, height);
                break;
            case ButtonState.Hovered:
                RenderTools.RenderNineSlice(hoveredCursor, shader, x, y, width, height);
                break;
        }
    }
}
