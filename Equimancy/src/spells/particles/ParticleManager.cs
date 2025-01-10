using Equimancy.src.spells.particles;
using MareLib;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;

namespace Equimancy;

/// <summary>
/// Calls updates on particle systems.
/// Particle systems will register themselves in a constructor, and remove themselves when disposed.
/// </summary>
[GameSystem(forSide = EnumAppSide.Client)]
public class ParticleManager : GameSystem, IRenderer
{
    private readonly Queue<IParticleSystem> activeSystems = new();

    public event Action<float>? RenderOpaque;
    public event Action<float>? RenderOit;
    public event Action<float>? RenderPost;

    public ParticleManager(bool isServer, ICoreAPI api) : base(isServer, api)
    {
    }

    public override void OnStart()
    {
        _ = new TestParticleSystem();

        MareShaderRegistry.AddShader("equimancy:billboardparticles", "equimancy:billboardparticles", "billboardparticles");
    }

    /// <summary>
    /// Register a system to update.
    /// Disposed systems will be removed on update.
    /// </summary>
    public void RegisterSystem(IParticleSystem system)
    {
        activeSystems.Enqueue(system);
        if (activeSystems.Count == 1) BeginRendering(); // Added the first system, begin rendering.
    }

    private void BeginRendering()
    {
        MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.Before);
        MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.Opaque);
        MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.OIT);
        MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.AfterPostProcessing);
    }

    private void StopRendering()
    {
        MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.Before);
        MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
        MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.OIT);
        MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.AfterPostProcessing);
    }

    public void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        if (stage == EnumRenderStage.Before)
        {
            for (int i = 0; i < activeSystems.Count; i++)
            {
                IParticleSystem system = activeSystems.Dequeue();

                if (!system.Alive)
                {
                    if (activeSystems.Count == 0)
                    {
                        MainAPI.Capi.Event.EnqueueMainThreadTask(StopRendering, "StopRendering");
                    }

                    continue;
                }

                system.UpdateParticles(dt);
                activeSystems.Enqueue(system);
            }
        }

        ShaderProgramBase currentShader = ShaderProgramBase.CurrentShaderProgram;

        if (stage == EnumRenderStage.Opaque)
        {
            RenderOpaque?.Invoke(dt);
        }

        if (stage == EnumRenderStage.OIT)
        {
            RenderOit?.Invoke(dt);
        }

        if (stage == EnumRenderStage.AfterPostProcessing)
        {
            RenderPost?.Invoke(dt);
        }

        if (ShaderProgramBase.CurrentShaderProgram != currentShader) currentShader.Use();
    }

    public override void OnClose()
    {
        foreach (IParticleSystem system in activeSystems)
        {
            system.Dispose();
        }

        activeSystems.Clear();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public double RenderOrder => 0.5;
    public int RenderRange => 0;
}