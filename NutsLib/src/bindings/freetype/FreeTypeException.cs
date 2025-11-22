namespace FreeTypeSharp;

/// <summary>
/// Represents an exception thrown as a result of a FreeType2 API error.
/// </summary>
public sealed class FreeTypeException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FreeTypeException"/> class.
    /// </summary>
    public FreeTypeException(FT_Error err) : base(GetErrorString(err))
    {

    }

    /// <summary>
    /// Gets the error string for the specified error code.
    /// </summary>
    private static String GetErrorString(FT_Error err)
    {
        return err switch
        {
            FT_Error.FT_Err_Ok => "no error",
            FT_Error.FT_Err_Cannot_Open_Resource => "cannot open resource",
            FT_Error.FT_Err_Unknown_File_Format => "unknown file format",
            FT_Error.FT_Err_Invalid_File_Format => "broken file",
            FT_Error.FT_Err_Invalid_Version => "invalid FreeType version",
            FT_Error.FT_Err_Lower_Module_Version => "module version is too low",
            FT_Error.FT_Err_Invalid_Argument => "invalid argument",
            FT_Error.FT_Err_Unimplemented_Feature => "unimplemented feature",
            FT_Error.FT_Err_Invalid_Table => "broken table",
            FT_Error.FT_Err_Invalid_Offset => "broken offset within table",
            FT_Error.FT_Err_Array_Too_Large => "array allocation size too large",
            FT_Error.FT_Err_Missing_Module => "missing module",
            FT_Error.FT_Err_Missing_Property => "missing property",
            FT_Error.FT_Err_Invalid_Glyph_Index => "invalid glyph index",
            FT_Error.FT_Err_Invalid_Character_Code => "invalid character code",
            FT_Error.FT_Err_Invalid_Glyph_Format => "invalid glyph format",
            FT_Error.FT_Err_Cannot_Render_Glyph => "cannot render this glyph format",
            FT_Error.FT_Err_Invalid_Outline => "invalid outline",
            FT_Error.FT_Err_Invalid_Composite => "invalid composite glyph",
            FT_Error.FT_Err_Too_Many_Hints => "too many hints",
            FT_Error.FT_Err_Invalid_Pixel_Size => "invalid pixel size",
            FT_Error.FT_Err_Invalid_Handle => "invalid object handle",
            FT_Error.FT_Err_Invalid_Library_Handle => "invalid library handle",
            FT_Error.FT_Err_Invalid_Driver_Handle => "invalid module handle",
            FT_Error.FT_Err_Invalid_Face_Handle => "invalid face handle",
            FT_Error.FT_Err_Invalid_Size_Handle => "invalid size handle",
            FT_Error.FT_Err_Invalid_Slot_Handle => "invalid glyph slot handle",
            FT_Error.FT_Err_Invalid_CharMap_Handle => "invalid charmap handle",
            FT_Error.FT_Err_Invalid_Cache_Handle => "invalid cache manager handle",
            FT_Error.FT_Err_Invalid_Stream_Handle => "invalid stream handle",
            FT_Error.FT_Err_Too_Many_Drivers => "too many modules",
            FT_Error.FT_Err_Too_Many_Extensions => "too many extensions",
            FT_Error.FT_Err_Out_Of_Memory => "out of memory",
            FT_Error.FT_Err_Unlisted_Object => "unlisted object",
            FT_Error.FT_Err_Cannot_Open_Stream => "cannot open stream",
            FT_Error.FT_Err_Invalid_Stream_Seek => "invalid stream seek",
            FT_Error.FT_Err_Invalid_Stream_Skip => "invalid stream skip",
            FT_Error.FT_Err_Invalid_Stream_Read => "invalid stream read",
            FT_Error.FT_Err_Invalid_Stream_Operation => "invalid stream operation",
            FT_Error.FT_Err_Invalid_Frame_Operation => "invalid frame operation",
            FT_Error.FT_Err_Nested_Frame_Access => "nested frame access",
            FT_Error.FT_Err_Invalid_Frame_Read => "invalid frame read",
            FT_Error.FT_Err_Raster_Uninitialized => "raster uninitialized",
            FT_Error.FT_Err_Raster_Corrupted => "raster corrupted",
            FT_Error.FT_Err_Raster_Overflow => "raster overflow",
            FT_Error.FT_Err_Raster_Negative_Height => "negative height while rastering",
            FT_Error.FT_Err_Too_Many_Caches => "too many registered caches",
            FT_Error.FT_Err_Invalid_Opcode => "invalid opcode",
            FT_Error.FT_Err_Too_Few_Arguments => "too few arguments",
            FT_Error.FT_Err_Stack_Overflow => "stack overflow",
            FT_Error.FT_Err_Code_Overflow => "code overflow",
            FT_Error.FT_Err_Bad_Argument => "bad argument",
            FT_Error.FT_Err_Divide_By_Zero => "division by zero",
            FT_Error.FT_Err_Invalid_Reference => "invalid reference",
            FT_Error.FT_Err_Debug_OpCode => "found debug opcode",
            FT_Error.FT_Err_ENDF_In_Exec_Stream => "found ENDF opcode in execution stream",
            FT_Error.FT_Err_Nested_DEFS => "nested DEFS",
            FT_Error.FT_Err_Invalid_CodeRange => "invalid code range",
            FT_Error.FT_Err_Execution_Too_Long => "execution context too long",
            FT_Error.FT_Err_Too_Many_Function_Defs => "too many function definitions",
            FT_Error.FT_Err_Too_Many_Instruction_Defs => "too many instruction definitions",
            FT_Error.FT_Err_Table_Missing => "SFNT font table missing",
            FT_Error.FT_Err_Horiz_Header_Missing => "horizontal header (hhea) table missing",
            FT_Error.FT_Err_Locations_Missing => "locations (loca) table missing",
            FT_Error.FT_Err_Name_Table_Missing => "name table missing",
            FT_Error.FT_Err_CMap_Table_Missing => "character map (cmap) table missing",
            FT_Error.FT_Err_Hmtx_Table_Missing => "horizontal metrics (hmtx) table missing",
            FT_Error.FT_Err_Post_Table_Missing => "PostScript (post) table missing",
            FT_Error.FT_Err_Invalid_Horiz_Metrics => "invalid horizontal metrics",
            FT_Error.FT_Err_Invalid_CharMap_Format => "invalid character map (cmap) format",
            FT_Error.FT_Err_Invalid_PPem => "invalid ppem value",
            FT_Error.FT_Err_Invalid_Vert_Metrics => "invalid vertical metrics",
            FT_Error.FT_Err_Could_Not_Find_Context => "could not find context",
            FT_Error.FT_Err_Invalid_Post_Table_Format => "invalid PostScript (post) table format",
            FT_Error.FT_Err_Invalid_Post_Table => "invalid PostScript (post) table",
            FT_Error.FT_Err_DEF_In_Glyf_Bytecode => "found FDEF or IDEF opcode in glyf bytecode",
            FT_Error.FT_Err_Missing_Bitmap => "missing bitmap in strike",
            FT_Error.FT_Err_Syntax_Error => "opcode syntax error",
            FT_Error.FT_Err_Stack_Underflow => "argument stack underflow",
            FT_Error.FT_Err_Ignore => "ignore",
            FT_Error.FT_Err_No_Unicode_Glyph_Name => "no Unicode glyph name found",
            FT_Error.FT_Err_Glyph_Too_Big => "glyph too big for hinting",
            FT_Error.FT_Err_Missing_Startfont_Field => "`STARTFONT' field missing",
            FT_Error.FT_Err_Missing_Font_Field => "`FONT' field missing",
            FT_Error.FT_Err_Missing_Size_Field => "`SIZE' field missing",
            FT_Error.FT_Err_Missing_Fontboundingbox_Field => "`FONTBOUNDINGBOX' field missing",
            FT_Error.FT_Err_Missing_Chars_Field => "`CHARS' field missing",
            FT_Error.FT_Err_Missing_Startchar_Field => "`STARTCHAR' field missing",
            FT_Error.FT_Err_Missing_Encoding_Field => "`ENCODING' field missing",
            FT_Error.FT_Err_Missing_Bbx_Field => "`BBX' field missing",
            FT_Error.FT_Err_Bbx_Too_Big => "`BBX' too big",
            FT_Error.FT_Err_Corrupted_Font_Header => "Font header corrupted or missing fields",
            FT_Error.FT_Err_Corrupted_Font_Glyphs => "Font glyphs corrupted or missing fields",
            _ => "unknown error",
        };
    }
}