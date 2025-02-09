using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MareLib;

public interface IRenderableText
{
    public float RenderLine(float x, float y, MareShader guiShader, float xAdvance = 0, bool centerVertically = false);
    public void RenderCenteredLine(float x, float y, MareShader guiShader, bool centerVertically = false);
    public void RenderLeftAlignedLine(float x, float y, MareShader guiShader, bool centerVertically = false);
    public int PixelLength { get; }
    public float LineHeight { get; }
}

public class TextObjectGroup : IRenderableText
{
    public List<TextObject> textObjects = new();

    public TextObjectGroup()
    {

    }

    public TextObjectGroup Add(TextObject text)
    {
        textObjects.Add(text);
        return this;
    }

    public int PixelLength => textObjects.Sum(x => x.PixelLength);
    public float LineHeight => textObjects.Count > 0 ? textObjects[0].LineHeight : 0;

    public float RenderLine(float x, float y, MareShader guiShader, float xAdvance = 0, bool centerVertically = false)
    {
        foreach (TextObject textObject in textObjects)
        {
            xAdvance = textObject.RenderLine(x, y, guiShader, xAdvance, centerVertically);
        }

        return xAdvance;
    }

    public void RenderCenteredLine(float x, float y, MareShader guiShader, bool centerVertically = false)
    {
        RenderLine(x - (PixelLength / 2), y, guiShader, 0, centerVertically);
    }

    public void RenderLeftAlignedLine(float x, float y, MareShader guiShader, bool centerVertically = false)
    {
        RenderLine(x - PixelLength, y, guiShader, 0, centerVertically);
    }
}

public class TextObject : IRenderableText
{
    protected string text;
    public Font font;
    public int fontScale;
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
        SetScale(50);

        float width50 = PixelLength;
        float widthMultiplier = (int)(widget.Width * widthScale) / width50;

        float height50 = font.LineHeight * 50;
        float heightMultiplier = (int)(widget.Height * heightScale) / height50;

        int fontScale = (int)Math.Min(50 * widthMultiplier, 50 * heightMultiplier);

        SetScale(fontScale);
    }

    public TextObject(string text, Font font, int fontScale, Vector4 color)
    {
        this.text = text;
        this.font = font;
        this.fontScale = fontScale;
        this.color = color;
        PixelLength = GetLineLength(text);
    }

    public void SetScale(int scale)
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
    public virtual float RenderLine(float x, float y, MareShader guiShader, float xAdvance = 0, bool centerVertically = false)
    {
        if (centerVertically) y += CenterOffset;

        xAdvance += font.RenderLine(x + xAdvance, y, text, fontScale, guiShader, color, italic, bold);

        return xAdvance;
    }

    /// <summary>
    /// Renders a line with the center at the position.
    /// </summary>
    public void RenderCenteredLine(float x, float y, MareShader guiShader, bool centerVertically = false)
    {
        RenderLine(x - (PixelLength / 2), y, guiShader, 0, centerVertically);
    }

    /// <summary>
    /// Renders a line to the left of the position.
    /// </summary>
    public void RenderLeftAlignedLine(float x, float y, MareShader guiShader, bool centerVertically = false)
    {
        RenderLine(x - PixelLength, y, guiShader, 0, centerVertically);
    }
}