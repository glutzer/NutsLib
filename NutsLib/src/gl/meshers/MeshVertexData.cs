using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace NutsLib;

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

    public void RotateX(float radians, bool round = true)
    {
        Matrix3 matrix = Matrix3.CreateRotationX(radians);
        position *= matrix;
        normal *= matrix;

        if (round)
        {
            position = new Vector3(MathF.Round(position.X, 6), MathF.Round(position.Y, 6), MathF.Round(position.Z, 6));
            normal = new Vector3(MathF.Round(normal.X, 6), MathF.Round(normal.Y, 6), MathF.Round(normal.Z, 6));
        }
    }

    public void RotateY(float radians, bool round = true)
    {
        Matrix3 matrix = Matrix3.CreateRotationY(radians);
        position *= matrix;
        normal *= matrix;

        if (round)
        {
            position = new Vector3(MathF.Round(position.X, 6), MathF.Round(position.Y, 6), MathF.Round(position.Z, 6));
            normal = new Vector3(MathF.Round(normal.X, 6), MathF.Round(normal.Y, 6), MathF.Round(normal.Z, 6));
        }
    }

    public void RotateZ(float radians, bool round = true)
    {
        Matrix3 matrix = Matrix3.CreateRotationZ(radians);
        position *= matrix;
        normal *= matrix;

        if (round)
        {
            position = new Vector3(MathF.Round(position.X, 6), MathF.Round(position.Y, 6), MathF.Round(position.Z, 6));
            normal = new Vector3(MathF.Round(normal.X, 6), MathF.Round(normal.Y, 6), MathF.Round(normal.Z, 6));
        }
    }
}