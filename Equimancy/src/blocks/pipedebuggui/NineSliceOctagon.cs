namespace MareLib;

/// <summary>
/// Simple 9 slice octagon widget.
/// </summary>
public class NineSliceOctagon : Widget
{
    public NineSliceTexture textureToRender;

    public NineSliceOctagon(Gui gui, Bounds bounds) : base(gui, bounds)
    {
        textureToRender = ClientCache.GetOrCache($"testElementTextureScale{MainAPI.GuiScale}", () =>
        {
            Texture tex = TextureBuilder.Begin(100, 100)
            .StrokeMode(20)
            .SetColor(0, 0, 1, 1)
            .DrawEmbossedOctagon(0, 0, 100, 100, 20, true)
            .End();

            return new NineSliceTexture(tex, 30, 30);
        });
    }

    public override void OnRender(float dt, MareShader shader)
    {
        RenderTools.RenderNineSlice(textureToRender, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }
}