using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Text;

namespace MareLib;

public class Font
{
    // Dictionary of all chars in this font.
    public readonly Dictionary<char, FontChar> fontChars;
    public readonly FontCharData[] fontCharData = new FontCharData[ushort.MaxValue];

    public float LineHeight { get; private set; }
    public float CenterOffset { get; private set; }

    public readonly Texture fontAtlas;

    public Font(Dictionary<char, FontChar> fontChars, float centerOffset, float lineHeight, Texture fontAtlas)
    {
        this.fontChars = fontChars;

        CenterOffset = centerOffset;
        LineHeight = lineHeight;

        this.fontAtlas = fontAtlas;

        FontChar space = fontChars[' '];
        FontCharData spaceData = new(space.meshHandle.vaoId, space.xAdvance);

        foreach (FontChar fontChar in fontChars.Values)
        {
            fontCharData[fontChar.unicode] = new FontCharData(fontChar.meshHandle.vaoId, fontChar.xAdvance);
        }

        for (int i = 0; i < ushort.MaxValue; i++)
        {
            // Unknown chars will have a space, which is guaranteed to exist.
            if (fontCharData[i].meshHandle == 0) fontCharData[i] = spaceData;
        }
    }

    /// <summary>
    /// Gets the width of a line of text, in pixels.
    /// </summary>
    public int GetLineWidth(string text, int fontScale)
    {
        float xAdvance = 0;

        foreach (char c in text)
        {
            xAdvance += (int)(fontCharData[c].xAdvance * fontScale);
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
            xAdvance -= (int)(fontCharData[text[i]].xAdvance * fontScale);

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
            xAdvance += (int)(fontCharData[text[i]].xAdvance * fontScale);
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

        guiShader.BindTexture(fontAtlas, "tex2d", 0);

        // Arbitrary value for italics.
        if (italic) guiShader.Uniform("italicSlant", LineHeight / 3);
        if (bold) guiShader.Uniform("bold", 1);

        foreach (char c in text)
        {
            FontCharData fontChar = fontCharData[c];
            guiShader.Uniform("modelMatrix", Matrix4.CreateScale(fontScale, fontScale, 1) * Matrix4.CreateTranslation(x + xAdvance, y, 0));
            xAdvance += (int)(fontChar.xAdvance * fontScale);
            RenderTools.RenderSquareVao(fontChar.meshHandle);
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

        guiShader.BindTexture(fontAtlas, "tex2d", 0);

        // Arbitrary value for italics.
        if (italic) guiShader.Uniform("italicSlant", LineHeight / 3);
        if (bold) guiShader.Uniform("bold", 1);

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            FontCharData fontChar = fontCharData[c];
            guiShader.Uniform("modelMatrix", Matrix4.CreateScale(fontScale, fontScale, 1) * Matrix4.CreateTranslation(x + xAdvance, y, 0));
            xAdvance += (int)(fontChar.xAdvance * fontScale);
            RenderTools.RenderSquareVao(fontChar.meshHandle);
        }

        if (italic) guiShader.Uniform("italicSlant", 0f);
        if (bold) guiShader.Uniform("bold", 0);

        guiShader.Uniform("shaderType", 0);

        return xAdvance;
    }

    public void Dispose()
    {
        foreach (FontChar fontChar in fontChars.Values)
        {
            fontChar.meshHandle.Dispose();
        }

        fontAtlas.Dispose();
    }
}