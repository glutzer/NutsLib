using MareLib;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace Equimancy;

[GameSystem(0, EnumAppSide.Client)]
public class DistortionSystem : GameSystem, IRenderer
{
    private readonly FboHandle distortionFbo;
    private readonly MeshHandle fullscreen = RenderTools.GetFullscreenTriangle();

    public event Action<float, MareShader>? OnRender;

    public DistortionSystem(bool isServer, ICoreAPI api) : base(isServer, api)
    {
        MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.AfterFinalComposition);

        distortionFbo = new FboHandle(MainAPI.RenderWidth, MainAPI.RenderHeight);
        MainAPI.OnWindowResize += (width, height) =>
        {
            distortionFbo.SetDimensions(width, height);
        };

        distortionFbo.AddAttachment(FramebufferAttachment.ColorAttachment0).AddAttachment(FramebufferAttachment.DepthAttachment);

        MareShaderRegistry.AddShader("equimancy:blit", "equimancy:blit", "blit");
        MareShaderRegistry.AddShader("equimancy:distortion", "equimancy:distortion", "distortion");
        MareShaderRegistry.AddShader("equimancy:distortionanimated", "equimancy:distortion", "distortionanimated");
    }

    public double RenderOrder => 100;
    public int RenderRange => 0;

    // Render onto primary before it is blitted to default.

    public void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        // Get current state.
        FrameBufferRef primary = RenderTools.GetFramebuffer(EnumFrameBuffer.Primary);
        IShaderProgram current = ShaderProgramBase.CurrentShaderProgram;

        // Render distortion.
        distortionFbo.Bind(FramebufferTarget.Framebuffer);

        // Depth write turned off in block outline.
        RenderTools.EnableDepthWrite();

        GL.ClearColor(new Color4(0, 0, 0, 0));
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        MareShader distortion = MareShaderRegistry.Get("distortion");
        distortion.Use();

        distortion.BindTexture(primary.ColorTextureIds[0], "primary", 0);
        distortion.BindTexture(primary.DepthTextureId, "depth", 1);

        distortion.Uniform("time", MainAPI.Client.ElapsedMilliseconds / 1000f);
        distortion.Uniform("resolution", new Vector2(MainAPI.RenderWidth, MainAPI.RenderHeight));

        RenderTools.EnableDepthTest();
        RenderTools.EnableCulling();

        OnRender?.Invoke(dt, distortion); // Event called where everything can be rendered distorted.
        Patches.RendererPatch.RenderDistortion(distortion, dt); // Special invisibility shader test.

        // Clean up.
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, primary.FboId);

        MareShader blit = MareShaderRegistry.Get("blit");
        blit.Use();

        blit.BindTexture(distortionFbo[FramebufferAttachment.ColorAttachment0].Handle, "tex2d", 0);

        GL.BindVertexArray(fullscreen.vaoId);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        GL.BindVertexArray(0);

        current?.Use();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        fullscreen.Dispose();
        distortionFbo.Dispose();
    }
}