using OpenTK.Graphics.OpenGL4;

namespace MareLib;

/// <summary>
/// Unused.
/// </summary>
public struct GLState
{
    public static GLState CurrentGlState { get; private set; }

    public BlendState bs = BlendState.None;
    public DepthState ds = DepthState.Less;
    public CullState cs = CullState.Back;

    public GLState(BlendState bs, DepthState ds, CullState cs)
    {
        CurrentGlState = this;
        this.bs = bs;
        this.ds = ds;
        this.cs = cs;
    }

    public readonly void Use()
    {
        if (CurrentGlState.bs != bs)
        {
            switch (bs)
            {
                case BlendState.None:
                    GL.Disable(EnableCap.Blend);
                    break;
                case BlendState.Alpha:
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                    break;
                case BlendState.Additive:
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                    break;
                case BlendState.Multiply:
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.DstColor, BlendingFactor.Zero);
                    break;
            }
        }

        if (CurrentGlState.ds != ds)
        {
            if (ds == DepthState.None)
            {
                GL.Disable(EnableCap.DepthTest);
            }
            else
            {
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc((DepthFunction)ds);
            }
        }

        if (CurrentGlState.cs != cs)
        {
            if (cs == CullState.NoCulling)
            {
                GL.Disable(EnableCap.CullFace);
            }
            else
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace((CullFaceMode)cs);
            }
        }

        CurrentGlState = this;
    }
}

public enum BlendState
{
    None,
    Alpha,
    Additive,
    Multiply
}

public enum DepthState
{
    None = 512,
    Less,
    Equal,
    LessOrEqual,
    Greater,
    NotEqual,
    GreaterOrEqual,
    Always
}

public enum CullState
{
    Front = 1028,
    Back = 1029,
    CullAll = 1032,
    NoCulling
}