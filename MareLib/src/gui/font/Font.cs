using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using System.Text;

namespace MareLib;

public class Font
{
    public string Name { get; private set; }
    public readonly GlyphRenderInfo[] fontCharData = new GlyphRenderInfo[ushort.MaxValue];

    public float LineHeight { get; private set; }
    public float CenterOffset { get; private set; }

    public Font(string name)
    {
        Name = name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GlyphRenderInfo GetGlyph(char c)
    {
        GlyphRenderInfo fontChar = fontCharData[c];
        if (fontChar.vaoId == 0) fontChar = fontCharData[c] = DynamicFontAtlas.GetGlyphMesh(c, Name);
        return fontChar;
    }

    /// <summary>
    /// Gets the width of a line of text, in pixels.
    /// </summary>
    public int GetLineWidth(string text, int fontScale)
    {
        float xAdvance = 0;

        foreach (char c in text)
        {
            xAdvance += (int)(GetGlyph(c).xAdvance * fontScale);
        }

        return (int)xAdvance;
    }

    /// <summary>
    /// Gets the index at this advance.
    /// May return text length (1 over index).
    /// </summary>
    public int GetIndexAtAdvance(string text, int fontScale, float xAdvance)
    {
        for (int i = 0; i < text.Length; i++)
        {
            xAdvance -= (int)(GetGlyph(text[i]).xAdvance * fontScale);

            if (xAdvance <= 0)
            {
                return i;
            }
        }

        return text.Length;
    }

    /// <summary>
    /// Gets the width of a line of text up to before an index.
    /// Index can be 1 longer than string.
    /// </summary>
    public float GetLineWidthUpToIndex(string text, int fontScale, int index)
    {
        float xAdvance = 0;

        for (int i = 0; i < index; i++)
        {
            xAdvance += (int)(GetGlyph(text[i]).xAdvance * fontScale);
        }

        return xAdvance;
    }

    /// <summary>
    /// Renders a line of text with the gui shader.
    /// Returns advance.
    /// </summary>
    public float RenderLine(float x, float y, string text, int fontScale, MareShader guiShader, Vector4 color, bool italic = false, bool bold = false)
    {
        // Floor x/y.
        x = (int)x;
        y = (int)y;

        float xAdvance = 0;

        guiShader.Uniform("shaderType", 2);
        guiShader.Uniform("fontColor", color);

        guiShader.BindTexture(DynamicFontAtlas.AtlasTexture, "tex2d", 0);

        // Arbitrary value for italics.
        if (italic) guiShader.Uniform("italicSlant", LineHeight / 3);
        if (bold) guiShader.Uniform("bold", 1);

        foreach (char c in text)
        {
            GlyphRenderInfo fontChar = GetGlyph(c);
            guiShader.Uniform("modelMatrix", Matrix4.CreateScale(fontScale, fontScale, 1) * Matrix4.CreateTranslation(x + xAdvance, y, 0));
            xAdvance += (int)(fontChar.xAdvance * fontScale);
            RenderTools.RenderSquareVao(fontChar.vaoId);
        }

        if (italic) guiShader.Uniform("italicSlant", 0f);
        if (bold) guiShader.Uniform("bold", 0);

        guiShader.Uniform("shaderType", 0);

        return xAdvance;
    }

    /// <summary>
    /// Renders a line of text with the gui shader.
    /// Returns advance.
    /// </summary>
    public float RenderLine(float x, float y, StringBuilder text, int fontScale, MareShader guiShader, Vector4 color, bool italic = false, bool bold = false)
    {
        // Floor x/y.
        x = (int)x;
        y = (int)y;

        float xAdvance = 0;

        guiShader.Uniform("shaderType", 2);
        guiShader.Uniform("fontColor", color);

        guiShader.BindTexture(DynamicFontAtlas.AtlasTexture, "tex2d", 0);

        // Arbitrary value for italics.
        if (italic) guiShader.Uniform("italicSlant", LineHeight / 3);
        if (bold) guiShader.Uniform("bold", 1);

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            GlyphRenderInfo fontChar = GetGlyph(c);
            guiShader.Uniform("modelMatrix", Matrix4.CreateScale(fontScale, fontScale, 1) * Matrix4.CreateTranslation(x + xAdvance, y, 0));
            xAdvance += (int)(fontChar.xAdvance * fontScale);
            RenderTools.RenderSquareVao(fontChar.vaoId);
        }

        if (italic) guiShader.Uniform("italicSlant", 0f);
        if (bold) guiShader.Uniform("bold", 0);

        guiShader.Uniform("shaderType", 0);

        return xAdvance;
    }
}