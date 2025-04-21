using FreeTypeSharp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Diagnostics;
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

/// <summary>
/// Static font atlas.
/// </summary>
public static unsafe class DynamicFontAtlas
{
    public static Texture AtlasTexture { get; private set; } = null!;
    private static TextureNode rootNode = null!;

    public static void Initialize()
    {
        // Red greyscale texture.
        AtlasTexture = Texture.CreateEmpty(2048, 2048, false, false, PixelInternalFormat.R8, PixelFormat.Red, PixelType.UnsignedByte);
        rootNode = new(new Vector2i(0, 0), new Vector2i(2048, 2048));

        for (int i = 32; i < 256; i++)
        {
            char iToChar = (char)i;
            LoadFontChar(iToChar);
        }

        PrintAtlasToPng("atlasdebug");
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

    public static void LoadFontChar(char character)
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

        FT_Set_Pixel_Sizes(face, 0, 48);

        uint glyphIndex = FT_Get_Char_Index(face, character);
        FT_Load_Glyph(face, glyphIndex, FT_LOAD_DEFAULT);
        FT_Render_Glyph(face->glyph, FT_RENDER_MODE_SDF);

        uint bitmapWidth = face->glyph->bitmap.width;
        uint bitmapHeight = face->glyph->bitmap.rows;
        byte* bitmapData = face->glyph->bitmap.buffer;

        if (bitmapWidth == 0)
        {
            int t = 1;
        }

        // Make white border around bitmap data.
        for (int i = 0; i < bitmapWidth; i++)
        {
            for (int j = 0; j < bitmapHeight; j++)
            {
                if (i == 0 || i == bitmapWidth - 1 || j == 0 || j == bitmapHeight - 1)
                {
                    bitmapData[i + (j * bitmapWidth)] = 255;
                }
            }
        }

        InsertData((int)bitmapWidth, (int)bitmapHeight, bitmapData);

        // Clean up freetype.
        FT_Done_Face(face);
        FT_Done_FreeType(lib);
    }

    public static void PrintAtlasToPng(string name)
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

    public static void OnClosing()
    {
        AtlasTexture?.Dispose();
        AtlasTexture = null!;

        rootNode = null!;
    }
}