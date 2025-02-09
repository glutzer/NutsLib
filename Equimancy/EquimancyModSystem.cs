using HarmonyLib;
using MareLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

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
        MareShaderRegistry.AddShader("marelib:gui", "equimancy:statusbar", "statusbar");
        _ = new HudStats();
    }

    public override void StartPre(ICoreAPI api)
    {
        Patch();
        AssetCategory.categories["fluidtypes"] = new AssetCategory("fluidtypes", true, EnumAppSide.Universal);
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
        EqGui.Dispose();
    }
}