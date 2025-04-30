using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MareLib;

/// <summary>
/// Used as a global ubo.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 256)]
public struct RenderGlobals
{
    [FieldOffset(0)]
    public Matrix4 originViewMatrix;

    [FieldOffset(64)]
    public Matrix4 perspectiveMatrix;

    [FieldOffset(128)]
    public Matrix4 orthographicMatrix;

    [FieldOffset(192)]
    public Matrix4 perspectiveViewMatrix;

    [FieldOffset(256)]
    public float zNear;

    [FieldOffset(260)]
    public float zFar;

    public RenderGlobals(Matrix4 originViewMatrix, Matrix4 perspectiveMatrix, Matrix4 orthographicMatrix)
    {
        this.originViewMatrix = originViewMatrix;
        this.perspectiveMatrix = perspectiveMatrix;
        this.orthographicMatrix = orthographicMatrix;
        perspectiveViewMatrix = originViewMatrix * perspectiveMatrix;
    }
}