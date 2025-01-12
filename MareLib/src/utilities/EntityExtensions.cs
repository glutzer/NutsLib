using OpenTK.Mathematics;
using Vintagestory.API.Common.Entities;

namespace MareLib;

public static class EntityExtensions
{
    public static Vector3d ToVector(this EntityPos pos)
    {
        return new Vector3d(pos.X, pos.Y, pos.Z);
    }
}