using OpenTK.Mathematics;

namespace Equimancy;

/// <summary>
/// All settings for a particle system.
/// Immutable.
/// </summary>
public class ParticleConfig
{
    /// <summary>
    /// How much particle acceleration is pulled down.
    /// </summary>
    public float gravity = 0;

    /// <summary>
    /// How long to emit particles for. -1 means forever.
    /// Only emits if registered.
    /// </summary>
    public float emitDuration = -1;

    /// <summary>
    /// Starting velocity for each particle.
    /// </summary>
    public Vector3 startVelocity = new(0, 0, 0);

    /// <summary>
    /// Amount to add or subtract randomly.
    /// </summary>
    public Vector3 startVelocityAdd = new(0, 0, 0);

    public float startSize = 1f;

    public float endSize = 1f;

    public Vector4 startColor = Vector4.One;

    public Vector4 endColor = new(1, 1, 1, 0);

    public int particlesToEmit = 5;
    public int particlesToAdd = 0;

    public float emitRadius = 1;

    public float emitInterval = 0.2f;

    public float particleLifetime = 1f;

    public bool emitConstantly = false;
}