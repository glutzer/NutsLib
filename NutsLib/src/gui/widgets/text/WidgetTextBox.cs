using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;

namespace NutsLib;

/// <summary>
/// Example of an editable text box, with 1 level of complexity.
/// </summary>
public class WidgetTextBox : Widget
{
    private readonly List<string> lines = [];
    private readonly Font font;
    private readonly int fontScale;
    private readonly Vector4 color;

    private int cursorXIndex; // Character index.
    private int cursorYIndex; // Line index.

    private Vector2 cursorRelativePosition; // Cursor offset.

    private bool active; // Is the box able to be typed in?

    private Texture cursorTexture;
    private Texture selectTexture;

    private bool insertMode = false;

    private bool dragging = false;

    private bool Selecting => selectStart != selectEnd;
    private Vector2i selectStart;
    private Vector2i selectEnd;

    private readonly bool limitTextToBox;
    private readonly int maxLines;

    private Gui? gui;

    public void ClearSelection()
    {
        selectStart = Vector2i.Zero;
        selectEnd = Vector2i.Zero;
    }

    private Vector2i GetIndexAtMousePos(int x, int y)
    {
        float lineHeight = font.LineHeight * fontScale;

        // Normalize position.
        x -= X;
        y -= Y;

        int lineIndex = (int)(y / lineHeight);

        if (lineIndex < 0)
        {
            // Behind.
            return new Vector2i(0, 0);
        }

        if (lineIndex >= lines.Count)
        {
            // After.
            return new Vector2i(lines[^1].Length, lines.Count - 1);
        }

        string line = lines[lineIndex];
        int index = font.GetIndexAtAdvance(line, fontScale, x);
        return new Vector2i(index, lineIndex);
    }

    public WidgetTextBox(Widget? parent, Gui gui, Font font, int fontScale, Vector4 color, bool limitTextToBox = true, int maxLines = 1000) : base(parent, gui)
    {
        this.font = font;
        this.fontScale = fontScale;
        this.color = color;

        this.limitTextToBox = limitTextToBox;
        this.maxLines = maxLines;

        cursorTexture = TextureBuilder.Begin(64, 64)
            .SetColor(SkiaThemes.White.WithAlpha(100))
            .DrawRectangle(0, 0, 64, 64)
            .End();

        selectTexture = TextureBuilder.Begin(64, 64)
            .SetColor(SkiaThemes.White.WithAlpha(100))
            .DrawRectangle(0, 0, 64, 64)
            .End();

        lines.Add("");
        MoveCursor(0, 0);
    }

    /// <summary>
    /// Sets the color of the cursor.
    /// </summary>
    public WidgetTextBox CursorColor(Vector4 color)
    {
        cursorTexture.Dispose();

        SKColor skColor = new((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255), (byte)(color.W * 255));

        cursorTexture = TextureBuilder.Begin(64, 64)
            .SetColor(skColor)
            .DrawRectangle(0, 0, 64, 64)
            .End();

        return this;
    }

    /// <summary>
    /// Sets the color of the selection.
    /// </summary>
    public WidgetTextBox SelectColor(Vector4 color)
    {
        selectTexture.Dispose();

        SKColor skColor = new((byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255), (byte)(color.W * 255));

        selectTexture = TextureBuilder.Begin(64, 64)
            .SetColor(skColor)
            .DrawRectangle(0, 0, 64, 64)
            .End();

        return this;
    }

    /// <summary>
    /// Sets content of the box. Splits at line breaks (\n).
    /// </summary>
    public void LoadContent(string content)
    {
        string[] strings = content.Split("\n");

        lines.Clear();
        lines.AddRange(strings);

        ClearSelection();
        MoveCursor(0, 0);
    }

    /// <summary>
    /// Sets content of the box.
    /// </summary>
    public void LoadContent(List<string> content)
    {
        lines.Clear();
        lines.AddRange(content);

        if (content.Count == 0)
        {
            lines.Add("");
        }

        ClearSelection();
        MoveCursor(0, 0);
    }

    /// <summary>
    /// Copies the current list of lines.
    /// </summary>
    public List<string> SaveContent()
    {
        return [.. lines];
    }

    /// <summary>
    /// Returns the current list of lines.
    /// </summary>
    public List<string> ReadContent()
    {
        return lines;
    }

