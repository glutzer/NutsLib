using FreeTypeSharp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;

namespace MareLib;

public class TextureNode
{
    public Vector2i Origin { get; private set; }
    public Vector2i Size { get; private set; }

    /// <summary>
    /// Node has no children, has not been split.
    /// </summary>
    public bool IsLeaf { get; private set; }

    public TextureNode? Left { get; private set; }
    public TextureNode? Right { get; private set; }

    public TextureNode(Vector2i origin, Vector2i size)
    {
        Origin = origin;
        Size = size;
    }

    /// <summary>
    /// Mark this node as being filled by a texture.
    /// </summary>
    public void MarkUsed()
    {
        IsLeaf = true;
    }

    /// <summary>
    /// Inserts a node into this one, only if empty.
    /// </summary>
    public TextureNode InsertNode(Vector2i textureSize)
    {
        int remainingX = Size.X - textureSize.X;
        int remainingY = Size.Y - textureSize.Y;

        // Can't split further.
        if (remainingX == 0 && remainingY == 0)
        {
            MarkUsed();
            return this;
        }

        bool verticalSplit = remainingX < remainingY;

        if (verticalSplit)
        {
            // First split vertically.

            // Top.
            Left = new TextureNode(Origin, new Vector2i(Size.X, textureSize.Y));

            // Bottom.
            Right = new TextureNode(
                new Vector2i(Origin.X, Origin.Y + textureSize.Y),
                new Vector2i(Size.X, Size.Y - textureSize.Y)
            );

            // This full fits the texture into the top part, that can be a leaf node.
            if (remainingX == 0)
            {
                Left.MarkUsed();
                return Left;
            }

            return Left.InsertNode(textureSize);
        }
        else
        {
            // First split horizontally.

            // Left.
            Left = new TextureNode(Origin, new Vector2i(textureSize.X, Size.Y));

            // Right.
            Right = new TextureNode(
                new Vector2i(Origin.X + textureSize.X, Origin.Y),
                new Vector2i(Size.X - textureSize.X, Size.Y)
            );

            // This full fits the texture into the left part, that can be a leaf node.
            if (remainingY == 0)
            {
                Left.MarkUsed();
                return Left;
            }

            return Left.InsertNode(textureSize);
        }
    }

    /// <summary>
    /// Call from the root, finds the first suitable node for the given size.
    /// </summary>
    public TextureNode? FindFirstSuitableNode(Vector2i size)
    {
        // Too large.
        if (size.X > Size.X || size.Y > Size.Y) return null;

        // Node has not had children set, can insert a node.
        if (Left == null && Right == null)
        {
            return InsertNode(size);
        }

        Debug.Assert(Left != null && Right != null);

        if (!Left.IsLeaf)
        {
            TextureNode? node = Left.FindFirstSuitableNode(size);
            if (node != null) return node;
        }

        if (!Right.IsLeaf)
        {
            TextureNode? node = Right.FindFirstSuitableNode(size);
            if (node != null) return node;
        }

        // Unable to fit into either child.
        return null;
    }
}

public struct GlyphRenderInfo
{
    public int vaoId;
    public float xAdvance;

    public GlyphRenderInfo(int vaoId, float xAdvance)
    {
        this.vaoId = vaoId;
        this.xAdvance = xAdvance;
    }
}

/// <summary>
/// Static font atlas.
/// </summary>
public static unsafe class DynamicFontAtlas
{
    public static Texture AtlasTexture { get; private set; } = null!;

    // Fonts in order that will be looked through for fallback glyphs.
    private static readonly string[] fallbackFonts = new string[]
    {
        "arial"
    };

    private static TextureNode rootNode = null!;
    private static MeshHandle emptyHandle = null!;
    private static FreeTypeLibrary freetype = null!;

    public const int FONT_SCALE = 64;

    private static readonly List<MeshHandle> glyphMeshes = new();

    private struct FTGlyphMeshInfo
    {
        public float width;
        public float height;
        public float advance;
        public float xBearing;
        public float yBearing;
    }

    public static void Initialize()
    {
        freetype = new FreeTypeLibrary();

        // Red greyscale texture.
        AtlasTexture = Texture.CreateEmpty(2048, 2048, false, false, PixelInternalFormat.R8, PixelFormat.Red, PixelType.UnsignedByte);
        rootNode = new(new Vector2i(0, 0), new Vector2i(2048, 2048));

        TextureNode emptyNode = rootNode.FindFirstSuitableNode(new Vector2i(32, 32))!;
        emptyHandle = CreateGlyphMesh(emptyNode, 0, 0, 32, 32);
        glyphMeshes.Add(emptyHandle);

















        for (int i = 34; i < 256; i++)
        {
            char iToChar = (char)i;
            LoadFontChar(iToChar, out TextureNode? texNode, out FTGlyphMeshInfo glyphInfo);
        }
    }

    /// <summary>
    /// Makes a new glyph mesh for a character.
    /// </summary>
    public static GlyphRenderInfo GetGlyphMesh(char character)
    {
        if (LoadFontChar(character, out TextureNode? texNode, out FTGlyphMeshInfo glyphInfo))
        {
            MeshHandle mesh = CreateGlyphMesh(texNode, glyphInfo.xBearing, glyphInfo.yBearing, glyphInfo.width, glyphInfo.height);
            glyphMeshes.Add(mesh);

            return new GlyphRenderInfo(mesh.vaoId, glyphInfo.advance);
        }

        // All invisible glyphs have 0.2f? But spaces do vary between fonts.
        return new GlyphRenderInfo(emptyHandle.vaoId, 0.2f);
    }

