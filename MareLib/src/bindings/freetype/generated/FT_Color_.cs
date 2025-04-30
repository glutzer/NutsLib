using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_Color_
{
    public byte blue;
    public byte green;
    public byte red;
    public byte alpha;
}