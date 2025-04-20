using System;
using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_Matrix_
{
    public IntPtr xx;
    public IntPtr xy;
    public IntPtr yx;
    public IntPtr yy;
}