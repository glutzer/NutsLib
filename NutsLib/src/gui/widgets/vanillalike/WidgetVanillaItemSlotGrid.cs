using OpenTK.Mathematics;
using Vintagestory.API.Common;

namespace NutsLib;

public class WidgetVanillaItemSlotGrid : WidgetBaseItemGrid
{
    public NineSliceTexture slotTex = VanillaThemes.ItemSlotTexture;
    private readonly TextObject slotNumber = new("", VanillaThemes.Font, 1f, VanillaThemes.WhitishTextColor);

    public WidgetVanillaItemSlotGrid(ItemSlot[] slots, int width, int height, int slotSize, Widget? parent, Gui gui) : base(slots, width, height, slotSize, parent, gui)
    {
        slotNumber.Shadow = true;
    }

    public override void RenderBackground(Vector2 start, int size, float dt, ShaderGui shader, ItemSlot slot, int slotIndex)
    {
        RenderTools.RenderNineSlice(slotTex, shader, start.X, start.Y, size, size);
    }

    public override void RenderOverlay(Vector2 start, int size, float dt, ShaderGui shader, ItemSlot slot, int slotIndex)
    {
        if (slot.Itemstack == null || slot.Itemstack.Collectible.MaxStackSize == 1) return;
        int slotCount = slot.Itemstack.StackSize;

        slotNumber.Text = slotCount.ToString();
        slotNumber.SetScaleFromWidth(size / 2f, size / 2f, 1f, 0.8f);
        slotNumber.RenderCenteredLine(start.X + (size * 0.75f), start.Y + (size * 0.25f), shader, true);
    }
}