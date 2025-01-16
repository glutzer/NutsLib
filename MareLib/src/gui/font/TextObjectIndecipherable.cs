using OpenTK.Mathematics;
using System;

namespace MareLib;

public enum CipherType
{
    NoRandomization,
    FirstRandomized,
    AllRandomized
}

/// <summary>
/// Text object that renders with the advance of another font.
/// Can't be bold/italic.
/// Font = runic, foster font is font to replace.
/// </summary>
public class TextObjectIndecipherable : TextObject
{
    public readonly Font fosterFont;
    private readonly CipherType type;
    private readonly Random rand = new();

    public TextObjectIndecipherable(string text, Font font, int fontScale, Vector4 color, Font fosterFont, CipherType type) : base(text, font, fontScale, color)
    {
        this.fosterFont = fosterFont;
        this.type = type;
        PixelLength = GetLineLength(text);
    }

    public override int GetLineLength(string text)
    {
        return fosterFont?.GetLineWidth(text, fontScale) ?? font.GetLineWidth(text, fontScale);
    }

    /// <summary>
    /// Render foster line, copied from font's code.
    /// If this looks broken it's because it's not up to date with the font code.
    /// </summary>
    public override float RenderLine(float x, float y, MareShader guiShader, float xAdvance = 0, bool centerVertically = false)
    {
        FontCharData[] fontData = font.fontCharData;
        FontCharData[] fosterFontData = fosterFont.fontCharData;

        // Floor x/y.
        x = (int)x;
        y = (int)y;

        guiShader.Uniform("shaderType", 2);
        guiShader.Uniform("fontColor", color);

        guiShader.BindTexture(font.fontAtlas, "tex2d", 0);

        // Arbitrary value for italics.
        if (italic) guiShader.Uniform("italicSlant", LineHeight / 3);
        if (bold) guiShader.Uniform("bold", 1);

        int index = 0;

        foreach (char c in text)
        {
            FontCharData fontChar;

            if ((type == CipherType.FirstRandomized && index == 0) || type == CipherType.AllRandomized)
            {
                char randomChar = (char)rand.Next(65, 123);
                fontChar = fontData[randomChar];
            }
            else
            {
                fontChar = fontData[c];
            }

            guiShader.Uniform("modelMatrix", Matrix4.CreateScale(fontScale, fontScale, 1) * Matrix4.CreateTranslation(x + xAdvance, y, 0));
            xAdvance += (int)(fosterFontData[c].xAdvance * fontScale);
            RenderTools.RenderFontChar(fontChar.meshHandle);
            index++;
        }

        if (italic) guiShader.Uniform("italicSlant", 0f);
        if (bold) guiShader.Uniform("bold", 0);

        guiShader.Uniform("shaderType", 0);

        return xAdvance;
    }
}