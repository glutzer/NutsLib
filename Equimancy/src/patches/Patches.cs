using HarmonyLib;
using MareLib;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace Equimancy;

public class Patches
{
    // This is registered in the constructor, before patching.
    // If I could figure out how to unregister the event it's possible.
    [HarmonyPatch]
    public class MousePatch
    {
        public static MethodBase TargetMethod()
        {
            return typeof(ClientMain).Assembly.GetType("Vintagestory.Client.NoObf.HudMouseTools").GetMethod("RecheckItemInfo", BindingFlags.Instance | BindingFlags.NonPublic, new Type[] { typeof(float) });
        }

        [HarmonyPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }

    // Main entry-point for post-processing.
    //[HarmonyPatch(typeof(EntityPlayerShapeRenderer), "DoRender3DOpaqueBatched")]
    //public class RendererPatch
    //{
    //    public static Queue<EntityPlayerShapeRenderer> RenderQueue { get; } = new();
    //    private static UBORef animationUbo = null!;

    //    [HarmonyPrefix]
    //    public static bool DoRender3DOpaqueBatched(EntityPlayerShapeRenderer __instance, float dt, bool isShadowPass)
    //    {
    //        // Enqueue actual renderer.
    //        if (!isShadowPass)
    //        {
    //            RenderQueue.Enqueue(__instance);
    //            animationUbo = MainAPI.Capi.Render.CurrentActiveShader.UBOs["Animation"];
    //        }

    //        return false;
    //    }

    //    public static void RenderDistortion(MareShader oldShader, float dt)
    //    {
    //        MareShader distortion = MareShaderRegistry.Get("distortionanimated");
    //        distortion.Use();

    //        animationUbo.Bind();

    //        FrameBufferRef primary = RenderTools.GetFramebuffer(EnumFrameBuffer.Primary);

    //        distortion.BindTexture(primary.ColorTextureIds[0], "primary", 0);
    //        distortion.BindTexture(primary.DepthTextureId, "depth", 1);

    //        distortion.Uniform("time", MainAPI.Client.ElapsedMilliseconds / 1000f);
    //        distortion.Uniform("resolution", new Vector2(MainAPI.RenderWidth, MainAPI.RenderHeight));

    //        distortion.UniformMatrix("playerViewMatrix", MainAPI.Capi.Render.CameraMatrixOriginf);

    //        while (RenderQueue.Count > 0)
    //        {
    //            EntityPlayerShapeRenderer renderer = RenderQueue.Dequeue();

    //            distortion.Uniform("addRenderFlags", renderer.AddRenderFlags);

    //            distortion.UniformMatrix("modelMatrix", renderer.ModelMat);

    //            animationUbo.Update(renderer.entity.AnimManager.Animator.Matrices, 0, renderer.entity.AnimManager.Animator.MaxJointId * 16 * 4);

    //            bool IsSelf = renderer.GetProperty<bool>("IsSelf");
    //            IClientPlayer? player = renderer.GetField<EntityPlayer>("entityPlayer").Player as IClientPlayer;

    //            string meshCode = IsSelf && player?.CameraMode == EnumCameraMode.FirstPerson ? "firstPersonMeshRef" : "thirdPersonMeshRef";

    //            MultiTextureMeshRef meshRef = renderer.GetField<MultiTextureMeshRef>(meshCode);

    //            if (meshRef != null)
    //            {
    //                for (int i = 0; i < meshRef.meshrefs.Length; i++)
    //                {
    //                    MeshRef vao = meshRef.meshrefs[i];
    //                    MainAPI.Capi.Render.RenderMesh(vao);
    //                }
    //            }
    //        }

    //        animationUbo.Unbind();

    //        oldShader.Use();
    //    }
    //}
}