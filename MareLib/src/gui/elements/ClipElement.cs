using OpenTK.Graphics.OpenGL4;
using Vintagestory.Client.NoObf;

namespace MareLib;

public class ClipElement : Widget
{
    public bool clip;

    public ClipElement(Gui gui, bool clip, Bounds bounds) : base(gui, bounds)
    {
        this.clip = clip;
    }

    public override void OnRender(float dt, ShaderProgram shader)
    {
        if (clip)
        {
            PushScissor();
        }
        else
        {
            PopScissor();
        }
    }

    public static bool GLScissorFlagEnabled => GL.IsEnabled(EnableCap.ScissorTest);

    public void PushScissor()
    {
        RenderTools.PushScissor(bounds);
    }

    public static void PopScissor()
    {
        RenderTools.PopScissor();
    }
}