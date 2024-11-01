using System;

namespace MareLib;

public struct FontCharData
{
    public int meshHandle;
    public float xAdvance;

    public FontCharData(int meshHandle, float xAdvance)
    {
        this.meshHandle = meshHandle;
        this.xAdvance = xAdvance;
    }
}

public class FontChar
{
    public int unicode;
    public float xAdvance;
    public MeshHandle meshHandle;

    public FontChar(MeshHandle meshHandle, float xAdvance, int unicode)
    {
        this.unicode = unicode;
        this.xAdvance = xAdvance;
        this.meshHandle = meshHandle;
    }
}

public class FontJson
{
    public FontAtlas? atlas;
    public FontMetrics? metrics;
    public FontGlyph[] glyphs = Array.Empty<FontGlyph>();
}

public class FontAtlas
{
    public string? type;
    public int distanceRange;
    public int size;
    public int width;
    public int height;
    public string? yOrigin;
}

public class FontMetrics
{
    public int emSize;
    public float lineHeight;
    public float ascender;
    public float descender;
    public float underlineY;
    public float underlineThickness;
}

public class FontGlyph
{
    public int unicode;
    public float advance;
    public FontBounds? planeBounds;
    public FontBounds? atlasBounds;
}

public class FontBounds
{
    public float left;
    public float bottom;
    public float right;
    public float top;
}