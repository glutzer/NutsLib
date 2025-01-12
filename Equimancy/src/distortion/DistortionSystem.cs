using MareLib;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace Equimancy;

[GameSystem(0, EnumAppSide.Client)]
public class DistortionSystem : GameSystem, IRenderer
{
    private readonly FboHandle distortionFbo;
    private readonly MeshHandle fullscreen = RenderTools.GetFullscreenTriangle();

    private readonly Dictionary<long, Action<float, MareShader>> renderCallbacks = new();
    private long nextInstanceId;

    public DistortionSystem(bool isServer, ICoreAPI api) : base(isServer, api)
    {
        distortionFbo = new FboHandle(MainAPI.RenderWidth, MainAPI.RenderHeight);
        MainAPI.OnWindowResize += (width, height) =>
        {
            distortionFbo.SetDimensions(width, height);
        };
        distortionFbo.AddAttachment(FramebufferAttachment.ColorAttachment0)
            .AddAttachment(FramebufferAttachment.DepthAttachment);

        // Write

        MareShaderRegistry.AddShader("equimancy:blit", "equimancy:blit", "blit");
        MareShaderRegistry.AddShader("equimancy:distortion/distortion", "equimancy:distortion/distortion", "distortion");
        MareShaderRegistry.AddShader("equimancy:distortion/distortionanimated", "equimancy:distortion/distortion", "distortionanimated");
    }

    /// <summary>
    /// Registers a renderer for distortion, passes in the default distortion shader.
    /// </summary>
    public long RegisterRenderer(Action<float, MareShader> renderCallback)
    {
        long id = nextInstanceId;
        renderCallbacks.Add(id, renderCallback);
        nextInstanceId++;

        if (renderCallbacks.Count == 1)
        {
            MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.AfterFinalComposition);
        }

        return id;
    }

    // Convert this to render after post processing later, so bloom is distorted.

    public void UnregisterRenderer(long instanceId)
    {
        renderCallbacks.Remove(instanceId);

        if (renderCallbacks.Count == 0)
        {
            MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.AfterFinalComposition);
        }
    }

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

        distortion.Uniform("time", TimeUtility.ElapsedClientSeconds());
        distortion.Uniform("resolution", new Vector2(MainAPI.RenderWidth, MainAPI.RenderHeight));
        distortion.Uniform("useTexture", 0);

        RenderTools.EnableDepthTest();
        RenderTools.EnableCulling();

        foreach (Action<float, MareShader> action in renderCallbacks.Values)
        {
            action(dt, distortion);
        }

        // Clean up.
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, primary.FboId);

        // Blit to primary texture.
        MareShader blit = MareShaderRegistry.Get("blit");
        blit.Use();
        blit.BindTexture(distortionFbo[FramebufferAttachment.ColorAttachment0].Handle, "tex2d", 0);
        GL.BindVertexArray(fullscreen.vaoId);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        GL.BindVertexArray(0);

        // Use old shader.
        current?.Use();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        fullscreen.Dispose();
        distortionFbo.Dispose();
    }

    public double RenderOrder => 100;
    public int RenderRange => 0;
}