using OpenTK.Graphics.OpenGL4;
using SkiaSharp;
using System;
using Vintagestory.API.Common;

namespace MareLib;

public unsafe class Texture : IDisposable
{
    public int Handle { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    /// <summary>
    /// Takes full asset location.
    /// </summary>
    public static Texture Create(string assetPath, bool aliased = true, bool mipmaps = false)
    {
        IAsset? textureAsset = MainAPI.Capi.Assets.Get(new AssetLocation(assetPath)) ?? throw new Exception($"Texture asset not found: {assetPath}!");
        byte[] pngData = textureAsset.Data;

        return Create(pngData, aliased, mipmaps);
    }

    public static Texture Create(byte[] pngData, bool aliased = true, bool mipmaps = false)
    {
        SKBitmap bmp = SKBitmap.Decode(pngData);
        return Create(bmp, aliased, mipmaps);
    }

    public static Texture Create(SKBitmap bitmap, bool aliased = true, bool mipmaps = false)
    {
        int textureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bitmap.Pixels);

        SetAliasing(aliased, mipmaps, TextureTarget.Texture2D);
        if (mipmaps) SetMipmaps(GetMaxMipmaps(bitmap.Width, bitmap.Height), TextureTarget.Texture2D);

        Texture texture = new()
        {
            Handle = textureHandle,
            Width = bitmap.Width,
            Height = bitmap.Height
        };

        return texture;
    }

    /// <summary>
    /// Create an empty texture for something like a font atlas.
    /// </summary>
    public static Texture CreateEmpty(
        int width,
        int height,
        bool aliased = true,
        bool mipmaps = false,
        PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
        PixelFormat format = PixelFormat.Bgra,
        PixelType pixelType = PixelType.UnsignedByte)
    {
        int textureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);

        GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, pixelType, IntPtr.Zero);

        // Create an empty array of pixel data.
        byte[] emptyData = new byte[width * height * 4];
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, format, pixelType, emptyData);

        SetAliasing(aliased, mipmaps, TextureTarget.Texture2D);
        if (mipmaps) SetMipmaps(GetMaxMipmaps(width, height), TextureTarget.Texture2D);

        Texture texture = new()
        {
            Handle = textureHandle,
            Width = width,
            Height = height
        };

        return texture;
    }

    /// <summary>
    /// Update a part of a texture.
    /// </summary>
    public void UpdatePartial(int x, int y, int width, int height, byte[] pngData, bool updateMipmaps = false)
    {
        SKBitmap bmp = SKBitmap.Decode(pngData);
        UpdatePartial(x, y, width, height, bmp, updateMipmaps);
    }

    /// <summary>
    /// Update a part of a texture.
    /// </summary>
    public void UpdatePartial(int x, int y, int width, int height, SKBitmap bmp, bool updateMipmaps = false)
    {
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, x, y, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, bmp.Pixels);

        if (updateMipmaps)
        {
            SetMipmaps(GetMaxMipmaps(Width, Height), TextureTarget.Texture2D);
        }
    }

    public void ClampToEdge()
    {
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
    }

    public static int GetMaxMipmaps(int width, int height)
    {
        return 1 + (int)Math.Log2(Math.Max(width, height));
    }

    public static void SetAliasing(bool aliased, bool mipmaps, TextureTarget target)
    {
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, aliased ? mipmaps ? (int)TextureMinFilter.NearestMipmapNearest : (int)TextureMinFilter.Nearest : mipmaps ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, aliased ? (int)TextureMagFilter.Nearest : (int)TextureMagFilter.Linear);
    }

    public static void SetMipmaps(int maxLevel, TextureTarget target)
    {
        GL.TexParameter(target, TextureParameterName.TextureBaseLevel, 0);
        GL.TexParameter(target, TextureParameterName.TextureMaxLevel, maxLevel);
        GL.TexParameter(target, TextureParameterName.TextureLodBias, 0f);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public void Dispose()
    {
        GL.DeleteTexture(Handle);
        GC.SuppressFinalize(this);
        Handle = 0;
    }
}