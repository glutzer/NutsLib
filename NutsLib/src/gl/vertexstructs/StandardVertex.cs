using OpenTK.Mathematics;

namespace NutsLib;

public struct StandardVertex
{
    public Vector3 position;
    public Vector2 uv;
    public Vector3 normal;
    public Vector4 color;

    public StandardVertex(Vector3 position, Vector2 uv, Vector3 normal, Vector4 color)
    {
        this.position = position;
        this.uv = uv;
        this.normal = normal;
        this.color = color;
    }
}