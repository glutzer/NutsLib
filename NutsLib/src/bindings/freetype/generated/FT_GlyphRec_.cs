using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_GlyphRec_
{
    public FT_LibraryRec_* library;
    public FT_Glyph_Class_* clazz;
    public FT_Glyph_Format_ format;
    public FT_Vector_ advance;
}