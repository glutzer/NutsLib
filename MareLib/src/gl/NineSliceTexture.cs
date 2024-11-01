using OpenTK.Mathematics;
using System;

namespace MareLib;

public class NineSliceTexture : IDisposable
{
    public Texture texture;
    public Vector2 SliceSize { get; private set; }
    public Vector2 Border { get; private set; }

    /// <summary>
    /// Takes textures and where to start the slice in pixels on the x and y.
    /// </summary>
    public NineSliceTexture(Texture texture, int slicePixelX, int slicePixelY)
    {
        this.texture = texture;
        SliceSize = new Vector2(slicePixelX, slicePixelY);
        Border = new Vector2((float)slicePixelX / texture.Width, (float)slicePixelY / texture.Height);
    }

    public Vector2 GetDimensions(float width, float height)
    {
        return new Vector2(SliceSize.X / width, SliceSize.Y / height);
    }

    public Vector2 GetCenterScale(float width, float height)
    {
        Vector2 originalCenter = new(texture.Width - (SliceSize.X * 2), texture.Height - (SliceSize.Y * 2));
        Vector2 newCenter = new Vector2(width, height) - SliceSize * 2;
        return newCenter / originalCenter;
    }

    public void Dispose()
    {
        texture.Dispose();
        GC.SuppressFinalize(this);
    }
}