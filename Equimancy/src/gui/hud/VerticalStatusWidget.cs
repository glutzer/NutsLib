using MareLib;
using OpenTK.Mathematics;
using SkiaSharp;
using System;

namespace Equimancy;

// Bar for stuff like mana.
public class VerticalStatusWidget : Widget
{
    private readonly MareShader barShader;
    private readonly NineSliceTexture barOverlay;

    public MeshHandle newQuad;

    public Func<float> getBarStatus;

    public VerticalStatusWidget(Gui gui, Bounds bounds, Func<float> getBarStatus) : base(gui, bounds)
    {
        barShader = MareShaderRegistry.Get("statusbar");

        // Flipped uv.
        newQuad = QuadMeshUtility.CreateGuiQuadMesh(vertex =>
        {
            Vector2 uv = new(vertex.uv.X, 1 - vertex.uv.Y);
            return new GuiVertex(vertex.position, uv);
        });

        barOverlay = TextureBuilder.Begin(32, 32)
            .SetColor(new SKColor(50, 50, 0, 255))
            .StrokeMode(8)
            .DrawEmbossedRectangle(0, 0, 32, 32, true)
            .End()
            .AsNineSlice(8, 8);

        this.getBarStatus = getBarStatus;
    }

    public override void OnRender(float dt, MareShader shader)
    {
        barShader.Use();

        barShader.Uniform("color", new Vector4(0, 0, 1, 1));
        barShader.Uniform("progress", getBarStatus());
        barShader.Uniform("time", MainAPI.Capi.World.ElapsedMilliseconds / 1000f);

        RenderTools.RenderElement(barShader, bounds.X, bounds.Y, bounds.Width, bounds.Height, newQuad);

        shader.Use();

        RenderTools.RenderNineSlice(barOverlay, shader, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    public override void Dispose()
    {
        newQuad.Dispose();
        barOverlay.Dispose();
    }
}