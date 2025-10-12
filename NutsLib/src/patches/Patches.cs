using HarmonyLib;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace NutsLib;

/// <summary>
/// Fix ordering of mouse so it doesn't break guis.
/// </summary>
[HarmonyPatch]
[HarmonyPatchCategory("core")]
public class MousePatch
{
    public static MethodBase TargetMethod()
    {
        return typeof(ClientMain).Assembly.GetType("Vintagestory.Client.NoObf.HudDropItem")!.GetProperty("DrawOrder")!.GetGetMethod()!;
    }

    [HarmonyPrefix]
    public static bool Prefix(ref double __result)
    {
        __result = 0;
        return false;
    }
}

[HarmonyPatch(typeof(EntityShapeRenderer), "RenderItem")]
[HarmonyPatchCategory("core")]
public class ItemRenderingPatch
{
    [HarmonyPrefix]
    public static bool Prefix(EntityShapeRenderer __instance, float dt, bool isShadowPass, ItemStack stack, AttachmentPointAndPose apap, ItemRenderInfo renderInfo)
    {
        return stack.Item is not IRenderableItem renderableItem || renderableItem.OnItemRender(__instance, dt, isShadowPass, stack, apap, renderInfo);
    }
}