using System;
using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_BBox_
{
    public IntPtr xMin;
    public IntPtr yMin;
    public IntPtr xMax;
    public IntPtr yMax;
}