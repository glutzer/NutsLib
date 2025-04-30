using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_LayerIterator_
{
    public uint num_layers;
    public uint layer;
    public byte* p;
}