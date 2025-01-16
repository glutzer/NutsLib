using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Equimancy;

[StructLayout(LayoutKind.Explicit, Size = 64)]
public struct StandardParticleInstance
{
    // Position relative to base point.
    [FieldOffset(0)]
    public Vector3 position;

    [FieldOffset(16)]
    public Vector4 color;

    [FieldOffset(32)]
    public Vector4 light;

    [FieldOffset(48)]
    public Vector2 scaleRot;
}