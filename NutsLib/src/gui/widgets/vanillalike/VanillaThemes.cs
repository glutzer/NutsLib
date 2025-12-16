using OpenTK.Mathematics;
using SkiaSharp;
using System.Collections.Generic;

namespace NutsLib;

public static class VectorExtensions
{
    public static SKColor ToSkia(this Vector4 vector)
    {
        return new SKColor(
            (byte)(vector.X * 255f),
            (byte)(vector.Y * 255f),
            (byte)(vector.Z * 255f),
            (byte)(vector.W * 255f)
        );
    }
}

/// <summary>
/// Hold textures for vanilla.
/// </summary>
public static class VanillaThemes
{
    public static Font Font => FontRegistry.GetFont("lora");

    public static Vector4 YellowColor => new(1f, 0.8f, 0f, 1f);
    public static Vector4 WhitishTextColor => new(0.9f, 0.9f, 0.9f, 1f);
    public static Vector4 BlueProgress => new(0.4f, 0.7f, 1.5f, 0.7f);

    public static Vector4 TemporalColor => new(0f, 1f, 0.55f, 1f);
    public static Vector4 TemporalColorDark => new(0f, 0.9f, 0.45f, 0.5f);
    public static Vector4 VintageBrown => new(0.3f, 0.25f, 0.2f, 1f);

    public static Vector3 Red => new(1f, 0f, 0f);
    public static Vector3 Green => new(0f, 1f, 0f);
    public static Vector3 Blue => new(0f, 0f, 1f);

    private static int StrokeWidth => 3;
    private static int BlurWidth => 2;

    public static WidgetVanillaTitle AddTitleBar(Widget draggableWidget, string title)
    {
        WidgetVanillaTitle bar = new(draggableWidget, draggableWidget.Gui, draggableWidget, title);
        bar.Alignment(Align.CenterTop, AlignFlags.OutsideV).PercentWidth(1f).FixedHeight(8).SetChildSizing(ChildSizing.IgnoreThis);
        bar.AddChild(new WidgetVanillaButton(bar, bar.Gui, () => bar.Gui.TryClose(), "").SetColor(new Vector4(0.4f, 0f, 0f, 1f)).Alignment(Align.RightMiddle).FixedSize(8, 8));
        return bar;
    }

    // Slightly darker inset texture.
    public static NineSliceTexture InsetTexture => GetOrCreate("inset", () =>
    {
        Vector4 darker = VintageBrown;
        darker.Xyz *= 0.5f;

        Vector4 lighter = VintageBrown;
        lighter.Xyz *= 1.5f;

        Vector4 innerTex = VintageBrown;
        innerTex.Xyz *= 0.75f;

        int relief = 0;

        return TextureBuilder.Begin(64, 64)
        .FillRect(0, 0, 64, 64, innerTex.ToSkia())
        .StrokeLine(0, 0, 63 - relief, 0, darker.ToSkia(), StrokeWidth)
        .StrokeLine(0, 0, 0, 63 - relief, darker.ToSkia(), StrokeWidth)
        .StrokeLine(63, 0 + relief, 63, 63, lighter.ToSkia(), StrokeWidth)
        .StrokeLine(0 + relief, 63, 63, 63, lighter.ToSkia(), StrokeWidth)
        .BlurAll(BlurWidth)
        .End()
        .AsNineSlice(16, 16);
    });

    public static NineSliceTexture OutsetTexture => GetOrCreate("outset", () =>
    {
        Vector4 darker = VintageBrown;
        darker.Xyz *= 0.5f;

        Vector4 lighter = VintageBrown;
        lighter.Xyz *= 1.5f;

        int relief = 0;

        return TextureBuilder.Begin(64, 64)
        .FillRect(0, 0, 64, 64, VintageBrown.ToSkia())
        .StrokeLine(0, 0, 63 - relief, 0, lighter.ToSkia(), StrokeWidth)
        .StrokeLine(0, 0, 0, 63 - relief, lighter.ToSkia(), StrokeWidth)
        .StrokeLine(63, 0 + relief, 63, 63, darker.ToSkia(), StrokeWidth)
        .StrokeLine(0 + relief, 63, 63, 63, darker.ToSkia(), StrokeWidth)
        .BlurAll(BlurWidth)
        .End()
        .AsNineSlice(16, 16);
    });

    public static NineSliceTexture ItemSlotTexture => GetOrCreate("slot", () =>
    {
        Vector4 beige = new(0.8f, 0.7f, 0.6f, 1f);

        Vector4 darker = new(0f, 0f, 0f, 0.5f);
        Vector4 lighter = new(1f, 1f, 1f, 0.5f);

        int relief = 4;

        return TextureBuilder.Begin(64, 64)
        .FillRect(0, 0, 64, 64, beige.ToSkia())
        .StrokeLine(0, 0, 63 - relief, 0, darker.ToSkia(), 6)
        .StrokeLine(0, 0, 0, 63 - relief, darker.ToSkia(), 6)
        .StrokeLine(63, 0 + relief, 63, 63, lighter.ToSkia(), 6)
        .StrokeLine(0 + relief, 63, 63, 63, lighter.ToSkia(), 6)
        .BlurAll(2)
        .BlurAll(2)
        .BlurAll(2)
        .End()
        .AsNineSlice(24, 24);
    });

    private static readonly Dictionary<string, object> cache = [];

    private static T GetOrCreate<T>(string path, Func<T> makeTex)
    {
        if (cache.TryGetValue(path, out object? value))
        {
            return (T)value;
        }
        else
        {
            object tex = makeTex()!;
            cache.Add(path, tex);
            return (T)tex;
        }
    }

    public static void ClearCache()
    {
        foreach (object obj in cache)
        {
            if (obj is IDisposable tex)
            {
                tex.Dispose();
            }
        }

        cache.Clear();
    }
}