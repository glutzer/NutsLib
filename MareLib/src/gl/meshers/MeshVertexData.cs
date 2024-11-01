using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MareLib;

/// <summary>
/// Used by mesh generators to store vertices.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MeshVertexData
{
    public Vector3 position;
    public Vector2 uv;
    public Vector3 normal;

    public MeshVertexData(Vector3 position, Vector2 uv, Vector3 normal)
    {
        this.position = position;
        this.uv = uv;
        this.normal = normal;
    }

    public void RotateX(float radians)
    {
        Matrix3 matrix = Matrix3.CreateRotationX(radians);
        position *= matrix;
        normal *= matrix;
    }

    public void RotateY(float radians)
    {
        Matrix3 matrix = Matrix3.CreateRotationY(radians);
        position *= matrix;
        normal *= matrix;
    }

    public void RotateZ(float radians)
    {
        Matrix3 matrix = Matrix3.CreateRotationZ(radians);
        position *= matrix;
        normal *= matrix;
    }
}