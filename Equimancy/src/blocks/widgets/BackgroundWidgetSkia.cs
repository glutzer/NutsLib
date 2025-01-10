using MareLib;
using System;
using Vintagestory.API.Client;

namespace Equimancy;

/// <summary>
/// Provides a simple background.
/// No nine slicing.
/// "equimancy:textures/singlepage.png"
/// </summary>
public class BackgroundWidgetSkia : Widget
{
    private readonly Texture texture;

    public BackgroundWidgetSkia(Gui gui, Bounds bounds, Func<Texture> textureDelegate) : base(gui, bounds)
    {
        texture = textureDelegate();
    }

    public override void OnRender(float dt, MareShader shader)
    {
        shader.BindTexture(texture.Handle, "tex2d", 0);
        RenderTools.RenderQuad(shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    // Handle events.

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseMove += GuiEvents_MouseMove;
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        if (bounds.IsInsideAndClip(obj))
        {
            obj.Handled = true;
        }
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!obj.Handled && bounds.IsInsideAndClip(obj))
        {
            obj.Handled = true;
        }
    }

    public override void Dispose()
    {
        texture.Dispose();
    }
}