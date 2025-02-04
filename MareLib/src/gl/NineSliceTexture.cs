using OpenTK.Mathematics;
using System;

namespace MareLib;

public static class NineSliceExtension
{
    public static NineSliceTexture AsNineSlice(this Texture texture, int slicePixelX, int slicePixelY)
    {
        return new NineSliceTexture(texture, slicePixelX, slicePixelY);
    }

    public static NineSliceTexture AsNineSlice(this Texture texture, int sliceX1, int sliceX2, int sliceY1, int sliceY2)
    {
        return new NineSliceTexture(texture, sliceX1, sliceX2, sliceY1, sliceY2);
    }
}

public class NineSliceTexture : IDisposable
{
    public Texture texture;
    public Vector4 SliceSize { get; private set; }
    public Vector4 Border { get; private set; }

    /// <summary>
    /// Takes textures and where to start the slice in pixels on the x and y.
    /// </summary>
    public NineSliceTexture(Texture texture, int slicePixelX, int slicePixelY)
    {
        this.texture = texture;
        SliceSize = new Vector4(slicePixelX, slicePixelY, slicePixelX, slicePixelY);
        Border = new Vector4((float)slicePixelX / texture.Width, (float)slicePixelY / texture.Height, (float)slicePixelX / texture.Width, (float)slicePixelY / texture.Height);
    }

    public NineSliceTexture(Texture texture, int sliceX1, int sliceX2, int sliceY1, int sliceY2)
    {
        this.texture = texture;
        SliceSize = new Vector4(sliceX1, sliceY1, sliceX2, sliceY2);
        Border = new Vector4((float)sliceX1 / texture.Width, (float)sliceY1 / texture.Height, (float)sliceX2 / texture.Width, (float)sliceY2 / texture.Height);
    }

    public Vector4 GetDimensions(float width, float height)
    {
        return new Vector4(SliceSize.X / width, SliceSize.Y / height, SliceSize.Z / width, SliceSize.W / height);
    }

    public Vector2 GetCenterScale(float width, float height)
    {
        Vector2 originalCenter = new(texture.Width - SliceSize.X - SliceSize.Z, texture.Height - SliceSize.Y - SliceSize.W);
        Vector2 newCenter = new(width - SliceSize.X - SliceSize.Z, height - SliceSize.Y - SliceSize.W);
        return newCenter / originalCenter;
    }

    public void Dispose()
    {
        texture.Dispose();
        GC.SuppressFinalize(this);
    }
}