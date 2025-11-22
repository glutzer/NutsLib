using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_Parameter_
{
    public UIntPtr tag;
    public void* data;
}