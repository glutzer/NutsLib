using System;
using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_Glyph_Class_
{
    public IntPtr glyph_size;
    public FT_Glyph_Format_ glyph_format;
    public void* glyph_init;
    public void* glyph_done;
    public void* glyph_copy;
    public void* glyph_transform;
    public void* glyph_bbox;
    public void* glyph_prepare;
}