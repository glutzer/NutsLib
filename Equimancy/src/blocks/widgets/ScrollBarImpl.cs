using MareLib;

namespace Equimancy;

internal class ScrollBarImpl : BaseScrollBarWidget
{
    private readonly NineSliceTexture background;
    private readonly NineSliceTexture cursor;

    public ScrollBarImpl(Gui gui, Bounds bounds, Bounds scrollBounds, int stepsPerPage = 10) : base(gui, bounds, scrollBounds, stepsPerPage)
    {
        Texture bgTex = TextureBuilder.Begin(32, 32)
            .SetColor(SkiaThemes.Legendary)
            .StrokeMode(4)
            .DrawEmbossedOctagon(0, 0, 32, 32, 12, true)
            .End();

        background = new NineSliceTexture(bgTex, 15, 15);

        Texture cursorTex = TextureBuilder.Begin(32, 32)
            .SetColor(SkiaThemes.Poor)
            .DrawOctagon(0, 0, 32, 32, 12)
            .SetColor(SkiaThemes.Blue)
            .StrokeMode(4)
            .DrawEmbossedOctagon(0, 0, 32, 32, 12, false)
            .End();

        cursor = new NineSliceTexture(cursorTex, 15, 15);
    }

    protected override void RenderBackground(int x, int y, int width, int height, MareShader shader)
    {
        RenderTools.RenderNineSlice(background, shader, x, y, width, height);
    }

    protected override void RenderCursor(int x, int y, int width, int height, MareShader shader)
    {
        RenderTools.RenderNineSlice(cursor, shader, x, y, width, height);
    }

    public override void Dispose()
    {
        background.Dispose();
        cursor.Dispose();
    }
}
