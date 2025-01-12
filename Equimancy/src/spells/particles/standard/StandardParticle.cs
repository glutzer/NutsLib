using OpenTK.Mathematics;

namespace Equimancy;

public struct StandardParticle
{
    public Vector3d position;
    public Vector3 velocity;
    public float age;
    public float lifetime;

    public readonly float Elapsed => age / lifetime;
}