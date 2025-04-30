using MareLib;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;

namespace Equimancy;

public class TextDivider : IRenderableText
{
    public int PixelLength => dividerWidth;
    public float LineHeight => Gui.Scaled(4);

    public NineSliceTexture dividerTexture;
    public int dividerWidth;

    public TextDivider(NineSliceTexture dividerTexture, int dividerWidth)
    {
        this.dividerTexture = dividerTexture;
        this.dividerWidth = dividerWidth;
    }

    public void RenderCenteredLine(float x, float y, MareShader guiShader, bool centerVertically = false)
    {
        RenderLine(x - (PixelLength / 2), y, guiShader, 0, centerVertically);
    }

    public void RenderLeftAlignedLine(float x, float y, MareShader guiShader, bool centerVertically = false)
    {
        RenderLine(x - PixelLength, y, guiShader, 0, centerVertically);
    }

    public float RenderLine(float x, float y, MareShader guiShader, float xAdvance = 0, bool centerVertically = false)
    {
        RenderTools.RenderNineSlice(dividerTexture, guiShader, x, y - (LineHeight / 2), dividerWidth, LineHeight / 2, Gui.Scaled(0.5f));
        return PixelLength;
    }
}

public struct ItemInfoString
{
    public IRenderableText text;

    public ItemInfoString(string text, int size, Vector4 color, Font font)
    {
        this.text = new TextObject(text, font, size, color);
    }

    public ItemInfoString(List<TextObject> parsedObjects)
    {
        text = new TextObjectGroup();

        foreach (TextObject textObject in parsedObjects)
        {
            ((TextObjectGroup)text).Add(textObject);
        }
    }

    public ItemInfoString(NineSliceTexture dividerTexture, int dividerWidth)
    {
        text = new TextDivider(dividerTexture, dividerWidth);
    }
}

/// <summary>
/// Info about hovered item for WAILA and mouseover.
/// </summary>
public class ItemInfoWidget : Widget, IItemWidget
{
    private readonly NineSliceTexture background;
    private readonly NineSliceTexture divider;

    private ItemSlot? slot;
    private readonly Font font;

    private readonly List<ItemInfoString> text = new();
    private int maxLineWidth;
    private int totalLineHeight;

    public ItemInfoWidget(Gui gui, Bounds bounds) : base(gui, bounds)
    {
        background = TextureBuilder.Begin(256, 256)
            .SetColor(new SKColor(40, 0, 40, 200))
            .FillMode()
            .DrawRectangle(0, 0, 256, 256)
            .StrokeMode(8)
            .DrawEmbossedRectangle(0, 0, 256, 256, true)
            .End()
            .AsNineSlice(16, 16);

        divider = Texture.Create("equimancy:textures/divider.png").AsNineSlice(16, 2);

        font = FontRegistry.GetFont("celestia");
    }

    public void SetPosition(int mouseX, int mouseY)
    {
        int startX = mouseX + (8 * MainAPI.GuiScale);
        int startY = mouseY + (8 * MainAPI.GuiScale);

        if (startX + maxLineWidth > MainAPI.RenderWidth)
        {
            startX = MainAPI.RenderWidth - maxLineWidth;
        }

        if (startY + totalLineHeight > MainAPI.RenderHeight)
        {
            startY = MainAPI.RenderHeight - totalLineHeight;
        }

        bounds.Fixed(startX, startY, maxLineWidth, totalLineHeight);
        bounds.NoScaling();
        bounds.SetBounds();
    }

    public void SetItemStackData(ItemSlot slot)
    {
        text.Clear();

        this.slot = slot;
        if (slot.Itemstack == null) return;

        CollectibleObject collectible = slot.Itemstack.Collectible;
        text.Add(new ItemInfoString(collectible.GetHeldItemName(slot.Itemstack), Gui.Scaled(8), Vector4.One, font));

        // Divider.
        text.Add(new ItemInfoString(divider, text[0].text.PixelLength + Gui.Scaled(8)));

        StringBuilder stringBuilder = new();
        collectible.GetHeldItemInfo(slot, stringBuilder, MainAPI.Capi.World, false);

        List<string> lines = new(stringBuilder.ToString().Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries));

        foreach (string line in lines)
        {
            if (line.StartsWith("Mod:"))
            {
                text.Add(new ItemInfoString(line, Gui.Scaled(4), new Vector4(0, 0.5f, 1, 1), FontRegistry.GetFont("friz")));
                continue;
            }

            List<TextObject> objects = Markdown.ConvertMarkdownLine(line, Gui.Scaled(4));
            text.Add(new ItemInfoString(objects));
        }

        maxLineWidth = 0;
        totalLineHeight = 0;

        foreach (ItemInfoString itemInfo in text)
        {
            maxLineWidth = Math.Max(maxLineWidth, itemInfo.text.PixelLength);
            totalLineHeight += (int)itemInfo.text.LineHeight;
        }

        maxLineWidth += Gui.Scaled(8);
        totalLineHeight += Gui.Scaled(8);

        bounds.FixedSize(maxLineWidth, totalLineHeight);
        bounds.SetBounds();
    }

    public void OnLeaveSlot(ItemSlot slot)
    {
        if (this.slot == slot)
        {
            this.slot = null;
        }
    }

    public override void OnRender(float dt, MareShader shader)
    {
        if (slot == null || slot.Itemstack == null) return;

        RenderTools.RenderNineSlice(background, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);

        int center = bounds.X + (maxLineWidth / 2);

        float currentHeight = 0;

        foreach (ItemInfoString itemInfo in text)
        {
            currentHeight += itemInfo.text.LineHeight;
            itemInfo.text.RenderCenteredLine(center, bounds.Y + currentHeight, shader);
        }
    }

    public override void Dispose()
    {
        background.Dispose();
        divider.Dispose();
    }
}