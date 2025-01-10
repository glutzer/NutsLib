using OpenTK.Mathematics;

namespace Equimancy;

public struct FluidVertex
{
    public Vector3 position;
    public Vector2 uv;
    public Vector3 normal;
    public Vector4 color;
    public float fluidLevelOffset;

    public FluidVertex(Vector3 position, Vector2 uv, Vector3 normal, Vector4 color, float fluidLevelOffset)
    {
        this.position = position;
        this.uv = uv;
        this.normal = normal;
        this.color = color;
        this.fluidLevelOffset = fluidLevelOffset;
    }
}