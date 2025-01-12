namespace Equimancy;

/// <summary>
/// Used to register to the particle manager.
/// Could be a particle group, single system, special system with a special renderer.
/// </summary>
public interface IParticleSystem
{
    public long InstanceId { get; }
    public void UpdateEmitter(float dt);
    public void BeforeFrame(float dt);
    public void Render(float dt);
}