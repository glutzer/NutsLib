using System;
using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_Size_Metrics_
{
    public ushort x_ppem;
    public ushort y_ppem;
    public IntPtr x_scale;
    public IntPtr y_scale;
    public IntPtr ascender;
    public IntPtr descender;
    public IntPtr height;
    public IntPtr max_advance;
}