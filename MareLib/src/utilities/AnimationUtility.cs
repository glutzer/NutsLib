using OpenTK.Mathematics;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace MareLib;

/// <summary>
/// Provides utilities for finding a world position and orientation from an attachment point.
/// </summary>
public static class AnimationUtility
{
    /// <summary>
    /// Gets a position of an attachment point using the player renderer.
    /// Must be after Opaque, 0.4 order for the model matrix to be loaded, else it will use it from the previous frame.
    /// In the model editor, north west bottom is 0, 0, 0, and is a block in size.
    /// </summary>
    public static void GetRightHandPosition(EntityPlayer player, Vector3 localPosition, out Vector3d position)
    {
        position = default;

        AttachmentPointAndPose? apap = player.AnimManager?.Animator?.GetAttachmentPointPose("RightHand");
        if (apap == null) return;

        if (player.Properties.Client.Renderer is not EntityPlayerShapeRenderer renderer) return;

        ItemSlot slot = player.RightHandItemSlot;
        if (slot.Itemstack == null) return;

        ItemRenderInfo renderInfo = MainAPI.Capi.Render.GetItemStackRenderInfo(slot, EnumItemRenderTarget.HandTp, 0);
        if (renderInfo?.Transform == null) return;

        AttachmentPoint ap = apap.AttachPoint;

        Matrix4 literalMat = Matrix4.CreateTranslation(-renderInfo.Transform.Origin.X, -renderInfo.Transform.Origin.Y, -renderInfo.Transform.Origin.Z)
             * Matrix4.CreateRotationZ((float)(ap.RotationZ + renderInfo.Transform.Rotation.Z) * GameMath.DEG2RAD)
             * Matrix4.CreateRotationY((float)(ap.RotationY + renderInfo.Transform.Rotation.Y) * GameMath.DEG2RAD)
             * Matrix4.CreateRotationX((float)(ap.RotationX + renderInfo.Transform.Rotation.X) * GameMath.DEG2RAD)
             * Matrix4.CreateTranslation(((float)ap.PosX / 16f) + renderInfo.Transform.Translation.X, ((float)ap.PosY / 16f) + renderInfo.Transform.Translation.Y, ((float)ap.PosZ / 16f) + renderInfo.Transform.Translation.Z)
             * Matrix4.CreateScale(renderInfo.Transform.ScaleXYZ.X, renderInfo.Transform.ScaleXYZ.Y, renderInfo.Transform.ScaleXYZ.Z)
             * Matrix4.CreateTranslation(renderInfo.Transform.Origin.X, renderInfo.Transform.Origin.Y, renderInfo.Transform.Origin.Z)
             * ConvertMatrix(apap.AnimModelMatrix)
             * ConvertMatrix(renderer.ModelMat);

        Vector4 pos = new Vector4(localPosition.X, localPosition.Y, localPosition.Z, 1f) * literalMat;

        float[]? pMatrixHandFov = renderer.GetField<float[]>("pMatrixHandFov");
        if (pMatrixHandFov != null) // Only for FP.
        {
            float[] pMatrixNormalFov = renderer.GetField<float[]>("pMatrixNormalFov");
            Matrix4 newOrigin = ConvertMatrix(MainAPI.Capi.Render.CameraMatrixOriginf);
            pos *= newOrigin * ConvertMatrix(pMatrixHandFov);
            pos *= ConvertMatrix(pMatrixNormalFov).Inverted() * newOrigin.Inverted();
        }

        position = new Vector3d(pos.X + player.CameraPos.X, pos.Y + player.CameraPos.Y, pos.Z + player.CameraPos.Z);
    }

    public static Matrix4 ConvertMatrix(Matrixf matrixF)
    {
        float[] matrixValues = matrixF.Values;

        return new Matrix4(
            matrixValues[0], matrixValues[1], matrixValues[2], matrixValues[3],
            matrixValues[4], matrixValues[5], matrixValues[6], matrixValues[7],
            matrixValues[8], matrixValues[9], matrixValues[10], matrixValues[11],
            matrixValues[12], matrixValues[13], matrixValues[14], matrixValues[15]
        );
    }

    public static Matrix4 ConvertMatrix(float[] matrixValues)
    {
        return new Matrix4(
            matrixValues[0], matrixValues[1], matrixValues[2], matrixValues[3],
            matrixValues[4], matrixValues[5], matrixValues[6], matrixValues[7],
            matrixValues[8], matrixValues[9], matrixValues[10], matrixValues[11],
            matrixValues[12], matrixValues[13], matrixValues[14], matrixValues[15]
        );
    }
}