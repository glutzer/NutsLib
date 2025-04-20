using MareLib;
using OpenTK.Mathematics;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Equimancy;

/// <summary>
/// Non-singleton particle system that will handle spawning and rendering particles.
/// Takes T, a particle instance that will be updated, and I, an instance on the GPU.
/// </summary>
public class ParticleSystem<T, I> : IParticleSystem where T : unmanaged where I : unmanaged
{
    protected readonly Queue<T> particleQueue = new();
    protected readonly MappedUboHandle<I> particleUbo;
    public int ActiveParticles { get; protected set; }

    public long InstanceId { get; private set; }
    private readonly EnumRenderStage stage;

    public ParticleSystem(EnumRenderStage stage)
    {
        particleUbo = new MappedUboHandle<I>(64);

        ParticleManager manager = MainAPI.GetGameSystem<ParticleManager>(EnumAppSide.Client);
        InstanceId = manager.GetNextInstance();

        manager.RegisterRenderer(this, stage);

        this.stage = stage;
    }

    /// <summary>
    /// Register it to receive events.
    /// </summary>
    public void RegisterEmitter()
    {
        MainAPI.GetGameSystem<ParticleManager>(EnumAppSide.Client).RegisterEmitter(this);
    }

    /// <summary>
    /// Emit particles once.
    /// May be called on an interval, or by sub-emitters when a particle event happens.
    /// </summary>
    public virtual void Emit(Vector3d position)
    {

    }

    /// <summary>
    /// Add a particle to the queue and resize the ubo to fit.
    /// </summary>
    protected void AddParticle(T particle)
    {
        // Resize ubo to fit more particles.
        if (particleQueue.Count == particleUbo.length)
        {
            particleUbo.Resize(particleUbo.length * 2);
        }

        particleQueue.Enqueue(particle);
    }

    /// <summary>
    /// Dispose gpu data, queue system to be removed by manager.
    /// </summary>
    public virtual void Dispose()
    {
        particleUbo.Dispose();

        ParticleManager manager = MainAPI.GetGameSystem<ParticleManager>(EnumAppSide.Client);

        manager.UnregisterEmitter(this);
        manager.UnregisterRenderer(this, stage);
    }

    public virtual void UpdateEmitter(float dt)
    {

    }

    /// <summary>
    /// Update all gpu particle instances here.
    /// </summary>
    public virtual void BeforeFrame(float dt)
    {

    }

    /// <summary>
    /// Render particles on opaque or oit.
    /// </summary>
    public virtual void Render(float dt)
    {

    }
}