using SkiaSharp;

namespace NutsLib;

/// <summary>
/// Builder for a skia texture with caching capabilities.
/// </summary>
public partial class TextureBuilder
{
    protected readonly SKBitmap bmp;
    protected readonly SKCanvas canvas;
    protected readonly SKPaint paint;
    public readonly int width;
    public readonly int height;

    /// <summary>
    /// Starts a skia texture builder.
    /// </summary>
    public static TextureBuilder Begin(int width, int height)
    {
        return new TextureBuilder(width, height);
    }

    /// <summary>
    /// Creates the texture.
    /// </summary>
    public Texture End(bool aliased = true, bool mipmaps = false)
    {
        Texture texture = Texture.Create(bmp, aliased, mipmaps);

        bmp.Dispose();
        canvas.Dispose();
        paint.Dispose();

        return texture;
    }

    protected TextureBuilder Shader(SKShader shader)
    {
        paint.Shader = shader;
        return this;
    }

    protected TextureBuilder NoShader()
    {
        paint.Shader = null;
        return this;
    }

    private TextureBuilder(int width, int height)
    {
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

    // Blur the entire bitmap.
    public TextureBuilder BlurAll(int blurRadius)
    {
        using SKBitmap blurred = new(width, height);
        using SKCanvas blurCanvas = new(blurred);
        using SKPaint blurPaint = new()
        {
            ImageFilter = SKImageFilter.CreateBlur(blurRadius, blurRadius)
        };
        blurCanvas.DrawBitmap(bmp, 0, 0, blurPaint);

        // Copy back to original bitmap.
        using SKCanvas originalCanvas = new(bmp);
        originalCanvas.DrawBitmap(blurred, 0, 0);

        blurred.Dispose();
        blurCanvas.Dispose();
        blurPaint.Dispose();

        return this;
    }

    /// <summary>
    /// Fill entire canvas with a gradient.
    /// </summary>
    public TextureBuilder FillGradient(int startX, int startY, int endX, int endY, SKColor startColor, SKColor endColor)
    {
        using SKShader shader = SKShader.CreateLinearGradient(
            new SKPoint(startX, startY),
            new SKPoint(endX, endY),
            new SKColor[] { startColor, endColor },
            null,
            SKShaderTileMode.Clamp);
        paint.Shader = shader;
        paint.Style = SKPaintStyle.Fill;
        canvas.DrawRect(new SKRect(0, 0, width, height), paint);
        paint.Shader = null;
        return this;
    }

    public TextureBuilder FillCircle(int cX, int cY, int radius, SKColor color)
    {
        paint.Color = color;
        paint.Style = SKPaintStyle.Fill;
        canvas.DrawCircle(cX, cY, radius, paint);
        return this;
    }

    public TextureBuilder FillRect(int x, int y, int width, int height, SKColor color)
    {
        paint.Color = color;
        paint.Style = SKPaintStyle.Fill;
        canvas.DrawRect(new SKRect(x, y, x + width, y + height), paint);
        return this;
    }

    public TextureBuilder StrokeCircle(int cX, int cY, int radius, SKColor color, int strokeWidth)
    {
        paint.Color = color;
        paint.StrokeWidth = strokeWidth;
        paint.Style = SKPaintStyle.Stroke;
        canvas.DrawCircle(cX, cY, radius, paint);
        return this;
    }

    public TextureBuilder StrokeRect(int x, int y, int width, int height, SKColor color, int strokeWidth)
    {
        paint.Color = color;
        paint.StrokeWidth = strokeWidth;
        paint.Style = SKPaintStyle.Stroke;
        canvas.DrawRect(new SKRect(x, y, x + width, y + height), paint);
        return this;
    }

    public TextureBuilder StrokeLine(int x1, int y1, int x2, int y2, SKColor color, int strokeWidth)
    {
        paint.Color = color;
        paint.StrokeWidth = strokeWidth;
        paint.Style = SKPaintStyle.Stroke;
        canvas.DrawLine(x1, y1, x2, y2, paint);
        return this;
    }
}