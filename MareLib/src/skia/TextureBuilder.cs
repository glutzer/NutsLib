using SkiaSharp;

namespace MareLib;

/// <summary>
/// Builder for a skia texture with caching capabilities.
/// </summary>
public partial class TextureBuilder
{
    /// <summary>
    /// Starts a skia texture builder.
    /// </summary>
    /// <param name="pixelGridLock">Normalized grid to lock this texture to for resizing. This gui system has 4 sizes.</param>
    public static TextureBuilder Begin(int width, int height, int pixelGridLock = 4)
    {
        return new TextureBuilder(width, height, pixelGridLock);
    }

    /// <summary>
    /// Creates the texture.
    /// </summary>
    public Texture End()
    {
        Texture texture = Texture.Create(bmp);

        bmp.Dispose();
        canvas.Dispose();
        paint.Dispose();

        return texture;
    }

    public TextureBuilder NoShader()
    {
        paint.Shader = null;
        return this;
    }

    public TextureBuilder Shader(SKShader shader)
    {
        paint.Shader = shader;
        return this;
    }

    public readonly SKBitmap bmp;
    public readonly SKCanvas canvas;
    public readonly SKPaint paint;
    public readonly int width;
    public readonly int height;

    private TextureBuilder(int width, int height, int pixelGridLock = 4)
    {
        // Round width/height up to normal grid.
        width = (int)((width / (float)pixelGridLock) + 0.5f) * pixelGridLock;
        height = (int)((height / (float)pixelGridLock) + 0.5f) * pixelGridLock;

        bmp = new SKBitmap(width, height);
        canvas = new SKCanvas(bmp);
        paint = new SKPaint()
        {
            IsAntialias = false
        };

        bmp.Erase(SKColors.Transparent);

        this.width = width;
        this.height = height;
    }
}