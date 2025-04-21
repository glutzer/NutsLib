using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_ColorIndex_
{
    public ushort palette_index;
    public short alpha;
}