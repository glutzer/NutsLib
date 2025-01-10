using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace Equimancy;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct BillboardParticleInstance
{
    // Position relative to base point.
    [FieldOffset(0)]
    public Vector3 position;
}