using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_Opaque_Paint_
{
    public byte* p;
    public byte insert_root_transform;
}