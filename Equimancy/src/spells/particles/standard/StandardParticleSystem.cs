using MareLib;
using OpenTK.Mathematics;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Equimancy;

public class StandardParticleSystem : ParticleSystem<StandardParticle, StandardParticleInstance>
{
    private readonly MeshHandle centeredMesh;
    private readonly ParticleConfig config;
    private readonly Random rand = new();
    private Accumulator emitter;

    public Vector3d position;

    public StandardParticleSystem(ParticleConfig config, Vector3d position) : base(EnumRenderStage.OIT)
    {
        this.config = config;
        centeredMesh = QuadMeshUtility.CreateCenteredQuadMesh(vertex =>
        {
            return new GuiVertex(vertex.position, vertex.uv);
        });
        this.position = position;

        emitter = Accumulator.WithInterval(config.emitInterval);
    }

    public override void UpdateEmitter(float dt)
    {
        emitter.Add(dt);
        while (emitter.Consume())
        {
            Emit(position);
        }
    }

    public override void BeforeFrame(float dt)
    {
        // Update position.
        for (int i = 0; i < particleQueue.Count; i++)
        {
            StandardParticle particle = particleQueue.Dequeue();
            particle.position += particle.velocity * dt;

            particle.velocity *= 0.98f; // Make drag configurable.
            particle.age += dt;

            if (particle.age > particle.lifetime) continue;

            particleQueue.Enqueue(particle);
        }

        // Update gpu.
        int index = 0;

        for (int i = 0; i < particleQueue.Count; i++)
        {
            StandardParticle particle = particleQueue.Dequeue();

            float elapsed = particle.Elapsed;

            if (elapsed > 1) continue; // Possibly call particle death event here.

            particleUbo[index] = new StandardParticleInstance()
            {
                position = RenderTools.CameraRelativePosition(particle.position),
                color = Vector4.Lerp(config.startColor, config.endColor, elapsed),
                scale = GameMath.Lerp(config.startSize * 2, config.endSize * 2, elapsed)
            };

            index++;

            particleQueue.Enqueue(particle);
        }
    }

    public override void Render(float dt)
    {
        MareShader shader = MareShaderRegistry.Get("billboardparticles");
        shader.Use();

        UboRegistry.SetUbo("billboardParticles", particleUbo);

        RenderTools.RenderMeshInstanced(centeredMesh, particleQueue.Count);
    }

    public override void Emit(Vector3d position)
    {
        int toEmit = config.particlesToEmit + rand.Next(config.particlesToAdd);

        for (int i = 0; i < toEmit; i++)
        {
            StandardParticle particle = new()
            {
                position = position,
                velocity = config.startVelocity + (-config.startVelocityAdd + (config.startVelocityAdd * 2 * rand.NextSingle())),
                lifetime = config.particleLifetime
            };

            particle.position += (new Vector3d(-1) + new Vector3d(rand.NextSingle() * 2, rand.NextSingle() * 2, rand.NextSingle() * 2)).Normalized() * config.emitRadius;

            AddParticle(particle);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        centeredMesh.Dispose();
    }
}