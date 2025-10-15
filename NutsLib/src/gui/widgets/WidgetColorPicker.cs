using OpenTK.Mathematics;
using System;
using Vintagestory.API.Client;

namespace NutsLib;

public class WidgetHueSlider : WidgetBaseSlider
{
    private readonly Texture texture;

    public Vector3 color;

    public WidgetHueSlider(Widget? parent, Gui gui, Action<int> onNewValue, int steps, float currentHue, Vector3 currentColor) : base(parent, gui, onNewValue, steps)
    {
        texture = TextureBuilder.Begin(16, 16).SetColor(1f, 1f, 1f, 1f).FillMode().DrawRectangle(0, 0, 16, 16).End();

        float huePercent = Math.Clamp(currentHue / 360f, 0f, 1f);
        cursorStep = (int)Math.Round(huePercent * steps);

        color = currentColor;
    }

    public override void OnRender(float dt, NuttyShader shader)
    {
        shader.BindTexture(texture, "tex2d");

        shader.Uniform("color", new Vector4(color, 1f));
        RenderTools.RenderQuad(shader, X, Y, Width, Height);

        float percentage = Percentage;
        float percentWidth = Width / 20f;

        Vector3 oppositeColor = new();

        float offsetableWidth = Width - percentWidth;
        float offset = offsetableWidth * percentage;

        shader.Uniform("color", new Vector4(oppositeColor, 0.5f));
        RenderTools.RenderQuad(shader, X + offset, Y, percentWidth, Height);

        shader.Uniform("color", Vector4.One);
    }

    public override void Dispose()
    {
        texture.Dispose();
    }
}

public class WidgetColorPicker : Widget
{
    public bool dragging;
    private Vector3 currentHsv = new(0f, 1f, 1f);
    public Action<Vector3> onNewColor;
    public TextObject textObj;

    private readonly WidgetHueSlider slider;

    public WidgetColorPicker(Widget? parent, Gui gui, Action<Vector3> onNewColor, Vector3 currentColor) : base(parent, gui)
    {
        this.onNewColor = onNewColor;
        currentHsv = ColorUtility.RgbToHsv(currentColor);

        slider = new WidgetHueSlider(this, gui, i =>
        {
            if (currentHsv.X != i)
            {
                currentHsv.X = i;
                ColorChanged();
            }
        }, 360, currentHsv.X, currentColor);

        slider.Alignment(Align.CenterBottom, AlignFlags.OutsideV);
        slider.PercentWidth(1f);
        slider.PercentHeight(0.2f);

        textObj = new($"{currentColor.X}, {currentColor.Y}, {currentColor.Z}", FontRegistry.GetFont("friz"), 4 * MainAPI.GuiScale, new Vector4(0.8f, 0.8f, 0.8f, 1f));
    }

    public void ColorChanged()
    {
        Vector3 newColor = ColorUtility.HsvToRgb(currentHsv.X, currentHsv.Y, currentHsv.Z);

        newColor.X = MathF.Round(newColor.X, 3);
        newColor.Y = MathF.Round(newColor.Y, 3);
        newColor.Z = MathF.Round(newColor.Z, 3);

        slider.color = newColor;

        textObj.Text = $"{newColor.X}, {newColor.Y}, {newColor.Z}";

        onNewColor(newColor);
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseUp += GuiEvents_MouseUp;
        guiEvents.MouseMove += GuiEvents_MouseMove;
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (dragging)
        {
            Vector3 oldHsv = currentHsv;

            // Get pos relative to X, Y.
            float x = (obj.X - X) / (float)Width;
            float y = (obj.Y - Y) / (float)Height;

            x = Math.Clamp(x, 0f, 1f);
            y = Math.Clamp(y, 0f, 1f);

            currentHsv.Y = x;
            currentHsv.Z = 1f - y;

            if (oldHsv != currentHsv)
            {
                ColorChanged();
            }
        }
    }

    public override void OnRender(float dt, NuttyShader shader)
    {
        NuttyShader colorShader = NuttyShaderRegistry.Get("colorwheelgui");
        colorShader.Use();

        colorShader.Uniform("hue", currentHsv.X);
        RenderTools.RenderQuad(colorShader, X, Y, Width, Height);

        shader.Use();

        if (dragging)
        {
            textObj.RenderLine(Gui.MouseX, Gui.MouseY, shader);
        }
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        dragging = false;
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && IsInAllBounds(obj)) dragging = true;
    }
}