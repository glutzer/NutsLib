using OpenTK.Mathematics;
using System;
using Vintagestory.Client.NoObf;

namespace MareLib;

public class TextObject
{
    private string text;
    public Font font;
    public int fontScale;

    public string Text
    {
        get => text;
        set => text = value;
    }

    public TextObject(string text, Font font, int fontScale)
    {
        this.text = text;
        this.font = font;
        this.fontScale = fontScale;
    }

    /// <summary>
    /// Renders the text at the specified position.
    /// </summary>
    /// <param name="center">Center the font vertically on the position?</param>
    public void RenderLine(float x, float y, ShaderProgram guiShader, bool center = false)
    {
        if (center) y += font.CenterOffset * fontScale;

        x = MathF.Round(x);
        y = MathF.Round(y);

        guiShader.Uniform("shaderType", 2);
        RenderTools.BindTexture(font.fontAtlas, guiShader);

        FontCharData[] fontData = font.fontCharData;

        float currentOffset = 0;

        for (int i = 0; i < text.Length; i++)
        {
            Matrix4 translation = Matrix4.CreateScale(fontScale, fontScale, 1) * Matrix4.CreateTranslation(x + currentOffset, y, 0);

            guiShader.Uniform("modelMatrix", translation);

            char c = text[i];
            FontCharData charData = fontData[c];
            RenderTools.RenderFontChar(charData.meshHandle);

            currentOffset += MathF.Round(charData.xAdvance * fontScale);
        }

        guiShader.Uniform("shaderType", 0);
    }
}