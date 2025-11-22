using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_StreamDesc_
{
    public IntPtr value;
    public void* pointer;
}