using OpenTK.Mathematics;

namespace NutsLib;

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

    public TextObjectIndecipherable(string text, Font font, float fontScale, Vector4 color, Font fosterFont, CipherType type) : base(text, font, fontScale, color)
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
    public override int RenderLine(float x, float y, ShaderGui guiShader, int xAdvance = 0, bool centerVertically = false)
    {
        if (centerVertically) y += CenterOffset;

        // Floor x/y.
        x = (int)x;
        y = (int)y;

        guiShader.ShaderType = 2;
        guiShader.Color = color;

        guiShader.BindTexture(DynamicFontAtlas.AtlasTexture, "tex2d");

        // Arbitrary value for italics.
        if (italic) guiShader.ItalicSlant = LineHeight / 3f;
        if (bold) guiShader.Bold = 1;

        int index = 0;

        foreach (char c in text)
        {
            GlyphRenderInfo fontChar;

            if ((type == CipherType.FirstRandomized && index == 0) || type == CipherType.AllRandomized)
            {
                char randomChar = (char)rand.Next(65, 123);
                fontChar = font.GetGlyph(randomChar);
            }
            else
            {
                fontChar = font.GetGlyph(c);
            }

            guiShader.ModelMatrix = Matrix4.CreateScale(fontScale, fontScale, 1f) * Matrix4.CreateTranslation(x + xAdvance, y, 0f);
            xAdvance += (int)(fosterFont.GetGlyph(c).xAdvance * fontScale);
            RenderTools.RenderSquareVao(fontChar.vaoId);
            index++;
        }

        if (italic) guiShader.ItalicSlant = 0f;
        if (bold) guiShader.Bold = 0;

        guiShader.ShaderType = 0;

        return xAdvance;
    }
}