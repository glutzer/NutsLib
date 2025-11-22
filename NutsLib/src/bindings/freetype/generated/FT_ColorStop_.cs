using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_ColorStop_
{
    public IntPtr stop_offset;
    public FT_ColorIndex_ color;
}