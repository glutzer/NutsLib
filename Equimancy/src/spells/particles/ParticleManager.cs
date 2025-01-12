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
    // Instance event mappings.
    private readonly Dictionary<long, IParticleSystem> emitterSystems = new(); // Receives updates (Register).
    private readonly Dictionary<long, IParticleSystem> opaqueSystems = new(); // Renders opaque particles.
    private readonly Dictionary<long, IParticleSystem> oitSystems = new(); // Renders OIT particles.

    private long nextInstanceId;
    public long GetNextInstance()
    {
        return nextInstanceId++;
    }

    public ParticleManager(bool isServer, ICoreAPI api) : base(isServer, api)
    {
    }

    public override void OnStart()
    {
        MareShaderRegistry.AddShader("equimancy:billboardparticles", "equimancy:billboardparticles", "billboardparticles");
    }

    /// <summary>
    /// Register a system to call update before every frame.
    /// </summary>
    public void RegisterEmitter(IParticleSystem system)
    {
        emitterSystems.Add(GetNextInstance(), system);
        if (emitterSystems.Count == 1) MainAPI.Capi.Event.RegisterRenderer(this, EnumRenderStage.Before);
    }

    public void UnregisterEmitter(IParticleSystem system)
    {
        emitterSystems.Remove(system.InstanceId);
        if (emitterSystems.Count == 0) MainAPI.Capi.Event.UnregisterRenderer(this, EnumRenderStage.Before);
    }

    public void RegisterRenderer(IParticleSystem system, EnumRenderStage stage)
    {
        if (stage == EnumRenderStage.Opaque)
        {
            opaqueSystems.Add(system.InstanceId, system);
            if (opaqueSystems.Count == 1)
            {
                MainAPI.Capi.Event.RegisterRenderer(this, stage);
            }
        }
        else
        {
            oitSystems.Add(system.InstanceId, system);
            if (oitSystems.Count == 1)
            {
                MainAPI.Capi.Event.RegisterRenderer(this, stage);
            }
        }
    }

    public void UnregisterRenderer(IParticleSystem system, EnumRenderStage stage)
    {
        if (stage == EnumRenderStage.Opaque)
        {
            opaqueSystems.Remove(system.InstanceId);
            if (opaqueSystems.Count == 0)
            {
                MainAPI.Capi.Event.UnregisterRenderer(this, stage);
            }
        }
        else
        {
            oitSystems.Remove(system.InstanceId);
            if (oitSystems.Count == 0)
            {
                MainAPI.Capi.Event.UnregisterRenderer(this, stage);
            }
        }
    }

    public void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        if (stage == EnumRenderStage.Before)
        {
            foreach (IParticleSystem system in emitterSystems.Values)
            {
                system.UpdateEmitter(dt);
            }

            //foreach (IParticleSystem system in opaqueSystems.Values)
            //{
            //    system.BeforeFrame(dt);
            //}

            foreach (IParticleSystem system in oitSystems.Values)
            {
                system.BeforeFrame(dt);
            }

            return;
        }

        ShaderProgramBase currentShader = ShaderProgramBase.CurrentShaderProgram;

        //if (stage == EnumRenderStage.Opaque)
        //{
        //    foreach (IParticleSystem system in opaqueSystems.Values)
        //    {
        //        system.Render(dt);
        //    }
        //}

        if (stage == EnumRenderStage.OIT)
        {
            foreach (IParticleSystem system in oitSystems.Values)
            {
                system.Render(dt);
            }
        }

        if (ShaderProgramBase.CurrentShaderProgram != currentShader) currentShader?.Use();
    }

    public override void OnClose()
    {
        // Systems should handle their own disposal.
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public double RenderOrder => 0.5;
    public int RenderRange => 0;
}