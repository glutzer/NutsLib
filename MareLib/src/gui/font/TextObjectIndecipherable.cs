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
        PixelLength = GetLineLength(text, fontScale);
    }

    public override int GetLineLength(string text, int fontScale)
    {
        float xAdvance = 0;

        FontCharData[] fontCharData = fosterFont?.fontCharData ?? font.fontCharData;

        foreach (char c in text)
        {
            xAdvance += fontCharData[c].xAdvance * fontScale;
        }

        return (int)xAdvance;
    }

    public override float RenderLine(float x, float y, MareShader guiShader, float xAdvance = 0, bool centerVertically = false)
    {
        if (centerVertically) y += font.CenterOffset * fontScale;

        x = (int)x;
        y = (int)y;

        guiShader.Uniform("shaderType", 2);
        guiShader.BindTexture(font.fontAtlas.Handle, "tex2d", 0);
        guiShader.Uniform("fontColor", color);

        FontCharData[] fontData = font.fontCharData;
        FontCharData[] fosterFontData = fosterFont.fontCharData;

        for (int i = 0; i < text.Length; i++)
        {
            Matrix4 translation = Matrix4.CreateScale(fontScale, fontScale, 1) * Matrix4.CreateTranslation(x + (int)xAdvance, y, 0);
            guiShader.Uniform("modelMatrix", translation);

            char c = text[i];
            FontCharData charData;

            if ((type == CipherType.FirstRandomized && i == 0) || type == CipherType.AllRandomized)
            {
                char randomChar = (char)rand.Next(65, 123);
                charData = fontData[randomChar];
            }
            else
            {
                charData = fontData[c];
            }

            RenderTools.RenderFontChar(charData.meshHandle);

            xAdvance += fosterFontData[c].xAdvance * fontScale;
        }

        guiShader.Uniform("shaderType", 0);

        return xAdvance;
    }
}