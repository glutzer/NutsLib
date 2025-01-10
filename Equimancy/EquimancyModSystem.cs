using HarmonyLib;
using MareLib;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.Client.NoObf;

namespace Equimancy;

public class EquimancyModSystem : ModSystem
{
    private static Harmony? Harmony { get; set; }

    public override double ExecuteOrder()
    {
        return 0.3;
    }

    public override void Start(ICoreAPI api)
    {

    }

    public override void StartServerSide(ICoreServerAPI api)
    {

    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        MareShaderRegistry.AddShader("equimancy:oitdebug", "equimancy:oitdebug", "oitdebug");

        RemoveHudGui();

        _ = new NewMouseTools(api);

        MareShaderRegistry.AddShader("marelib:gui", "equimancy:statusbar", "statusbar");
        _ = new HudStats(api);
    }

    public override void StartPre(ICoreAPI api)
    {
        Patch();
    }

    public static void Patch()
    {
        if (Harmony != null) return;

        Harmony = new Harmony("equimancy");
        Harmony.PatchAll();
    }

    public static void Unpatch()
    {
        if (Harmony == null) return;

        Harmony.UnpatchAll("equimancy");
        Harmony = null;
    }

    public override void Dispose()
    {
        Unpatch();
    }

    public static void RemoveHudGui()
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
}