    public override void OnRender(float dt, NuttyShader shader)
    {
        RenderTools.PushScissor(this);

        float lineHeight = font.LineHeight * fontScale;

        // Render every lines from top to bottom.
        for (int i = 0; i < lines.Count; i++)
        {
            float y = Y + (i * lineHeight) + lineHeight;
            font.RenderLine(X, y, lines[i], fontScale, shader, color);
        }

        // Render cursor.
        if (active)
        {
            shader.BindTexture(cursorTexture.Handle, "tex2d", 0);

            if (insertMode && cursorXIndex != lines[cursorYIndex].Length)
            {
                char charAtIndex = lines[cursorYIndex][cursorXIndex];
                float charWidth = font.GetGlyph(charAtIndex).xAdvance * fontScale;
                RenderTools.RenderQuad(shader, X + cursorRelativePosition.X, Y + cursorRelativePosition.Y + (lineHeight / 4), charWidth, -lineHeight);
            }
            else if (MainAPI.Capi.World.ElapsedMilliseconds / 1000f % 1 < 0.5f) // Blink.
            {
                RenderTools.RenderQuad(shader, X + cursorRelativePosition.X, Y + cursorRelativePosition.Y, lineHeight / 2, 4); // Magic numbers.
            }

            // Selection is assumed not to be out of the bounds here.
            if (Selecting)
            {
                shader.BindTexture(selectTexture.Handle, "tex2d", 0);

                MinAndMaxSelection(out Vector2i minSelection, out Vector2i maxSelection);

                int startLine = minSelection.Y;
                int endLine = maxSelection.Y;

                for (int i = startLine; i < endLine + 1; i++)
                {
                    string line = lines[i];

                    // When not truncating these flickering occurs.
                    float xStart = (int)(i == startLine ? font.GetLineWidthUpToIndex(line, fontScale, minSelection.X) : 0);
                    float xEnd = (int)(i == endLine ? font.GetLineWidthUpToIndex(line, fontScale, maxSelection.X) : font.GetLineWidthUpToIndex(line, fontScale, line.Length));

                    float y = Y + (i * lineHeight) + lineHeight;
                    RenderTools.RenderQuad(shader, X + xStart, y, xEnd - xStart, -lineHeight);
                }
            }
        }

        RenderTools.PopScissor();
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.KeyDown += GuiEvents_KeyDown;
        guiEvents.KeyPress += GuiEvents_KeyPress;
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseUp += GuiEvents_MouseUp;
        guiEvents.MouseMove += GuiEvents_MouseMove;

        gui = guiEvents.gui;
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && IsInAllBounds(obj))
        {
            obj.Handled = true;

            dragging = true;

            Vector2i pos = GetIndexAtMousePos(obj.X, obj.Y);
            selectStart = pos;
            selectEnd = pos;

            MoveCursor(pos.X, pos.Y);

            active = true;
        }
        else
        {
            active = false;
        }
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        dragging = false;
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (!obj.Handled && IsInAllBounds(obj))
        {
            // Set cursor.
            gui.MouseOverCursor = "textselect";
        }

