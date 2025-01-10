using MareLib;
using OpenTK.Mathematics;
using System;
using Vintagestory.API.Client;

namespace Equimancy;

public class CornerButton : Widget
{
    private readonly Texture texture;
    private readonly Action onClick;
    private bool hovering;

    public CornerButton(Gui gui, Bounds bounds, Action onClick, string texturePath) : base(gui, bounds)
    {
        texture = Texture.Create(texturePath);
        this.onClick = onClick;
    }

    public override void OnRender(float dt, MareShader shader)
    {
        if (!hovering)
        {
            // Faded color when not hovered.
            shader.Uniform("color", new Vector4(0.9f, 0.9f, 0.9f, 0.5f));
        }

        shader.BindTexture(texture.Handle, "tex2d", 0);
        RenderTools.RenderQuad(shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);

        shader.Uniform("color", Vector4.One);
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseMove += GuiEvents_MouseMove;
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        hovering = bounds.IsInside(obj.X, obj.Y);
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        if (!hovering) return;

        obj.Handled = true;
        onClick();
    }

    public override void Dispose()
    {
        texture.Dispose();
    }
}