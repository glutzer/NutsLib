using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_Vector_
{
    public IntPtr x;
    public IntPtr y;
}