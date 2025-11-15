using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NutsLib;

public interface IRenderableText
{
    int RenderLine(float x, float y, ShaderGui guiShader, int xAdvance = 0, bool centerVertically = false);
    void RenderCenteredLine(float x, float y, ShaderGui guiShader, bool centerVertically = false);
    void RenderLeftAlignedLine(float x, float y, ShaderGui guiShader, bool centerVertically = false);
    int PixelLength { get; }
    float LineHeight { get; }
}

public class TextObjectGroup : IRenderableText
{
    public List<TextObject> textObjects = [];

    public TextObjectGroup()
    {

    }

    public TextObjectGroup Add(TextObject text)
    {
        textObjects.Add(text);
        return this;
    }

    public int PixelLength => textObjects.Sum(x => x.PixelLength);
    public float LineHeight => textObjects.Count > 0 ? textObjects[0].LineHeight : 0f;

    public int RenderLine(float x, float y, ShaderGui guiShader, int xAdvance = 0, bool centerVertically = false)
    {
        foreach (TextObject textObject in textObjects)
        {
            xAdvance = textObject.RenderLine(x, y, guiShader, xAdvance, centerVertically);
        }

        return xAdvance;
    }

    public void RenderCenteredLine(float x, float y, ShaderGui guiShader, bool centerVertically = false)
    {
        RenderLine(x - (PixelLength / 2), y, guiShader, 0, centerVertically);
    }

    public void RenderLeftAlignedLine(float x, float y, ShaderGui guiShader, bool centerVertically = false)
    {
        RenderLine(x - PixelLength, y, guiShader, 0, centerVertically);
    }
}

public class TextObject : IRenderableText
{
    public bool Shadow { get; set; }
    protected string text;
    public Font font;
    public float fontScale;
    public Vector4 color;

    protected bool bold;
    protected bool italic;

    public TextObject Bold(bool bold)
    {
        this.bold = bold;
        return this;
    }

    public TextObject Italic(bool italic)
    {
        this.italic = italic;
        return this;
    }

    /// <summary>
    /// Pixel length of the current text/scale.
    /// Equivalent to what would be advanced if it were rendered.
    /// </summary>
    public int PixelLength { get; protected set; }
    public float LineHeight => font.LineHeight * fontScale;
    public float CenterOffset => font.CenterOffset * fontScale;

    public string Text
    {
        get => text;
        set
        {
            text = value;
            PixelLength = GetLineLength(text);
        }
    }

    public void SetScaleFromWidget(Widget widget, float widthScale, float heightScale)
    {
        SetScale(50f);

        float width50 = PixelLength;
        float widthMultiplier = (int)(widget.Width * widthScale) / width50;

        float height50 = font.LineHeight * 50f;
        float heightMultiplier = (int)(widget.Height * heightScale) / height50;

        float fontScale = (int)Math.Min(50 * widthMultiplier, 50f * heightMultiplier);

        SetScale(fontScale);
    }

    public TextObject(string text, Font font, float fontScale, Vector4 color)
    {
        this.text = text;
        this.font = font;
        this.fontScale = fontScale;
        this.color = color;
        PixelLength = GetLineLength(text);
    }

    public void SetScale(float scale)
    {
        fontScale = scale;
        PixelLength = GetLineLength(text);
    }

    public virtual int GetLineLength(string text)
    {
        return font.GetLineWidth(text, fontScale);
    }

    /// <summary>
    /// Render a single line using the font.
    /// Returns current x advance.
    /// </summary>
    public virtual int RenderLine(float x, float y, ShaderGui guiShader, int xAdvance = 0, bool centerVertically = false)
    {
        if (centerVertically) y += CenterOffset;

        if (Shadow)
        {
            Vector4 shadowed = color;
            shadowed.Xyz *= 0.25f;
            float shadowOffset = fontScale / 20f;
            font.RenderLine(x + xAdvance + shadowOffset, y + shadowOffset, text, fontScale, guiShader, shadowed, italic, bold);
        }

        xAdvance += font.RenderLine(x + xAdvance, y, text, fontScale, guiShader, color, italic, bold);

        return xAdvance;
    }

    /// <summary>
    /// Renders a line with the center at the position.
    /// </summary>
    public void RenderCenteredLine(float x, float y, ShaderGui guiShader, bool centerVertically = false)
    {
        RenderLine(x - (PixelLength / 2), y, guiShader, 0, centerVertically);
    }

    /// <summary>
    /// Renders a line to the left of the position.
    /// </summary>
    public void RenderLeftAlignedLine(float x, float y, ShaderGui guiShader, bool centerVertically = false)
    {
        RenderLine(x - PixelLength, y, guiShader, 0, centerVertically);
    }
}