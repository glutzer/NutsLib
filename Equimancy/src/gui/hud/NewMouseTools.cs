using MareLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Equimancy;

public class NewMouseTools : Gui
{
    public override EnumDialogType DialogType => EnumDialogType.HUD;
    public override bool Focusable => false;
    public override double DrawOrder => 0.9;

    private ItemInfoWidget? itemInfoWidget;

    public NewMouseTools(ICoreClientAPI capi) : base(capi)
    {
        TryOpen();
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.BeforeRender += GuiEvents_BeforeRender;
    }

    /// <summary>
    /// Functions the same as a widget render.
    /// </summary>
    private void GuiEvents_BeforeRender(float dt)
    {
        MareShader guiShader = MareShaderRegistry.Get("gui");

        // Render the mouse stack.
        ItemSlot? playerMouseSlot = MainAPI.Capi.World.Player?.InventoryManager.GetOwnInventory("mouse")[0];
        if (playerMouseSlot == null) return;
        int mouseX = MainAPI.Capi.Input.MouseX;
        int mouseY = MainAPI.Capi.Input.MouseY;
        RenderTools.RenderItemStackToGui(playerMouseSlot, guiShader, mouseX, mouseY, 32, dt);

        // Move the fixed position of this widget to the mouse.
        itemInfoWidget?.SetPosition(mouseX, mouseY);
    }

    public override void PopulateWidgets(out Bounds mainBounds)
    {
        mainBounds = Bounds.Create().Fixed(0, 0, 0, 0);

        AddWidget(itemInfoWidget = new ItemInfoWidget(this, mainBounds));
    }

    public override bool OnMouseEnterSlot(ItemSlot slot)
    {
        itemInfoWidget?.OnEnterSlot(slot);
        return true;
    }

    public override bool OnMouseLeaveSlot(ItemSlot slot)
    {
        itemInfoWidget?.OnLeaveSlot(slot);
        return true;
    }
}