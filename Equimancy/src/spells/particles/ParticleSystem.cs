using MareLib;
using OpenTK.Mathematics;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace Equimancy;

public interface IParticleSystem
{
    public void UpdateParticles(float dt);
    public void Dispose();
    public bool Alive { get; }
}

/// <summary>
/// Non-singleton particle system that will handle spawning and rendering particles.
/// Takes a struct which will be used as a particle instance.
/// </summary>
public class ParticleSystem<T, I> : IParticleSystem where T : unmanaged where I : unmanaged
{
    protected readonly Queue<T> particleQueue = new();
    protected readonly MappedUboHandle<I> particleUbo;
    public bool Alive { get; private set; } = true;
    public int ActiveParticles { get; protected set; }

    public ParticleSystem()
    {
        particleUbo = new MappedUboHandle<I>(64);
        MainAPI.GetGameSystem<ParticleManager>(EnumAppSide.Client).RegisterSystem(this);
    }

    /// <summary>
    /// Emit particles once.
    /// May be called on an interval, or by subemitters when a particle event happens.
    /// </summary>
    public virtual void Emit(Vector3d position)
    {

    }

    /// <summary>
    /// Called before rendering at an interval.
    /// </summary>
    public virtual void UpdateParticles(float dt)
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

    public virtual void Dispose()
    {
        particleUbo.Dispose();
        Alive = false;
    }
}