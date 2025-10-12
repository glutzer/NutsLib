using System;
using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_Glyph_Metrics_
{
    public IntPtr width;
    public IntPtr height;
    public IntPtr horiBearingX;
    public IntPtr horiBearingY;
    public IntPtr horiAdvance;
    public IntPtr vertBearingX;
    public IntPtr vertBearingY;
    public IntPtr vertAdvance;
}