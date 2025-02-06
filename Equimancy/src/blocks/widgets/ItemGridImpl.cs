using MareLib;
using OpenTK.Mathematics;
using Vintagestory.API.Common;

namespace Equimancy;

public class ItemGridImpl : WidgetBaseItemGrid
{
    private readonly NineSliceTexture backgroundTexture;

    public ItemGridImpl(ItemSlot[] slots, int width, int height, int slotSize, Gui gui, Bounds bounds) : base(slots, width, height, slotSize, gui, bounds)
    {
        backgroundTexture = TextureBuilder.Begin(64, 64)
            .SetColor(SkiaThemes.Black.WithAlpha(100))
            .FillMode()
            .DrawRectangle(0, 0, 64, 64)
            .SetColor(SkiaThemes.Beige)
            .StrokeMode(8)
            .DrawEmbossedRectangle(0, 0, 64, 64, true)
            .End()
            .AsNineSlice(16, 16);
    }

    public override void RenderBackground(Vector2 start, int size, float dt, MareShader shader, ItemSlot slot)
    {
        RenderTools.RenderNineSlice(backgroundTexture, shader, start.X, start.Y, size, size);
    }

    public override void Dispose()
    {
        base.Dispose();
        backgroundTexture.Dispose();
    }
}