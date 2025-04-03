using OpenTK.Mathematics;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace MareLib;

public static class LightingUtilities
{
    /// <summary>
    /// Add a point light for this frame.
    /// </summary>
    public static void AddPointLight(Vector3d position, Vector3 rgb)
    {
        int pointLightsCount = MainAPI.Client.shUniforms.PointLightsCount;
        if (pointLightsCount < ClientSettings.MaxDynamicLights)
        {
            Vector3 worldTransform = RenderTools.CameraRelativePosition(position);
            Vector3 transformedPosition = (new Vector4(worldTransform, 1f) * MainAPI.OriginViewMatrix).Xyz;

            MainAPI.Client.shUniforms.PointLights3[3 * pointLightsCount] = transformedPosition.X;
            MainAPI.Client.shUniforms.PointLights3[(3 * pointLightsCount) + 1] = transformedPosition.Y;
            MainAPI.Client.shUniforms.PointLights3[(3 * pointLightsCount) + 2] = transformedPosition.Z;

            MainAPI.Client.shUniforms.PointLightColors3[3 * pointLightsCount] = rgb.Z;
            MainAPI.Client.shUniforms.PointLightColors3[(3 * pointLightsCount) + 1] = rgb.Y;
            MainAPI.Client.shUniforms.PointLightColors3[(3 * pointLightsCount) + 2] = rgb.X;
            MainAPI.Client.shUniforms.PointLightsCount++;
        }
    }

    /// <summary>
    /// Add a point light for this frame.
    /// </summary>
    public static void AddPointLight(byte[] lightHSV, Vector3d position)
    {
        Vec3f rgb = new();
        byte h = MainAPI.Client.WorldMap.hueLevels[lightHSV[0]];
        int s = MainAPI.Client.WorldMap.satLevels[lightHSV[1]];
        int v = (int)(MainAPI.Client.WorldMap.BlockLightLevels[lightHSV[2]] * 255f);
        ColorUtil.ToRGBVec3f(ColorUtil.HsvToRgba(h, s, v), ref rgb);

        int pointLightsCount = MainAPI.Client.shUniforms.PointLightsCount;
        if (pointLightsCount < ClientSettings.MaxDynamicLights)
        {
            Vector3 worldTransform = RenderTools.CameraRelativePosition(position);
            Vector3 transformedPosition = (new Vector4(worldTransform, 1f) * MainAPI.OriginViewMatrix).Xyz;

            MainAPI.Client.shUniforms.PointLights3[3 * pointLightsCount] = transformedPosition.X;
            MainAPI.Client.shUniforms.PointLights3[(3 * pointLightsCount) + 1] = transformedPosition.Y;
            MainAPI.Client.shUniforms.PointLights3[(3 * pointLightsCount) + 2] = transformedPosition.Z;

            MainAPI.Client.shUniforms.PointLightColors3[3 * pointLightsCount] = rgb.Z;
            MainAPI.Client.shUniforms.PointLightColors3[(3 * pointLightsCount) + 1] = rgb.Y;
            MainAPI.Client.shUniforms.PointLightColors3[(3 * pointLightsCount) + 2] = rgb.X;
            MainAPI.Client.shUniforms.PointLightsCount++;
        }
    }
}