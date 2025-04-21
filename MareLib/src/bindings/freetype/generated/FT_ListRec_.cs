using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_ListRec_
{
    public FT_ListNodeRec_* head;
    public FT_ListNodeRec_* tail;
}