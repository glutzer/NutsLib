using Vintagestory.Client.NoObf;

namespace MareLib;

public class TestElement : Widget
{
    public NineSliceTexture textureToRender;
    public Font font;
    public TextObject text;

    public TestElement(Gui gui, Bounds bounds) : base(gui, bounds)
    {
        textureToRender = Cache.GetOrCache($"testElementTextureScale{MainHook.GuiScale}", () =>
        {
            Texture tex = TextureBuilder.Begin(100, 100)
            .StrokeMode(20)
            .SetColor(0, 0, 1, 1)
            .DrawEmbossedOctagon(0, 0, 100, 100, 20, true)
            .End();

            return new NineSliceTexture(tex, 30, 30);
        });

        font = FontRegistry.GetFont("runic");

        text = new TextObject("Medium Fertility Soil", font, 40);
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {

    }

    float sizeIncrease = 0;

    public override void OnRender(float dt, ShaderProgram guiShader)
    {
        sizeIncrease += dt;

        RenderTools.BindTexture(textureToRender.texture, guiShader);
        RenderTools.RenderQuad(guiShader, bounds.X, bounds.Y, 128 * 4, 128 * 4);

        RenderTools.RenderNineSlice(textureToRender, guiShader, bounds.X + 200, bounds.Y, bounds.Width * sizeIncrease, bounds.Height * sizeIncrease);

        text.RenderLine(bounds.X, bounds.Y, guiShader, true);
    }

    public override void Dispose()
    {
        textureToRender.Dispose();
    }
}