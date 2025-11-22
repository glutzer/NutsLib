using SkiaSharp;

namespace NutsLib;

public partial class TextureBuilder
{
    private static SKPath GetPath(SKPoint[] points)
    {
        SKPath path = new();
        path.MoveTo(points[0]);
        for (int i = 1; i < points.Length; i++)
        {
            path.LineTo(points[i]);
        }
        path.Close();
        return path;
    }

    private static SKPoint[] GetTrianglePoints(int x, int y, int width, int height)
    {
        SKPoint[] points =
        [
            // Set 3 triangle points.
            new(x + (width / 2), y),
            new(x + width, y + height),
            new(x, y + height)
        ];
        return points;
    }

    private static SKPoint[] GetHexagonPoints(int x, int y, int width, int height)
    {
        SKPoint[] points =
        [
            // Set 6 hexagon points.
            new(x + (width / 2), y),
            new(x + width, y + (height / 4)),
            new(x + width, y + height - (height / 4)),
            new(x + (width / 2), y + height),
            new(x, y + height - (height / 4)),
            new(x, y + (height / 4))
        ];
        return points;
    }

    private static SKPoint[] GetOctagonPoints(int x, int y, int width, int height, int cornerSize)
    {
        SKPoint[] points =
        [
            // Set 8 octagon points.
            new(x + cornerSize, y),
            new(x + width - cornerSize, y),
            new(x + width, y + cornerSize),
            new(x + width, y + height - cornerSize),
            new(x + width - cornerSize, y + height),
            new(x + cornerSize, y + height),
            new(x, y + height - cornerSize),
            new(x, y + cornerSize)
        ];

        return points;
    }
}