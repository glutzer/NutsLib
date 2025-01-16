using OpenTK.Mathematics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using Vintagestory.Common;

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

    // The two below don't actually work.

    /// <summary>
    /// Returns light for shading
    /// 0-23 - RGBs, 24-31, sunlight.
    /// </summary>
    public static int GetLightAsInt(Vector3d position)
    {
        WorldMap map = MainAPI.Client.WorldMap;
        Vector3i positionInt = (Vector3i)position;
        int index = ChunkMath.ChunkIndex3d(positionInt.X, positionInt.Y, positionInt.Z);

        IWorldChunk? chunk = map.GetChunkNonLocking(positionInt.X / 32, positionInt.Y / 32, positionInt.Z / 32);
        if (chunk == null) return ColorUtil.HsvToRgba(0, 0, 0, (int)(map.SunLightLevels[map.SunBrightness] * 255f));

        ChunkDataLayer? lightLayer = ((ChunkData)chunk.Data).lightLayer;
        if (lightLayer == null) return ColorUtil.HsvToRgba(0, 0, 0, (int)(map.SunLightLevels[map.SunBrightness] * 255f));

        uint lightData = (uint)lightLayer.Get(index);
        int lightSat = (int)((lightData >> 16) & 7);
        ushort light = (ushort)lightData;

        int sunlight = light & 0x1F;
        int blockLight = (light >> 5) & 0x1F;
        int hue = light >> 10;

        int a = (int)(map.SunLightLevels[sunlight] * 255f);
        byte h = map.hueLevels[hue];
        int s = map.satLevels[lightSat];
        int v = (int)(map.BlockLightLevels[blockLight] * 255f);

        // Test if this is in the same format...

        int ACTUAL_VALUE = ColorUtil.HsvToRgba(h, s, v, a);

        return ACTUAL_VALUE;
    }

    public static Vector4 GetLightAsVector4(Vector3d position)
    {
        WorldMap map = MainAPI.Client.WorldMap;
        Vector3i positionInt = (Vector3i)position;
        int index = ChunkMath.ChunkIndex3d(positionInt.X, positionInt.Y, positionInt.Z);

        IWorldChunk? chunk = map.GetChunkNonLocking(positionInt.X / 32, positionInt.Y / 32, positionInt.Z / 32);
        if (chunk == null) return Vector4.One;

        ChunkDataLayer? lightLayer = ((ChunkData)chunk.Data).lightLayer;
        if (lightLayer == null) return Vector4.One;

        uint lightData = (uint)lightLayer.Get(index);
        int lightSat = (int)((lightData >> 16) & 7);
        ushort light = (ushort)lightData;

        int sunlight = light & 0x1F;
        int blockLight = (light >> 5) & 0x1F;
        int hue = light >> 10;

        int a = (int)(map.SunLightLevels[sunlight] * 255f);
        byte h = map.hueLevels[hue];
        int s = map.satLevels[lightSat];
        int v = (int)(map.BlockLightLevels[blockLight] * 255f);

        int ACTUAL_VALUE = ColorUtil.HsvToRgba(h, s, v, a);

        Vector4 light2 = new((ACTUAL_VALUE & 0xFF) / 255f, ((ACTUAL_VALUE >> 8) & 0xFF) / 255f, ((ACTUAL_VALUE >> 16) & 0xFF) / 255f, ((ACTUAL_VALUE >> 24) & 0xFF) / 255f);

        return light2;
    }
}