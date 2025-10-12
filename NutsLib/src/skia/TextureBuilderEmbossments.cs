using SkiaSharp;

namespace NutsLib;

public partial class TextureBuilder
{
    public static SKColor Shadow { get; } = new SKColor(0, 0, 0, 135);
    public static SKColor Highlight { get; } = new SKColor(255, 255, 255, 75);

    public void DrawEmbossed(SKPath path, bool inside)
    {
        SKColor shadowColor = inside ? Shadow : Highlight;
        SKColor highlightColor = inside ? Highlight : Shadow;

        int blurOffset = (int)(paint.StrokeWidth / 4);

        SKShader oldShader = paint.Shader;
        SKColor oldColor = paint.Color;
        NoShader();

        canvas.Save();
        canvas.ClipPath(path);

        canvas.Save();

        // Draw shadowed shape.
        canvas.Save();
        paint.Color = shadowColor;
        canvas.Translate(blurOffset, blurOffset);
        canvas.DrawPath(path, paint);
        canvas.Restore();

        // Draw highlighted shape.
        canvas.Save();
        paint.Color = highlightColor;
        canvas.Translate(-blurOffset, -blurOffset);
        canvas.DrawPath(path, paint);
        canvas.Restore();
        canvas.Restore();

        // Draw outer shape.
        paint.Shader = oldShader;
        paint.Color = oldColor;
        canvas.DrawPath(path, paint);
        canvas.Restore();
    }

    public TextureBuilder DrawEmbossedRectangle(int x, int y, int width, int height, bool inside)
    {
        using SKPath path = new();
        path.AddRect(new SKRect(x, y, x + width, y + height));
        DrawEmbossed(path, inside);
        return this;
    }

    public TextureBuilder DrawEmbossedRoundRectangle(int x, int y, int width, int height, int cornerSize, bool inside)
    {
        using SKPath path = new();
        path.AddRoundRect(new SKRect(x, y, x + width, y + height), cornerSize, cornerSize);
        DrawEmbossed(path, inside);
        return this;
    }

    public TextureBuilder DrawEmbossedCircle(int x, int y, int radius, bool inside)
    {
        using SKPath path = new();
        path.AddCircle(x, y, radius);
        DrawEmbossed(path, inside);
        return this;
    }

    public TextureBuilder DrawEmbossedTriangle(int x, int y, int width, int height, bool inside)
    {
        using SKPath path = GetPath(GetTrianglePoints(x, y, width, height));
        DrawEmbossed(path, inside);
        return this;
    }

    public TextureBuilder DrawEmbossedHexagon(int x, int y, int width, int height, bool inside)
    {
        using SKPath path = GetPath(GetHexagonPoints(x, y, width, height));
        DrawEmbossed(path, inside);
        return this;
    }

    public TextureBuilder DrawEmbossedOctagon(int x, int y, int width, int height, int cornerSize, bool inside)
    {
        using SKPath path = GetPath(GetOctagonPoints(x, y, width, height, cornerSize));
        DrawEmbossed(path, inside);
        return this;
    }
}