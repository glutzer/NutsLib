using MareLib;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace Equimancy;

[GameSystem(forSide = EnumAppSide.Client)]
public class ItemInfoSystem : GameSystem
{
    protected NewMouseTools itemGui = null!;

    public ItemInfoSystem(bool isServer, ICoreAPI api) : base(isServer, api)
    {
    }

    public override void OnStart()
    {
        RemoveHudGui();

        itemGui = new NewMouseTools();
    }

    private static void RemoveHudGui()
    {
        GuiAPI guiApi = (GuiAPI)MainAPI.Capi.Gui;
        Type mouseTools = typeof(ClientMain).Assembly.GetType("Vintagestory.Client.NoObf.HudMouseTools")!;
        GuiDialog toRemove = null!;
        foreach (GuiDialog loadedGui in guiApi.LoadedGuis)
        {
            if (loadedGui.GetType() == mouseTools)
            {
                toRemove = loadedGui;
            }
        }
        guiApi.LoadedGuis.Remove(toRemove);
    }

    /// <summary>
    /// Register the widget in start after start order 0.
    /// Takes the main bounds, creates an widget.
    /// </summary>
    public void RegisterItemInfoWidget(System.Func<Bounds, IItemWidget> widgetFactory)
    {
        itemGui.itemWidgetFactories.Add(widgetFactory);
        itemGui.SetWidgets();
    }
}
