using MareLib;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Equimancy;

/// <summary>
/// Input box or multiple boxes with a callback to the string input.
/// </summary>
public class InputBox : Widget
{
    public readonly List<WidgetTextBoxSingle> textBoxes = new();
    private readonly NineSliceTexture background;

    public InputBox(Gui gui, Bounds bounds, int boxes, int fontScale, Action<int, string> onNewBoxValue, bool center, params string[] defaultValues) : base(gui, bounds)
    {
        Font font = FontRegistry.GetFont("friz");

        background = EqGui.Box;

        if (boxes > 1)
        {
            float boxWidth = 0.9f / boxes;
            float padding = 0.1f / boxes;

            for (int i = 0; i < boxes; i++)
            {
                int c = i;

                float boundsStart = i * (boxWidth + padding);
                Bounds boxBounds = Bounds.CreateFrom(bounds).Percent(boundsStart, 0f, boxWidth, 1f);

                WidgetTextBoxSingle textBox = new(gui, boxBounds, font, fontScale, Vector4.One, false, center, str =>
                {
                    onNewBoxValue(c, str);
                }, defaultValues[i]);

                AddChild(textBox);
                textBoxes.Add(textBox);
            }
        }
        else
        {
            WidgetTextBoxSingle textBox = new(gui, bounds, font, fontScale, Vector4.One, false, center, str =>
            {
                onNewBoxValue(0, str);
            }, defaultValues[0]);

            AddChild(textBox);
            textBoxes.Add(textBox);
        }
    }

    public override void OnRender(float dt, MareShader shader)
    {
        foreach (WidgetTextBoxSingle widget in textBoxes)
        {
            RenderTools.RenderNineSlice(background, shader, widget.bounds.X, widget.bounds.Y, widget.bounds.Width, widget.bounds.Height);
        }
    }

    public override void Initialize()
    {

    }
}