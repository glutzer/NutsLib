using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NutsLib;

public interface IMarkdownToken
{

}

public class MarkdownText : IMarkdownToken
{
    public string text;

    public MarkdownText(string text)
    {
        this.text = text;
    }
}

public class MarkdownTag : IMarkdownToken
{
    public string tag = null!;
    public Dictionary<string, string> attributes = [];
    public List<IMarkdownToken> children = [];

    public MarkdownTag(string tag)
    {
        ParseTag(tag);
    }

    public void ParseTag(string tag)
    {
        string[] parts = tag.Split(' ');

        this.tag = parts[0];

        // Attributes cannot contain any spaces or this will break.
        for (int i = 1; i < parts.Length; i++)
        {
            int index = parts[i].IndexOf('=');

            // Split at = if index is not -1.
            // Attributes may contain "", remove when needed.
            if (index != -1)
            {
                string key = parts[i][..index];
                string value = parts[i][(index + 1)..];
                attributes.Add(key, value);
            }
        }
    }
}

public static partial class Markdown
{
    /// <summary>
    /// Convert a line of markdown into a formatted one. Does not work with links, special elements.
    /// </summary>
    public static List<TextObject> ConvertMarkdownLine(string text, int fontScale)
    {
        List<IMarkdownToken> tokens = Parse(text);
        List<TextObject> textObjects = [];

        foreach (IMarkdownToken token in tokens)
        {
            ProcessToken(textObjects, token, false, false, Vector4.One, fontScale);
        }

        return textObjects;
    }

    /// <summary>
    /// Recursively processes tokens.
    /// Could be bad if it's really big but it's not.
    /// </summary>
    private static void ProcessToken(List<TextObject> objects, IMarkdownToken token, bool bold, bool italic, Vector4 color, int fontScale)
    {
        if (token is MarkdownText markdownText)
        {
            objects.Add(new TextObject(markdownText.text, FontRegistry.GetFont("celestia"), fontScale, color).Bold(bold).Italic(italic));
            return;
        }

        if (token is MarkdownTag tag)
        {
            if (tag.tag == "i")
            {
                italic = true;
            }

            if (tag.tag == "strong")
            {
                bold = true;
            }

            if (tag.tag == "font")
            {
                if (tag.attributes.TryGetValue("color", out string? hex))
                {
                    color = ConvertHexToColor(hex);
                }
            }

            foreach (IMarkdownToken child in tag.children)
            {
                ProcessToken(objects, child, bold, italic, color, fontScale);
            }
        }
    }

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex MyRegex();

    /// <summary>
    /// Converts hex string, returns white if invalid.
    /// </summary>
    public static Vector4 ConvertHexToColor(string hex)
    {
        // Cleanse hex.
        hex = MyRegex().Replace(hex, "");

        if (hex.Length != 6)
        {
            return Vector4.One; // Invalid hex code, return default color.
        }

        Vector4 color;

        try
        {
            int r = Convert.ToInt32(hex[..2], 16);
            int g = Convert.ToInt32(hex[2..4], 16);
            int b = Convert.ToInt32(hex[4..6], 16);

            color = new Vector4(r / 255f, g / 255f, b / 255f, 1);
        }
        catch
        {
            color = Vector4.One;
        }

        return color;
    }

    /// <summary>
    /// Parses markdown text (not real markdown text).
    /// Builds a hierarchy from top level elements, which result in an IMarkdownToken.
    /// </summary>
    public static List<IMarkdownToken> Parse(string text)
    {
        bool insideTag = false;

        // Builder for both text and tags.
        StringBuilder builder = new();

        List<IMarkdownToken> topLevelElements = [];
        Stack<MarkdownTag> tagStack = new();

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '<')
            {
                insideTag = true;

                if (builder.Length > 0)
                {
                    if (tagStack.Count == 0)
                    {
                        topLevelElements.Add(new MarkdownText(builder.ToString()));
                    }
                    else
                    {
                        tagStack.Peek().children.Add(new MarkdownText(builder.ToString()));
                    }

                    builder.Clear();
                }

                continue;
            }

            if (c == '>' && insideTag)
            {
                if (builder[0] == '/') // Try to close the tag.
                {
                    // Get current tag except /
                    string currentTag = builder.ToString()[1..];
                    MarkdownTag markdownTag = new(currentTag);

                    // This does not close anything, ignore it and continue on.
                    if (tagStack.Count == 0 || tagStack.Peek().tag != markdownTag.tag)
                    {
                        builder.Clear();
                        insideTag = false;
                        continue;
                    }

                    tagStack.Pop();
                    builder.Clear();
                }
                else // Opening tag.
                {
                    if (tagStack.Count == 0)
                    {
                        topLevelElements.Add(new MarkdownTag(builder.ToString()));
                        tagStack.Push((MarkdownTag)topLevelElements[^1]);
                    }
                    else
                    {
                        tagStack.Peek().children.Add(new MarkdownTag(builder.ToString()));
                        tagStack.Push((MarkdownTag)tagStack.Peek().children[^1]);
                    }

                    builder.Clear();
                }

                continue;
            }

            // Append actual text.
            builder.Append(c);
        }

        // Remaining text.
        if (tagStack.Count == 0)
        {
            topLevelElements.Add(new MarkdownText(builder.ToString()));
        }
        else
        {
            tagStack.Peek().children.Add(new MarkdownText(builder.ToString()));
        }

        return topLevelElements;
    }
}