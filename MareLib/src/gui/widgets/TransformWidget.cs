using OpenTK.Mathematics;
using System;
using Vintagestory.API.Client;

namespace MareLib;

/// <summary>
/// Applies a transform to the shader.
/// Transform needed for both to apply transform to mouse events.
/// Delta only supplied when rendering.
/// Takes a delegate that has the delta time and outputs a mat4.
/// </summary>
public class TransformWidget : Widget
{
    private readonly Func<float, Matrix4> transformDelegate;
    private readonly bool entering;

    public TransformWidget(Gui gui, Bounds bounds, Func<float, Matrix4> transformDelegate, bool entering) : base(gui, bounds)
    {
        this.transformDelegate = transformDelegate;
        this.entering = entering;
    }

    public override void RegisterEvents(GuiEvents guiEvents)
    {
        guiEvents.MouseMove += GuiEvents_MouseMove;
        guiEvents.MouseDown += GuiEvents_MouseDown;
        guiEvents.MouseUp += GuiEvents_MouseUp;
        guiEvents.MouseWheel += GuiEvents_MouseWheel;
    }

    private void GuiEvents_MouseWheel(MouseWheelEventArgs obj)
    {
        PushToStack(false);
    }

    private void GuiEvents_MouseUp(MouseEvent obj)
    {
        PushToStack(false);
    }

    private void GuiEvents_MouseDown(MouseEvent obj)
    {
        PushToStack(false);
    }

    private void GuiEvents_MouseMove(MouseEvent obj)
    {
        PushToStack(false);
    }

    private void PushToStack(bool rendering, float dt = 0)
    {
        if (rendering ? !entering : entering)
        {
            RenderTools.GuiTransformStack.Pop();

            if (rendering)
            {
                int doTransform = RenderTools.GuiTransformStack.Count > 1 ? 1 : 0;
                RenderTools.TransformUbo.UpdateData(new TransformData() { doTrans = doTransform, transform = RenderTools.GuiTransformStack.Peek() });
            }
        }
        else
        {
            RenderTools.GuiTransformStack.Push(transformDelegate(dt) * RenderTools.GuiTransformStack.Peek());

            if (rendering)
            {
                RenderTools.TransformUbo.UpdateData(new TransformData() { doTrans = 1, transform = RenderTools.GuiTransformStack.Peek() });
            }
        }
    }

    public override void OnRender(float dt, MareShader shader)
    {
        PushToStack(true, dt);
    }
}