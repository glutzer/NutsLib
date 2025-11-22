using OpenTK.Mathematics;

namespace NutsLib;

public class ShaderGui : NuttyShader
{
    // Vert shader.
    [Uniform] protected int italicSlant;
    public float ItalicSlant { set => Uniform(italicSlant, value); }

    [Uniform] protected int removeDepth;
    public bool RemoveDepth { set => Uniform(removeDepth, value ? 1 : 0); }

    // Frag shader.
    [Uniform] protected int shaderType;
    public int ShaderType { set => Uniform(shaderType, value); }

    [Uniform] protected int color;
    public Vector4 Color { set => Uniform(color, value); }

    [Uniform] protected int fontColor;
    public Vector4 FontColor { set => Uniform(fontColor, value); }

    // 9-slicing.
    [Uniform] protected int dimensions;
    public Vector4 Dimensions { set => Uniform(dimensions, value); }

    [Uniform] protected int border;
    public Vector4 Border { set => Uniform(border, value); }

    [Uniform] protected int centerScale;
    public Vector2 CenterScale { set => Uniform(centerScale, value); }

    [Uniform] protected int bold;
    public int Bold { set => Uniform(bold, value); }

    [Uniform] protected int blendToColorMap;
    protected float BlendToColorMap { set => Uniform(blendToColorMap, value); }

    public ShaderGui() : base()
    {
    }

    public void ResetColor()
    {
        Color = Vector4.One;
    }

    public void ResetFontColor()
    {
        FontColor = Vector4.One;
    }

    public void SetColorMap(Texture colorMap, float blendToColorMap)
    {
        BindTexture(colorMap, "colorMap");
        BlendToColorMap = blendToColorMap;
    }

    public void RemoveColorMap()
    {
        BindTexture(0, "colorMap");
        BlendToColorMap = 0f;
    }
}