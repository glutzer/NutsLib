using SkiaSharp;

namespace MareLib;

public partial class TextureBuilder
{
    public TextureBuilder SetColor(SKColor color)
    {
        paint.Color = color;
        return this;
    }

    public TextureBuilder SetColor(float r, float g, float b, float a)
    {
        paint.Color = new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
        return this;
    }

    public TextureBuilder StrokeMode()
    {
        paint.Style = SKPaintStyle.Stroke;
        return this;
    }

    public TextureBuilder StrokeMode(float width)
    {
        paint.StrokeWidth = width;
        paint.Style = SKPaintStyle.Stroke;
        return this;
    }

    public TextureBuilder FillMode()
    {
        paint.Style = SKPaintStyle.Fill;
        return this;
    }
}