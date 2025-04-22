using System.Collections.Generic;

namespace MareLib;

public static class FontRegistry
{
    private static readonly Dictionary<string, Font> fonts = new();

    /// <summary>
    /// Returns a font.
    /// Will return the first font registered if none are found.
    /// </summary>
    public static Font GetFont(string name)
    {
        if (!fonts.TryGetValue(name, out Font? font))
        {
            font = new Font(name);
            fonts[name] = font;
        }

        return font;
    }

    public static void Dispose()
    {
        fonts.Clear();
    }
}