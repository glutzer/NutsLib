using Newtonsoft.Json;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace MareLib;

public static class FontRegistry
{
    private static readonly Dictionary<string, Font> fonts = new();

    /// <summary>
    /// Constant used for all spaces.
    /// </summary>
    private const float SPACE_ADVANCE = 0.2f;

    /// <summary>
    /// Returns a font.
    /// Will return the first font registered if none are found.
    /// </summary>
    public static Font GetFont(string name)
    {
        if (!fonts.TryGetValue(name, out Font? font))
        {
            font = fonts.Values.FirstOrDefault();
        }

        return font!;
    }

    public static void LoadFont(string jsonData, Texture fontAtlas, string name)
    {
        FontJson? fontJson = JsonConvert.DeserializeObject<FontJson>(jsonData);
        if (fontJson == null || fontJson.atlas == null || fontJson.metrics == null) throw new Exception($"Unable to deserialize font!");

        // How much the font must be translated downward to center it, * scale.
        float centerOffset = (fontJson.metrics.ascender + fontJson.metrics.descender) / 2;

        // Offset to new line.
        float lineHeight = fontJson.metrics.lineHeight;

        int atlasWidth = fontJson.atlas.width;
        int atlasHeight = fontJson.atlas.height;

        Dictionary<char, FontChar> fontDict = new();

        foreach (FontGlyph glyph in fontJson.glyphs)
        {
            if (glyph.planeBounds == null || glyph.atlasBounds == null) continue;
            MeshHandle glyphMesh = CreateGlyphHandle(glyph.planeBounds, glyph.atlasBounds, atlasWidth, atlasHeight);

            FontChar fontChar = new(glyphMesh, glyph.advance, glyph.unicode);

            fontDict.Add((char)glyph.unicode, fontChar);
        }

        // Add a space char.
        if (!fontDict.TryGetValue(' ', out _))
        {
            FontBounds spaceBounds = new()
            {
                left = 0,
                right = 0,
                top = 0,
                bottom = 0
            };

            MeshHandle spaceMesh = CreateGlyphHandle(spaceBounds, spaceBounds, atlasWidth, atlasHeight);
            FontChar spaceChar = new(spaceMesh, SPACE_ADVANCE, 32);

            fontDict.Add(' ', spaceChar);
        }

        fonts.Add(name, new Font(fontDict, centerOffset, lineHeight, fontAtlas));
    }

    public static MeshHandle CreateGlyphHandle(FontBounds planeBounds, FontBounds atlasBounds, int atlasWidth, int atlasHeight)
    {
        float leftUv = atlasBounds.left / atlasWidth;
        float rightUv = atlasBounds.right / atlasWidth;
        float topUv = (atlasHeight - atlasBounds.top) / atlasHeight;
        float bottomUv = (atlasHeight - atlasBounds.bottom) / atlasHeight;

        float uvWidth = rightUv - leftUv;
        float uvHeight = topUv - bottomUv;

        float width = planeBounds.right - planeBounds.left;
        float height = planeBounds.top - planeBounds.bottom;

        return QuadMeshUtility.CreateGuiQuadMesh(vertex =>
        {
            Vector3 position = new(planeBounds.left + (width * vertex.position.X), -planeBounds.top + (height * vertex.position.Y), 0f);
            Vector2 uv = new(leftUv + (uvWidth * vertex.uv.X), topUv - (uvHeight * vertex.uv.Y));

            return new GuiVertex(position, uv);
        });
    }

    /// <summary>
    /// Loads every msdf font from assets and data/fonts.
    /// </summary>
    public static void LoadFonts()
    {
        ICoreClientAPI capi = MainAPI.Capi;

        string dataPath = GamePaths.DataPath;

        // Check if FontCache exists.
        if (Directory.Exists(dataPath + "/FontCache")) Directory.Delete(dataPath + "/FontCache", true);
        Directory.CreateDirectory(dataPath + "/FontCache");

        foreach (IAsset font in capi.Assets.GetMany("fonts"))
        {
            File.WriteAllBytes(dataPath + "/FontCache/font.zip", font.Data);
            ZipFile.ExtractToDirectory(dataPath + "/FontCache/font.zip", dataPath + "/FontCache");

            byte[] textureData = File.ReadAllBytes(dataPath + "/FontCache/font.png");
            Texture texture = Texture.Create(textureData, false);

            string jsonData = File.ReadAllText(dataPath + "/FontCache/font.json");

            LoadFont(jsonData, texture, font.Name.Split('.')[0]);

            // Delete all files in FontCache.
            foreach (string file in Directory.GetFiles(dataPath + "/FontCache")) File.Delete(file);
        }

        // Remove FontCache.
        Directory.Delete(dataPath + "/FontCache");
    }

    public static void Dispose()
    {
        foreach (Font font in fonts.Values) font.Dispose();
        fonts.Clear();
    }
}