using FreeTypeSharp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;

namespace NutsLib;

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

public enum FontStatus
{
    Success,
    ZeroSize,
    NotFound
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
        "ptserif",
        "yaheibold"
    };

    private static TextureNode rootNode = null!;
    private static MeshHandle emptyHandle = null!;
    private static FreeTypeLibrary freetype = null!;

    public const int PADDING = 2;
    public const int FONT_SCALE = 64;

    public const float SDF_OFFSET = 0.125f; // Offset of the quad to deal with sdfs, because I don't know where freetype declares it.
    public const float CENTER_OFFSET = 0.3f; // Constant values for these, not from freetype.
    public const float LINEHEIGHT = 1f;

    private static int currentAtlasSize = 256;

    private static readonly List<MeshHandle> glyphMeshes = [];

    // Cache of all glyphs loaded.
    private static readonly Dictionary<string, GlyphRenderInfo> glyphCache = [];

    public static event Action? OnAtlasResize;

    private struct FTGlyphMeshInfo
    {
        public float width;
        public float height;
        public float advance;
        public float xBearing;
        public float yBearing;
    }

    public static void AssetsLoaded()
    {
        // Extract all ttf files.
        List<IAsset> assets = MainAPI.Capi.Assets.GetMany("config/freetypefonts/");
        string extractFolder = Path.Combine(GamePaths.DataPath, "ttf");
        if (!Directory.Exists(extractFolder)) Directory.CreateDirectory(extractFolder);
        foreach (IAsset asset in assets)
        {
            string fileName = asset.Name;
            if (!fileName.EndsWith(".ttf")) continue;
            File.WriteAllBytes(Path.Combine(extractFolder, asset.Name), asset.Data);
        }
    }

    public static void Initialize()
    {
        freetype = new FreeTypeLibrary();

        // Red greyscale texture.
        currentAtlasSize = 128;
        AtlasTexture = Texture.CreateEmpty(currentAtlasSize, currentAtlasSize, false, false, PixelInternalFormat.R8, PixelFormat.Red, PixelType.UnsignedByte);
        AtlasTexture.ClampToEdge();

        rootNode = new(new Vector2i(0, 0), new Vector2i(currentAtlasSize, currentAtlasSize));
        TextureNode emptyNode = rootNode.FindFirstSuitableNode(new Vector2i(32, 32))!;
        emptyHandle = CreateGlyphMesh(emptyNode, 0f, 0f, 0.01f, 0.01f);
        glyphMeshes.Add(emptyHandle);
    }

    /// <summary>
    /// Called when atlas is not large enough to hold fonts.
    /// </summary>
    private static void ResizeAtlas()
    {
        currentAtlasSize *= 2;

        AtlasTexture.Dispose();
        AtlasTexture = Texture.CreateEmpty(currentAtlasSize, currentAtlasSize, false, false, PixelInternalFormat.R8, PixelFormat.Red, PixelType.UnsignedByte);
        AtlasTexture.ClampToEdge();

        foreach (MeshHandle mesh in glyphMeshes) mesh.Dispose();
        glyphMeshes.Clear();
        glyphCache.Clear();

        rootNode = new(new Vector2i(0, 0), new Vector2i(currentAtlasSize, currentAtlasSize));
        TextureNode emptyNode = rootNode.FindFirstSuitableNode(new Vector2i(32, 32))!;
        emptyHandle = CreateGlyphMesh(emptyNode, 0f, 0f, 0.01f, 0.01f);
        glyphMeshes.Add(emptyHandle);

        OnAtlasResize?.Invoke();
    }

    public static void GetMetrics(string fontName, out float lineHeight, out float centerOffset)
    {
        string fontPath = Path.Combine(GamePaths.DataPath, "ttf", $"{fontName}.ttf");

        // Init face.
        FT_FaceRec_* face;
        FT_New_Face(freetype.Native, (byte*)Marshal.StringToHGlobalAnsi(fontPath), 0, &face);

        FT_Set_Pixel_Sizes(face, 0, FONT_SCALE);

        lineHeight = LINEHEIGHT;
        centerOffset = CENTER_OFFSET;

        FT_Done_Face(face);
    }

    /// <summary>
    /// Makes a new glyph mesh for a character, or returns a cached one.
    /// </summary>
    public static GlyphRenderInfo GetGlyphMesh(char character, string fontName)
    {
        if (glyphCache.TryGetValue($"{character}@{fontName}", out GlyphRenderInfo cachedInfo)) return cachedInfo;

        FontStatus status = LoadFontChar(fontName, character, out TextureNode? texNode, out FTGlyphMeshInfo glyphInfo);

        if (status == FontStatus.Success && texNode != null)
        {
            MeshHandle mesh = CreateGlyphMesh(texNode, glyphInfo.xBearing, glyphInfo.yBearing, glyphInfo.width, glyphInfo.height);
            glyphMeshes.Add(mesh);

            GlyphRenderInfo renderInfo = new(mesh.vaoId, glyphInfo.advance);
            glyphCache.Add($"{character}@{fontName}", renderInfo);

            return renderInfo;
        }

        if (status == FontStatus.ZeroSize)
        {
            return new GlyphRenderInfo(emptyHandle.vaoId, glyphInfo.advance);
        }

        if (status == FontStatus.NotFound)
        {
            foreach (string fallback in fallbackFonts)
            {
                if (glyphCache.TryGetValue($"{character}@{fallback}", out GlyphRenderInfo cachedFallbackInfo)) return cachedFallbackInfo;

                FontStatus fallbackStatus = LoadFontChar(fallback, character, out TextureNode? fallbackTexNode, out FTGlyphMeshInfo fallbackGlyphInfo);

                if (fallbackStatus == FontStatus.Success && fallbackTexNode != null)
                {
                    MeshHandle mesh = CreateGlyphMesh(fallbackTexNode, fallbackGlyphInfo.xBearing, fallbackGlyphInfo.yBearing, fallbackGlyphInfo.width, fallbackGlyphInfo.height);
                    glyphMeshes.Add(mesh);

                    GlyphRenderInfo renderInfo = new(mesh.vaoId, fallbackGlyphInfo.advance);
                    glyphCache.Add($"{character}@{fallback}", renderInfo);

                    return new GlyphRenderInfo(mesh.vaoId, glyphInfo.advance);
                }
            }
        }

        // Not found in foster, return empty.
        return new GlyphRenderInfo(emptyHandle.vaoId, glyphInfo.advance);
    }

    private static MeshHandle CreateGlyphMesh(TextureNode texNode, float xBearing, float yBearing, float width, float height)
    {
        float xStart = xBearing;
        float yStart = -yBearing;

        float uvStartX = (texNode.Origin.X + PADDING + 0.5f) / AtlasTexture.Width;
        float uvStartY = (texNode.Origin.Y + PADDING - 0.5f) / AtlasTexture.Height;

        float uvWidth = (texNode.Size.X - (PADDING * 2) - 1f) / AtlasTexture.Width;
        float uvHeight = (texNode.Size.Y - (PADDING * 2) - 1f) / AtlasTexture.Height;

        return QuadMeshUtility.CreateGuiQuadMesh(vertex =>
        {
            Vector3 position = new(xStart + (width * vertex.position.X), yStart + (height * vertex.position.Y), 0f);

            position.X -= SDF_OFFSET;
            position.Y -= SDF_OFFSET;

            Vector2 uv = new(uvStartX + (uvWidth * vertex.position.X), uvStartY + (uvHeight * vertex.position.Y));

            return new GuiVertex(position, uv, Vector4.One);
        });
    }

    /// <summary>
    /// Load a font char and return the node, returns false if the glyph doesn't exist, or is 0 size.
    /// </summary>
    private static FontStatus LoadFontChar(string faceName, char character, out TextureNode? texNode, out FTGlyphMeshInfo glyphInfo)
    {
        string fontPath = Path.Combine(GamePaths.DataPath, "ttf", $"{faceName}.ttf");

        // Init face.
        FT_FaceRec_* face;
        FT_New_Face(freetype.Native, (byte*)Marshal.StringToHGlobalAnsi(fontPath), 0, &face);

        FT_Set_Pixel_Sizes(face, 0, FONT_SCALE);

        uint glyphIndex = FT_Get_Char_Index(face, character);

        if (glyphIndex == 0)
        {
            texNode = null;
            glyphInfo = default;

            return FontStatus.NotFound;
        }

        FT_Load_Glyph(face, glyphIndex, FT_LOAD_DEFAULT);
        FT_Render_Glyph(face->glyph, FT_RENDER_MODE_SDF);

        uint bitmapWidth = face->glyph->bitmap.width;
        uint bitmapHeight = face->glyph->bitmap.rows;
        byte* bitmapData = face->glyph->bitmap.buffer;

        if (bitmapWidth == 0 || bitmapHeight == 0)
        {
            // Return an empty glyph, but with the advance.
            texNode = null;
            glyphInfo = default;

            glyphInfo.advance = (float)(face->glyph->metrics.horiAdvance.ToInt64() / (double)FONT_SCALE / 64.0);

            // Clean up freetype.
            FT_Done_Face(face);

            return FontStatus.ZeroSize;
        }

        texNode = InsertData((int)bitmapWidth, (int)bitmapHeight, bitmapData);
        glyphInfo.width = (float)(bitmapWidth / 64.0);
        glyphInfo.height = (float)(bitmapHeight / 64.0);

        // Bitshift 6 for values in pixels (uses 1/64).
        glyphInfo.xBearing = (float)(face->glyph->metrics.horiBearingX.ToInt64() / (double)FONT_SCALE / 64.0);
        glyphInfo.yBearing = (float)(face->glyph->metrics.horiBearingY.ToInt64() / (double)FONT_SCALE / 64.0);
        glyphInfo.advance = (float)(face->glyph->metrics.horiAdvance.ToInt64() / (double)FONT_SCALE / 64.0);

        // Clean up freetype.
        FT_Done_Face(face);

        return FontStatus.Success;
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
        TextureNode? node = rootNode.FindFirstSuitableNode(new Vector2i(width + (PADDING * 2), height + (PADDING * 2)));

        while (node == null)
        {
            ResizeAtlas();
            node = rootNode.FindFirstSuitableNode(new Vector2i(width + (PADDING * 2), height + (PADDING * 2)));
        }

        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

        GL.BindTexture(TextureTarget.Texture2D, AtlasTexture.Handle);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, node.Origin.X + PADDING, node.Origin.Y + PADDING, width, height, PixelFormat.Red, PixelType.UnsignedByte, (nint)data);

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

        glyphCache.Clear();

        OnAtlasResize = null;
    }
}