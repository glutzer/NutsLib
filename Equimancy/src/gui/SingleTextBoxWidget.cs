using MareLib;
using OpenTK.Mathematics;
using System;
using System.Text;
using Vintagestory.API.Client;

namespace Equimancy;

/// <summary>
/// Similar to the text box, but only for a single line.
/// A lot of code duplication.
/// </summary>
public class SingleTextBoxWidget : FocusableWidget
{
    private readonly TextObject text;

    private readonly bool limitTextToBox;
    private readonly bool centerValues;
    private readonly Action<string>? onNewText;

    private readonly Texture cursorTexture;
    private readonly Texture selectTexture;

    private bool Selecting => selectStart != selectEnd;

    private int cursorIndex;
    private float cursorRelativePos;
    private bool dragging;

    private int selectStart;
    private int selectEnd;

    private bool insertMode;

    private int CenterOffset
    {
        get
        {
            int center = centerValues ? bounds.Width / 2 - text.PixelLength / 2 : 0;

            if (!centerValues && cursorRelativePos > bounds.Width)
            {
                center -= (int)(cursorRelativePos - bounds.Width);
            }

            return center;
        }
    }

    public SingleTextBoxWidget(Gui gui, Bounds bounds, Font font, int fontScale, Vector4 color, bool limitTextToBox = true, bool centerValues = true, Action<string>? onNewText = null, string? defaultText = null) : base(gui, bounds)
    {
        this.limitTextToBox = limitTextToBox;
        this.centerValues = centerValues;
        this.onNewText = onNewText;

        defaultText ??= "";
        text = new TextObject(defaultText, font, fontScale, color);

        cursorTexture = TextureBuilder.Begin(64, 64)
            .SetColor(SkiaThemes.White.WithAlpha(100))
            .DrawRectangle(0, 0, 64, 64)
            .End();

        selectTexture = TextureBuilder.Begin(64, 64)
            .SetColor(SkiaThemes.White.WithAlpha(100))
            .DrawRectangle(0, 0, 64, 64)
            .End();
    }

    public override void OnRender(float dt, MareShader shader)
    {
        RenderTools.PushScissor(bounds);

        float lineHeight = text.font.LineHeight * text.fontScale;
        float centerOffset = CenterOffset;

        if (centerValues)
        {
            text.RenderCenteredLine(bounds.X + bounds.Width / 2, bounds.Y + lineHeight, shader);
        }
        else
        {
            int x = bounds.X;

            if (cursorRelativePos > bounds.Width)
            {
                x -= (int)(cursorRelativePos - bounds.Width);
            }

            text.RenderLine(x, bounds.Y + lineHeight, shader, 0);
        }

        if (Focused)
        {
            shader.BindTexture(cursorTexture.Handle, "tex2d", 0);

            if (insertMode && cursorIndex != text.Text.Length)
            {
                char charAtIndex = text.Text[cursorIndex];
                float charWidth = text.font.fontCharData[charAtIndex].xAdvance * text.fontScale;
                RenderTools.RenderQuad(shader, bounds.X + cursorRelativePos + centerOffset, bounds.Y + lineHeight + (lineHeight / 4), charWidth, -lineHeight);
            }
            else if (MainAPI.Capi.World.ElapsedMilliseconds / 1000f % 1 < 0.5f) // Blink.
            {
                RenderTools.RenderQuad(shader, bounds.X + cursorRelativePos + centerOffset, bounds.Y + lineHeight, lineHeight / 2, 4); // Magic numbers.
            }

            // Selection is assumed not to be out of the bounds here.
            if (Selecting)
            {
                shader.BindTexture(selectTexture.Handle, "tex2d", 0);

                MinAndMaxSelection(out int minSelection, out int maxSelection);

                string line = text.Text;

                float start = (int)text.font.GetLineWidthUpToIndex(line, text.fontScale, minSelection);
                float end = (int)text.font.GetLineWidthUpToIndex(line, text.fontScale, maxSelection);
                float y = bounds.Y + lineHeight;
                RenderTools.RenderQuad(shader, bounds.X + start + centerOffset, y, end - start, -lineHeight);
            }
        }

        RenderTools.PopScissor();
    }

    public override void Focus()
    {
        base.Focus();
        MoveCursor(text.Text.Length);
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        base.RegisterEvents(guiEvents);

        guiEvents.KeyDown += GuiEvents_KeyDown;
        guiEvents.KeyPress += GuiEvents_KeyPress;
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseUp += GuiEvents_MouseUp;
        guiEvents.MouseMove += GuiEvents_MouseMove;
    }

