using FreeTypeSharp;
using OpenTK.Graphics.OpenGL4;
using System.IO;
using System.Runtime.InteropServices;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;

namespace MareLib;

/// <summary>
/// Static font atlas.
/// </summary>
public static unsafe class DynamicFontAtlas
{
    public static Texture AtlasTexture { get; private set; } = null!;

    public static void Initialize()
    {
        // Red greyscale texture.
        AtlasTexture = Texture.CreateEmpty(2048, 2048, false, false, PixelInternalFormat.R8, PixelFormat.Red, PixelType.UnsignedByte);
    }

    /// <summary>
    /// Insert data into the font atlas.
    /// </summary>
    private static void InsertData(int x, int y, int width, int height, byte* data)
    {
        GL.BindTexture(TextureTarget.Texture2D, AtlasTexture.Handle);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, width, height, PixelFormat.Red, PixelType.UnsignedByte, (nint)data);
    }

    public static void LoadFontChar()
    {
        // Write to vintage story data folder.
        IAsset asset = MainAPI.Capi.Assets.Get("marelib:config/freetypefonts/friz.ttf");
        string dataPath = GamePaths.DataPath;
        string fontPath = Path.Combine(dataPath, "fonts", "friz.ttf");
        Directory.CreateDirectory(Path.GetDirectoryName(fontPath)!);
        File.WriteAllBytes(fontPath, asset.Data);

        // Init freetype.
        FT_LibraryRec_* lib;
        FT_FaceRec_* face;
        FT_Init_FreeType(&lib);
        FT_New_Face(lib, (byte*)Marshal.StringToHGlobalAnsi(fontPath), 0, &face);

        FT_Set_Char_Size(face, 0, 16 * 64, 300, 300);

        uint glyphIndex = FT_Get_Char_Index(face, 'F');

        FT_Load_Glyph(face, glyphIndex, FT_LOAD_DEFAULT);
        FT_Render_Glyph(face->glyph, FT_RENDER_MODE_SDF);

        uint bitmapWidth = face->glyph->bitmap.width;
        uint bitmapHeight = face->glyph->bitmap.rows;
        byte* bitmapData = face->glyph->bitmap.buffer;

        InsertData(0, 0, (int)bitmapWidth, (int)bitmapHeight, bitmapData);
    }

    public static void OnClosing()
    {
        AtlasTexture?.Dispose();
        AtlasTexture = null!;
    }
}