using System.Collections.Generic;

namespace MareLib;

public class Font
{
    // Dictionary of all chars in this font.
    public readonly Dictionary<char, FontChar> fontChars;
    public readonly FontCharData[] fontCharData = new FontCharData[ushort.MaxValue];
    public float CenterOffset { get; private set; }
    public readonly Texture fontAtlas;

    public Font(Dictionary<char, FontChar> fontChars, float centerOffset, Texture fontAtlas)
    {
        this.fontChars = fontChars;
        CenterOffset = centerOffset;
        this.fontAtlas = fontAtlas;

        FontChar space = fontChars[' '];
        FontCharData spaceData = new(space.meshHandle.vaoId, space.xAdvance);

        foreach (FontChar fontChar in fontChars.Values)
        {
            fontCharData[fontChar.unicode] = new FontCharData(fontChar.meshHandle.vaoId, fontChar.xAdvance);
        }

        for (int i = 0; i < ushort.MaxValue; i++)
        {
            // Unknown chars will have a space, which is guaranteed to exist.
            if (fontCharData[i].meshHandle == 0) fontCharData[i] = spaceData;
        }
    }

    public void Dispose()
    {
        foreach (FontChar fontChar in fontChars.Values)
        {
            fontChar.meshHandle.Dispose();
        }

        fontAtlas.Dispose();
    }
}