using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace NutsLib;

public class OverlayPatch
{
    [HarmonyPatch(typeof(EntityBehaviorNameTag), "OnRenderFrame")]
    [HarmonyPatchCategory("core")]
    public class RendererPatch2D
    {
        [HarmonyPrefix]
        public static bool Prefix(EntityBehaviorNameTag __instance)
        {
            return !EntityOverlaySystem.EntityHasRenderingDisabled(__instance.entity);
        }
    }

    [HarmonyPatch(typeof(EntityShapeRenderer), "DoRender3DOpaqueBatched")]
    [HarmonyPatchCategory("core")]
    public class RendererPatch
    {
        /// <summary>
        /// Animation UBO captured by player renderer.
        /// </summary>
        public static UBORef? AnimationUbo { get; private set; }
        private static Action? after;

        [HarmonyPrefix]
        public static bool Prefix(EntityShapeRenderer __instance, bool isShadowPass)
        {
            if (isShadowPass && EntityOverlaySystem.EntityHasRenderingDisabled(__instance.entity))
            {
                return false;
            }

            // Enqueue actual renderer.
            if (!isShadowPass && EntityOverlaySystem.EntityHasHandler(__instance.entity))
            {
                Entity entity = __instance.entity;
                AnimationUbo = MainAPI.Capi.Render.CurrentActiveShader.UBOs["Animation"];

                after = () =>
                {
                    MultiTextureMeshRef? meshRef = __instance.GetField<MultiTextureMeshRef>("meshRefOpaque");
                    if (meshRef == null) return;

                    float[] modelMat = __instance.ModelMat;

                    IAnimator? animator = __instance.entity.AnimManager.Animator;
                    if (animator == null) return;

                    OverlayRenderInfo renderInfo = new(meshRef, animator, modelMat)
                    {
                        renderFlags = __instance.AddRenderFlags
                    };

                    EntityOverlaySystem.Instance?.ExecuteHandlers(entity, renderInfo);
                };

                if (EntityOverlaySystem.EntityHasRenderingDisabled(entity))
                {
                    return false;
                }
            }

            // Returning false here will remove all rendering.
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            if (after != null)
            {
                after();
                after = null;
            }
        }
    }
}