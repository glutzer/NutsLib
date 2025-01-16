using MareLib;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Equimancy;

public class NewMouseTools : Gui
{
    public override EnumDialogType DialogType => EnumDialogType.HUD;
    public override bool Focusable => false;
    public override double DrawOrder => 0.9;

    private ItemInfoWidget? mainWidget;

    public List<System.Func<Bounds, IItemWidget>> itemWidgetFactories = new();

    public NewMouseTools() : base()
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
        mainWidget?.SetPosition(mouseX, mouseY);
    }

    public override void PopulateWidgets(out Bounds mainBounds)
    {
        mainBounds = Bounds.Create().Fixed(0, 0, 0, 0);

        AddWidget(mainWidget = new ItemInfoWidget(this, mainBounds));

        foreach (System.Func<Bounds, IItemWidget> factory in itemWidgetFactories)
        {
            IItemWidget widget = factory(mainBounds);
            AddWidget((Widget)widget);
        }
    }

    public override bool OnMouseEnterSlot(ItemSlot slot)
    {
        foreach (IItemWidget widget in ForWidgets<IItemWidget>())
        {
            widget.SetItemStackData(slot);
        }

        return true;
    }

    public override bool OnMouseLeaveSlot(ItemSlot slot)
    {
        foreach (IItemWidget widget in ForWidgets<IItemWidget>())
        {
            widget.OnLeaveSlot(slot);
        }

        return true;
    }
}