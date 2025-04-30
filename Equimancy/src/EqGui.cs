using MareLib;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Equimancy;

/// <summary>
/// Common themes for this mods gui.
/// </summary>
public static class EqGui
{
    private static readonly Dictionary<string, NineSliceTexture> textures = new();

    private static SKColor BackgroundColor => new(75, 0, 150);
    private static SKColor RimColor => new(50, 50, 50);
    private static SKColor BoxBackgroundColor => new(25, 0, 50);

    private static SKColor ButtonColor => new(140, 70, 20); // Brown
    private static SKColor ButtonPressedColor => new(100, 60, 30); // Darker Brown
    private static SKColor ButtonHoveredColor => new(160, 80, 50); // Lighter Brown

    public static NineSliceTexture Button => GetOrCreate("button", () => Octagon(6, false, ButtonColor, RimColor));
    public static NineSliceTexture ButtonPressed => GetOrCreate("buttonPressed", () => Octagon(6, true, ButtonPressedColor, RimColor));
    public static NineSliceTexture ButtonHovered => GetOrCreate("buttonHovered", () => Octagon(6, false, ButtonHoveredColor, RimColor));

    public static NineSliceTexture Background => GetOrCreate("background", () => Rectangle(6, true, BackgroundColor.WithAlpha(100), BackgroundColor));

    public static NineSliceTexture BarOverlay => GetOrCreate("barOverlay", () => Rectangle(6, false, new SKColor(0, 0, 0, 0), RimColor));

    /// <summary>
    /// Box for checkboxes.
    /// </summary>
    public static NineSliceTexture Box => GetOrCreate("box", () => Octagon(6, true, BoxBackgroundColor, RimColor));

    private static NineSliceTexture Octagon(int stroke, bool inside, SKColor insideColor, SKColor outsideColor)
    {
        int size = stroke * 8;

        return TextureBuilder.Begin(size, size)
            .SetColor(insideColor)
            .FillMode()
            .DrawOctagon(0, 0, size, size, stroke)

            .SetColor(outsideColor)
            .StrokeMode(stroke)
            .DrawEmbossedOctagon(0, 0, size, size, stroke, inside)
            .End()

            .AsNineSlice(stroke * 2, stroke * 2);
    }

    private static NineSliceTexture Rectangle(int stroke, bool inside, SKColor insideColor, SKColor outsideColor)
    {
        int size = stroke * 8;

        return TextureBuilder.Begin(size, size)
            .SetColor(insideColor)
            .FillMode()
            .DrawRectangle(0, 0, size, size)

            .SetColor(outsideColor)
            .StrokeMode(stroke)
            .DrawEmbossedRectangle(0, 0, size, size, inside)
            .End()

            .AsNineSlice(stroke * 2, stroke * 2);
    }

    private static NineSliceTexture GetOrCreate(string path, Func<NineSliceTexture> makeTex)
    {
        if (textures.TryGetValue(path, out NineSliceTexture? value))
        {
            return value;
        }
        else
        {
            NineSliceTexture tex = makeTex();
            textures.Add(path, tex);
            return tex;
        }
    }

    public static void Dispose()
    {
        // Dispose all textures.
        foreach (KeyValuePair<string, NineSliceTexture> texture in textures)
        {
            texture.Value.Dispose();
        }

        textures.Clear();
    }
}