    private void GuiEvents_KeyPress(KeyEvent obj)
    {
        if (!Focused || obj.Handled) return;

        obj.Handled = true;

        TryDeleteSelection();

        string lastText = text.Text;

        string currentLine = text.Text;

        char character = obj.KeyChar;

        if (limitTextToBox && text.font.GetLineWidth(currentLine, text.fontScale) + text.font.fontCharData[character].xAdvance * text.fontScale > bounds.Width)
        {
            // If the new character would exceed the bounds, skip.
            return;
        }

        if (insertMode)
        {
            if (cursorIndex == currentLine.Length)
            {
                currentLine += character;
            }
            else
            {
                // Maybe just use string builders for everything.
                StringBuilder lineBuilder = new(currentLine);
                lineBuilder[cursorIndex] = character;
                currentLine = lineBuilder.ToString();
            }
        }
        else
        {
            currentLine = currentLine.Insert(cursorIndex, character.ToString());
        }

        text.Text = currentLine;

        if (lastText != text.Text) onNewText?.Invoke(text.Text);

        MoveCursor(cursorIndex + 1);
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && bounds.IsInAllBounds(obj))
        {
            obj.Handled = true;

            dragging = true;

            int pos = GetIndexAtMousePos(obj.X - CenterOffset);
            selectStart = pos;
            selectEnd = pos;

            MoveCursor(pos);

            if (!Focused) Focus();
        }
        else
        {
            Unfocus();
        }
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        dragging = false;
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (!obj.Handled && bounds.IsInAllBounds(obj))
        {
            // Set cursor.
            gui.MouseOverCursor = "textselect";
        }

        if (dragging)
        {
            selectEnd = GetIndexAtMousePos(obj.X - CenterOffset);
            MoveCursor(selectEnd);
        }
    }

    private int GetIndexAtMousePos(int x)
    {
        x -= bounds.X;
        int index = text.font.GetIndexAtAdvance(text.Text, text.fontScale, x);
        return index;
    }

    public void MoveCursor(int xIndex)
    {
        cursorIndex = xIndex;
        cursorRelativePos = text.font.GetLineWidthUpToIndex(text.Text, text.fontScale, xIndex);
    }

    private void GuiEvents_KeyDown(KeyEvent obj)
    {
        if (!Focused || obj.Handled) return;

        obj.Handled = true;

        string currentText = text.Text;

        HandleSpecialKeys(obj);

        if (text.Text != currentText) onNewText?.Invoke(text.Text);
    }

    public void HandleSpecialKeys(KeyEvent obj)
    {
        if (obj.KeyCode == (int)GlKeys.Delete)
        {
            TryDeleteSelection();

            if (cursorIndex == text.Text.Length) return; // Nothing to delete.

            string currentLine = text.Text;
            text.Text = currentLine.Remove(cursorIndex, 1);

            return;
        }

        if (obj.KeyCode == (int)GlKeys.Insert)
        {
            insertMode = !insertMode;
            return;
        }

        if (obj.KeyCode == (int)GlKeys.Enter)
        {
            TryDeleteSelection();
            Unfocus(); // No line breaks.
            return;
        }

        if (obj.KeyCode == (int)GlKeys.BackSpace)
        {
            // Only remove the selection with backspace.
            if (Selecting)
            {
                TryDeleteSelection();
                return;
            }

            if (cursorIndex == 0) return; // Nothing to remove.

            string currentLine = text.Text;

            // Remove character behind index and move cursor backwards.
            text.Text = currentLine.Remove(cursorIndex - 1, 1);
            MoveCursor(cursorIndex - 1);
        }

        // Left/right.
        if (obj.KeyCode == (int)GlKeys.Left)
        {
            ClearSelection();
            if (cursorIndex == 0) return; // Nothing to move to.
            MoveCursor(cursorIndex - 1);
        }

        if (obj.KeyCode == (int)GlKeys.Right)
        {
            ClearSelection();
            if (cursorIndex == text.Text.Length) return; // Nothing to move to.
            MoveCursor(cursorIndex + 1);
        }

        if (obj.CtrlPressed)
        {
            // Select all.
            if (obj.KeyCode == (int)GlKeys.A)
            {
                selectStart = 0;
                selectEnd = text.Text.Length;
            }

            // Copy.
            if (obj.KeyCode == (int)GlKeys.C && Selecting)
            {
                StringBuilder sb = new();
                MinAndMaxSelection(out int minSelection, out int maxSelection);

                sb.Append(text.Text[minSelection..maxSelection]);

                MainAPI.Capi.Forms.SetClipboardText(sb.ToString());
            }

            if (obj.KeyCode == (int)GlKeys.V)
            {
                TryDeleteSelection();

                StringBuilder sb = new();
                string[] splitLines = MainAPI.Capi.Forms.GetClipboardText().Split('\n');
                foreach (string line in splitLines)
                {
                    sb.Append(line);
                }

                // Insert text at cursor pos.
                string currentLine = text.Text;
                text.Text = currentLine.Insert(cursorIndex, sb.ToString());
                MoveCursor(cursorIndex + sb.Length);
            }
        }
    }

    /// <summary>
    /// Tries to delete the selection, placing the cursor at the start of the selection.
    /// </summary>
    public void TryDeleteSelection()
    {
        if (!Selecting) return;

        MinAndMaxSelection(out int minSelection, out int maxSelection);

        string line = text.Text;
        text.Text = line.Remove(minSelection, maxSelection - minSelection);
        MoveCursor(minSelection);

        ClearSelection();
    }

    public void ClearSelection()
    {
        selectStart = 0;
        selectEnd = 0;
    }

    public void MinAndMaxSelection(out int min, out int max)
    {
        min = Math.Min(selectStart, selectEnd);
        max = Math.Max(selectStart, selectEnd);
    }

    public override void Dispose()
    {
        cursorTexture.Dispose();
        selectTexture.Dispose();
    }
}