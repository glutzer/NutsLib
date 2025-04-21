using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FTC_ImageTypeRec_
{
    public void* face_id;
    public uint width;
    public uint height;
    public int flags;
}