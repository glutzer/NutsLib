using MareLib;
using OpenTK.Mathematics;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Equimancy.src.spells.particles;

public class TestParticleSystem : ParticleSystem<Particle, BillboardParticleInstance>
{
    private readonly MeshHandle centeredMesh;

    public TestParticleSystem()
    {
        MainAPI.GetGameSystem<ParticleManager>(EnumAppSide.Client).RenderOpaque += Opaque;

        centeredMesh = QuadMeshUtility.CreateCenteredQuadMesh(vertex =>
        {
            return new GuiVertex(vertex.position * 0.2f, vertex.uv);
        });
    }

    private void Opaque(float dt)
    {
        int index = 0;

        foreach (Particle particle in particleQueue)
        {
            particleUbo[index] = new BillboardParticleInstance()
            {
                position = RenderTools.CameraRelativePosition(particle.position)
            };

            index++;
        }

        UboRegistry.SetUbo("billboardParticles", particleUbo);

        // Get particle shader.
        // Render.

        MareShaderRegistry.Get("billboardparticles").Use();

        Vec3d pos = MainAPI.Capi.World.Player.Entity.Pos.XYZ;
        Vector3d pos2 = new(pos.X, pos.Y, pos.Z);

        Emit(pos2);

        RenderTools.RenderMeshInstanced(centeredMesh, index);
    }

    public override void UpdateParticles(float dt)
    {
        for (int i = 0; i < particleQueue.Count; i++)
        {
            Particle particle = particleQueue.Dequeue();
            particle.position += particle.velocity * dt;
            particle.velocity *= 0.98f;
            particle.timeLeft -= dt;

            if (particle.timeLeft < 0) continue;

            particleQueue.Enqueue(particle);
        }
    }

    public override void Emit(Vector3d position)
    {
        Particle particle = new()
        {
            position = position,
            velocity = new Vector3(0, 0.5f, 0),
            timeLeft = 2
        };

        AddParticle(particle);
    }

    public override void Dispose()
    {
        base.Dispose();
        
        centeredMesh.Dispose();
    }
}