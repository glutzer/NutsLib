using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_MemoryRec_
{
    public void* user;
    public void* alloc;
    public void* free;
    public void* realloc;
}