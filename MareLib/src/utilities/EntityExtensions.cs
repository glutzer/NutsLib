using OpenTK.Mathematics;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.Client.NoObf;

namespace MareLib;

public static class EntityExtensions
{
    public static bool IsSelf(this Entity entity)
    {
        return entity is EntityPlayer player && player.PlayerUID == ClientSettings.PlayerUID;
    }

    public static Vector3d ToVector(this EntityPos pos)
    {
        return new Vector3d(pos.X, pos.Y, pos.Z);
    }
}