        if (dragging)
        {
            selectEnd = GetIndexAtMousePos(obj.X, obj.Y);
            MoveCursor(selectEnd.X, selectEnd.Y);
        }
    }

    private void GuiEvents_KeyDown(KeyEvent obj)
    {
        if (!active || obj.Handled) return;

        obj.Handled = true;

        if (obj.KeyCode == (int)GlKeys.Delete)
        {
            TryDeleteSelection();

            if (cursorXIndex == lines[cursorYIndex].Length && cursorYIndex == lines.Count - 1) return; // Nothing to delete.

            if (cursorXIndex == lines[cursorYIndex].Length)
            {
                string nextLine = lines[cursorYIndex + 1];
                lines[cursorYIndex] += nextLine;
                lines.RemoveAt(cursorYIndex + 1);
            }
            else
            {
                string currentLine = lines[cursorYIndex];
                lines[cursorYIndex] = currentLine.Remove(cursorXIndex, 1);
            }
        }

        if (obj.KeyCode == (int)GlKeys.Insert)
        {
            insertMode = !insertMode;
        }

        if (obj.KeyCode == (int)GlKeys.Enter)
        {
            TryDeleteSelection();

            if (cursorYIndex == maxLines - 1) return; // Max lines reached.

            string firstLine = lines[cursorYIndex][..cursorXIndex];

            // Check for the end of line index.
            string secondLine = cursorXIndex == lines[cursorYIndex].Length ? "" : lines[cursorYIndex][cursorXIndex..];

            lines[cursorYIndex] = firstLine;
            lines.Insert(cursorYIndex + 1, secondLine);

            MoveCursor(0, cursorYIndex + 1);
        }

        if (obj.KeyCode == (int)GlKeys.BackSpace)
        {
            // Only remove the selection with backspace.
            if (Selecting)
            {
                TryDeleteSelection();
                return;
            }

            if (cursorXIndex == 0 && cursorYIndex == 0) return; // Nothing to remove.

            if (cursorXIndex == 0)
            {
                string previousLine = lines[cursorYIndex - 1];
                string currentLine = lines[cursorYIndex];
                lines.RemoveAt(cursorYIndex);
                lines[cursorYIndex - 1] = previousLine + currentLine;
                MoveCursor(previousLine.Length, cursorYIndex - 1);
            }
            else
            {
                string currentLine = lines[cursorYIndex];

                // Remove character behind index and move cursor backwards.
                lines[cursorYIndex] = currentLine.Remove(cursorXIndex - 1, 1);
                MoveCursor(cursorXIndex - 1, cursorYIndex);
            }
        }

        // Left/right.
        if (obj.KeyCode == (int)GlKeys.Left)
        {
            ClearSelection();

            if (cursorXIndex == 0 && cursorYIndex == 0) return; // Nothing to move to.

            if (cursorXIndex == 0)
            {
                MoveCursor(lines[cursorYIndex - 1].Length, cursorYIndex - 1);
            }
            else
            {
                MoveCursor(cursorXIndex - 1, cursorYIndex);
            }
        }

        if (obj.KeyCode == (int)GlKeys.Right)
        {
            ClearSelection();

            if (cursorXIndex == lines[cursorYIndex].Length && cursorYIndex == lines.Count - 1) return; // Nothing to move to.

            if (cursorXIndex == lines[cursorYIndex].Length)
            {
                MoveCursor(0, cursorYIndex + 1);
            }
            else
            {
                MoveCursor(cursorXIndex + 1, cursorYIndex);
            }
        }

        // Up/down.
        if (obj.KeyCode == (int)GlKeys.Up)
        {
            ClearSelection();

            if (cursorYIndex == 0) return; // Nothing to move to.
            int xIndex = Math.Min(cursorXIndex, lines[cursorYIndex - 1].Length);
            MoveCursor(xIndex, cursorYIndex - 1);
        }

        if (obj.KeyCode == (int)GlKeys.Down)
        {
            ClearSelection();

            if (cursorYIndex == lines.Count - 1) return; // Nothing to move to.
            int xIndex = Math.Min(cursorXIndex, lines[cursorYIndex + 1].Length);
            MoveCursor(xIndex, cursorYIndex + 1);
        }

        // Tab.
        if (obj.KeyCode == (int)GlKeys.Tab)
        {
            TryDeleteSelection();

            lines[cursorYIndex] = lines[cursorYIndex].Insert(cursorXIndex, "    ");
            MoveCursor(cursorXIndex + 4, cursorYIndex);
        }

        if (obj.CtrlPressed)
        {
            // Select all.
            if (obj.KeyCode == (int)GlKeys.A)
            {
                selectStart = new Vector2i(0, 0);
                selectEnd = new Vector2i(lines[^1].Length, lines.Count - 1);
                return;
            }

            // Copy.
            if (obj.KeyCode == (int)GlKeys.C && Selecting)
            {
                StringBuilder sb = new();
                MinAndMaxSelection(out Vector2i minSelection, out Vector2i maxSelection);
                for (int i = minSelection.Y; i < maxSelection.Y + 1; i++)
                {
                    string line = lines[i];
                    if (i == minSelection.Y)
                    {
                        sb.Append(line[minSelection.X..]);
                    }
                    else if (i == maxSelection.Y)
                    {
                        sb.Append(line[..maxSelection.X]);
                    }
                    else
                    {
                        sb.Append(line);
                    }

                    if (i != maxSelection.Y)
                    {
                        sb.Append('\n');
                    }
                }

                MainAPI.Capi.Forms.SetClipboardText(sb.ToString());

                return;
            }

            if (obj.KeyCode == (int)GlKeys.V)
            {
                TryDeleteSelection();

                string[] splitLines = MainAPI.Capi.Forms.GetClipboardText().Split('\n');

                string beforeText = lines[cursorYIndex][..cursorXIndex];
                string afterText = lines[cursorYIndex][cursorXIndex..];

                lines[cursorYIndex] = beforeText + splitLines[0];

                for (int i = 1; i < splitLines.Length; i++)
                {
                    lines.Insert(cursorYIndex + i, splitLines[i]);
                }

                if (splitLines.Length > 1)
                {
                    lines.Insert(cursorYIndex + splitLines.Length, afterText);
                    MoveCursor(0, cursorYIndex + splitLines.Length);
                }
                else
                {
                    lines[cursorYIndex] += afterText;
                    MoveCursor(cursorXIndex + splitLines[0].Length, cursorYIndex);
                }

                return;
            }
        }
    }

    private void GuiEvents_KeyPress(KeyEvent obj)
    {
        if (!active || obj.Handled) return;

        obj.Handled = true;

        TryDeleteSelection();

        string currentLine = lines[cursorYIndex];

        char character = obj.KeyChar;

        if (limitTextToBox && font.GetLineWidth(currentLine, fontScale) + (font.GetGlyph(character).xAdvance * fontScale) > Width)
        {
            // If the new character would exceed the bounds, skip.
            return;
        }

        if (insertMode)
        {
            if (cursorXIndex == currentLine.Length)
            {
                currentLine += character;
            }
            else
            {
                // Maybe just use string builders for everything.
                StringBuilder lineBuilder = new(currentLine);
                lineBuilder[cursorXIndex] = character;
                currentLine = lineBuilder.ToString();
            }
        }
        else
        {
            currentLine = currentLine.Insert(cursorXIndex, character.ToString());
        }

        lines[cursorYIndex] = currentLine;

        MoveCursor(cursorXIndex + 1, cursorYIndex);
    }

    /// <summary>
    /// Tries to delete the selection, placing the cursor at the start of the selection.
    /// </summary>
    public void TryDeleteSelection()
    {
        if (!Selecting) return;

        MinAndMaxSelection(out Vector2i minSelection, out Vector2i maxSelection);

        if (minSelection.Y == maxSelection.Y)
        {
            string line = lines[minSelection.Y];
            lines[minSelection.Y] = line.Remove(minSelection.X, maxSelection.X - minSelection.X);
            MoveCursor(minSelection.X, minSelection.Y);
        }
        else
        {
            string firstLine = lines[minSelection.Y][..minSelection.X];
            string secondLine = lines[maxSelection.Y][maxSelection.X..];
            lines[minSelection.Y] = firstLine + secondLine;
            for (int i = maxSelection.Y; i > minSelection.Y; i--)
            {
                lines.RemoveAt(i);
            }
            MoveCursor(minSelection.X, minSelection.Y);
        }

        ClearSelection();
    }

    public void MinAndMaxSelection(out Vector2i min, out Vector2i max)
    {
        if (selectStart.Y < selectEnd.Y || (selectStart.Y == selectEnd.Y && selectStart.X < selectEnd.X))
        {
            min = selectStart;
            max = selectEnd;
        }
        else
        {
            min = selectEnd;
            max = selectStart;
        }
    }

    /// <summary>
    /// Move the cursor and re-calculate the position.
    /// </summary>
    public void MoveCursor(int xIndex, int yIndex)
    {
        cursorXIndex = xIndex;
        cursorYIndex = yIndex;

        float lineHeight = font.LineHeight * fontScale;

        cursorRelativePosition.X = font.GetLineWidthUpToIndex(lines[yIndex], fontScale, xIndex);
        cursorRelativePosition.Y = (yIndex * lineHeight) + lineHeight;
    }

    public override void Dispose()
    {
        cursorTexture.Dispose();
        selectTexture.Dispose();
    }
}