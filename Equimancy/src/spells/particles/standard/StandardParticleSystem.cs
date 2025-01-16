using MareLib;
using OpenTK.Mathematics;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Equimancy;

public class StandardParticleSystem : ParticleSystem<StandardParticle, StandardParticleInstance>
{
    public const int MAX_PARTICLES = 1000;

    private readonly MeshHandle centeredMesh;
    private readonly ParticleConfig config;
    private readonly Random rand = new();
    private Accumulator emitter;

    private readonly Texture texture;


    public Vector3d position;

    public StandardParticleSystem(ParticleConfig config, Vector3d position) : base(EnumRenderStage.OIT)
    {
        this.config = config;

        centeredMesh = QuadMeshUtility.CreateCenteredQuadMesh(vertex =>
        {
            return new GuiVertex(vertex.position, vertex.uv);
        });
        this.position = position;

        emitter = Accumulator.WithInterval(Math.Max(0.01f, config.emitInterval));

        try
        {
            texture = ClientCache.GetOrCache($"particle_{config.texture}", () => Texture.Create(config.texture, false));
        }
        catch
        {
            texture = ClientCache.GetOrCache($"particle_equimancy:textures/spark1.png", () => Texture.Create("equimancy:textures/spark1.png", false));
        }
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
            particle.rotation += particle.angularVelocity * dt;

            particle.velocity *= 0.98f; // Make drag configurable.
            particle.angularVelocity *= 1 - config.angularDrag;

            particle.velocity.Y -= config.gravity;

            particle.age += dt;

            if (particle.Elapsed > 1) continue;

            particleQueue.Enqueue(particle);
        }

        // Update gpu.
        int index = 0;

        for (int i = 0; i < particleQueue.Count; i++)
        {
            // Skip the queue when maximum particles are reached.
            if (index == MAX_PARTICLES)
            {
                break;
            }

            StandardParticle particle = particleQueue.Dequeue();
            float elapsed = particle.Elapsed;

            int ACTUAL_VALUE = MainAPI.Client.blockAccessor.GetLightRGBsAsInt((int)particle.position.X, (int)particle.position.Y, (int)particle.position.Z);
            Vector4 light = new((ACTUAL_VALUE & 0xFF) / 255f, ((ACTUAL_VALUE >> 8) & 0xFF) / 255f, ((ACTUAL_VALUE >> 16) & 0xFF) / 255f, ((ACTUAL_VALUE >> 24) & 0xFF) / 255f);

            particleUbo[index] = new StandardParticleInstance()
            {
                position = RenderTools.CameraRelativePosition(particle.position),
                color = Vector4.Lerp(config.startColor, config.endColor, elapsed),
                light = light,
                scaleRot = new Vector2(GameMath.Lerp(config.startSize * 2, config.endSize * 2, elapsed), particle.rotation)
            };

            index++;

            particleQueue.Enqueue(particle);
        }
    }

    public override void Render(float dt)
    {
        MareShader shader = MareShaderRegistry.Get("billboardparticles");

        // Set ubo before using a shader, otherwise it may be disposed?
        UboRegistry.SetUbo("billboardParticles", particleUbo);

        shader.Use();

        shader.LightUniforms();
        shader.ShadowUniforms();

        shader.Uniform("glowAmount", config.glowAmount);

        shader.BindTexture(texture, "tex2d");

        RenderTools.RenderMeshInstanced(centeredMesh, particleQueue.Count);
    }

    public override void Emit(Vector3d position)
    {
        int toEmit = config.particlesToEmit + rand.Next(config.particlesToAdd);

        for (int i = 0; i < toEmit; i++)
        {
            float addX = -1 + 2 * rand.NextSingle();
            float addY = -1 + 2 * rand.NextSingle();
            float addZ = -1 + 2 * rand.NextSingle();

            StandardParticle particle = new()
            {
                position = position,
                velocity = config.startVelocity + new Vector3(addX, addY, addZ).Normalized() * config.startVelocityAdd,
                angularVelocity = config.angularVelocityStart + (-config.angularVelocityAdd + (config.angularVelocityAdd * 2 * rand.NextSingle())),
                lifetime = config.particleLifetime
            };

            particle.position += (new Vector3d(-1) + new Vector3d(rand.NextSingle() * 2, rand.NextSingle() * 2, rand.NextSingle() * 2)).Normalized() * config.emitRadius * rand.NextSingle();

            AddParticle(particle);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        centeredMesh.Dispose();
    }
}