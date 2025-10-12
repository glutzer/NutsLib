using OpenTK.Mathematics;

namespace NutsLib;

public struct PositionVertex
{
    public Vector3 position;
    public Vector2 uv;

    public PositionVertex(Vector3 position, Vector2 uv)
    {
        this.position = position;
        this.uv = uv;
    }
}