    private static MeshHandle CreateGlyphMesh(TextureNode texNode, float xBearing, float yBearing, float width, float height)
    {
        float xStart = xBearing;
        float yStart = -yBearing;

        float uvStartX = (texNode.Origin.X + 0.5f) / AtlasTexture.Width;
        float uvStartY = (texNode.Origin.Y + 0.5f) / AtlasTexture.Height;

        float uvWidth = (texNode.Size.X - 1) / AtlasTexture.Width;
        float uvHeight = (texNode.Size.Y - 1) / AtlasTexture.Height;

        return QuadMeshUtility.CreateGuiQuadMesh(vertex =>
        {
            Vector3 position = new(xStart + (width * vertex.position.X), yStart + (yBearing * vertex.position.Y), 0f);
            Vector2 uv = new(uvStartX + (uvWidth * vertex.uv.X), uvStartY - (uvHeight * vertex.uv.Y));

            return new GuiVertex(position, uv);
        });
    }

    /// <summary>
    /// Load a font char and return the node, returns false if it doesn't exist.
    /// </summary>
    private static bool LoadFontChar(char character, [NotNullWhen(true)] out TextureNode? texNode, out FTGlyphMeshInfo glyphInfo)
    {
        // Write to vintage story data folder.
        IAsset asset = MainAPI.Capi.Assets.Get("marelib:config/freetypefonts/friz.ttf");
        string dataPath = GamePaths.DataPath;
        string fontPath = Path.Combine(dataPath, "fontscache", "friz.ttf");
        Directory.CreateDirectory(Path.GetDirectoryName(fontPath)!);
        File.WriteAllBytes(fontPath, asset.Data);

        // Init face.
        FT_FaceRec_* face;
        FT_New_Face(freetype.Native, (byte*)Marshal.StringToHGlobalAnsi(fontPath), 0, &face);

        FT_Set_Pixel_Sizes(face, 0, FONT_SCALE);

        uint glyphIndex = FT_Get_Char_Index(face, character);
        FT_Load_Glyph(face, glyphIndex, FT_LOAD_DEFAULT);
        FT_Render_Glyph(face->glyph, FT_RENDER_MODE_SDF);

        uint bitmapWidth = face->glyph->bitmap.width;
        uint bitmapHeight = face->glyph->bitmap.rows;
        byte* bitmapData = face->glyph->bitmap.buffer;

        if (bitmapWidth == 0 || bitmapHeight == 0)
        {
            // Return an empty glyph.
            texNode = null;
            glyphInfo = default;

            // Clean up freetype.
            FT_Done_Face(face);

            return false;
        }



        texNode = InsertData((int)bitmapWidth, (int)bitmapHeight, bitmapData);
        glyphInfo.width = (int)(bitmapWidth / 64.0);
        glyphInfo.height = (int)(bitmapHeight / 64.0);

        // Bitshift 6 for values in pixels (uses 1/64).
        glyphInfo.xBearing = (float)(face->glyph->metrics.horiBearingX.ToInt64() / (double)FONT_SCALE / 64.0);
        glyphInfo.yBearing = (float)(face->glyph->metrics.horiBearingY.ToInt64() / (double)FONT_SCALE / 64.0);
        glyphInfo.advance = (float)(face->glyph->metrics.horiAdvance.ToInt64() / (double)FONT_SCALE / 64.0);

        // Clean up freetype.
        FT_Done_Face(face);

        return true;
    }

    private static void PrintAtlasToPng(string name)
    {
        byte[] atlasData = new byte[AtlasTexture.Width * AtlasTexture.Height];
        GL.BindTexture(TextureTarget.Texture2D, AtlasTexture.Handle);
        GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Red, PixelType.UnsignedByte, atlasData);

        using SKBitmap bitmap = new(new SKImageInfo(AtlasTexture.Width, AtlasTexture.Height, SKColorType.Gray8, SKAlphaType.Opaque));

        IntPtr ptr = bitmap.GetPixels();
        Marshal.Copy(atlasData, 0, ptr, atlasData.Length);

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        using FileStream stream = File.OpenWrite($"{name}.png");
        data.SaveTo(stream);
    }

    /// <summary>
    /// Insert data into the font atlas.
    /// </summary>
    private static TextureNode InsertData(int width, int height, byte* data)
    {
        TextureNode? node = rootNode.FindFirstSuitableNode(new Vector2i(width, height)) ?? throw new Exception("Node needs to have resizing implemented, filled up atlas!");

        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

        GL.BindTexture(TextureTarget.Texture2D, AtlasTexture.Handle);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, node.Origin.X, node.Origin.Y, width, height, PixelFormat.Red, PixelType.UnsignedByte, (nint)data);

        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);

        return node;
    }

    public static void OnClosing()
    {
        AtlasTexture?.Dispose();
        AtlasTexture = null!;

        rootNode = null!;

        emptyHandle = null!;

        // Dispose all glyph meshes.
        foreach (MeshHandle mesh in glyphMeshes)
        {
            mesh.Dispose();
        }
        glyphMeshes.Clear();

        freetype?.Dispose();
        freetype = null!;
    }
}

// Make white border around bitmap data.
//for (int i = 0; i < bitmapWidth; i++)
//{
//    for (int j = 0; j < bitmapHeight; j++)
//    {
//        if (i == 0 || i == bitmapWidth - 1 || j == 0 || j == bitmapHeight - 1)
//        {
//            bitmapData[i + (j * bitmapWidth)] = 255;
//        }
//    }
//}