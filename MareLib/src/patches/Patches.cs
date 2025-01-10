using HarmonyLib;
using System.Reflection;
using Vintagestory.Client.NoObf;

namespace MareLib;

public class Patches
{
    /// <summary>
    /// Fix ordering of mouse so it doesn't break guis.
    /// </summary>
    [HarmonyPatch]
    [HarmonyPatchCategory("core")]
    public class MousePatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(ClientMain).Assembly.GetType("Vintagestory.Client.NoObf.HudDropItem").GetProperty("DrawOrder").GetGetMethod();
        }

        [HarmonyPrefix]
        public static bool Prefix(ref double __result)
        {
            __result = 0;
            return false;
        }
    }
}