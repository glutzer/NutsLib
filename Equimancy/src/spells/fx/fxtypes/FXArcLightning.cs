using MareLib;
using OpenTK.Mathematics;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace Equimancy;

/// <summary>
/// Returns if was able to get point.
/// </summary>
public delegate Vector3d GetPointDelegate();

public class ArcLightningInstance
{
    public GetPointDelegate start;
    public GetPointDelegate end;
    public Vector3 color;
    public float lifetime = 0.5f;
    public float age;

    public ArcLightningInstance(GetPointDelegate start, GetPointDelegate end, Vector3 color)
    {
        this.start = start;
        this.end = end;
        this.color = color;
    }
}

/// <summary>
/// Effect that will render a pulsing quad over a lifetime.
/// </summary>
[FX]
public class FXArcLightning : FXType<ArcLightningInstance>
{
    public override EnumRenderStage[] RenderStages => new EnumRenderStage[] { EnumRenderStage.Before, EnumRenderStage.OIT };

    private readonly MeshInfo<PositionVertex> meshInfo = new(4, 6);
    private readonly MeshHandle meshHandle;
    private readonly MareShader shader;
    private readonly Texture arcTexture1;

    public FXArcLightning()
    {
        meshInfo.AddVertex(new PositionVertex(Vector3.Zero, new Vector2(0, 0)));
        meshInfo.AddVertex(new PositionVertex(Vector3.Zero, new Vector2(1, 0)));
        meshInfo.AddVertex(new PositionVertex(Vector3.Zero, new Vector2(0, 1)));
        meshInfo.AddVertex(new PositionVertex(Vector3.Zero, new Vector2(1, 1)));

        meshInfo.AddTriangle(1, 3, 0);
        meshInfo.AddTriangle(0, 3, 2);

        meshHandle = meshInfo.Upload();

        shader = MareShaderRegistry.AddShader("equimancy:arclightning", "equimancy:arclightning", "arclightning");

        arcTexture1 = Texture.Create("equimancy:textures/arc1.png", false, true);
    }

    public override void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        if (stage == EnumRenderStage.Before)
        {
            ForEachInstance((id, inst) =>
            {
                LightingUtilities.AddPointLight(inst.end(), inst.color * (inst.lifetime - inst.age));
            });
            return;
        }

        ShaderProgramBase lastShader = ShaderProgramBase.CurrentShaderProgram;
        shader.Use();

        shader.BindTexture(arcTexture1, "tex2d");

        ForEachInstance((id, instance) =>
        {
            Vector3d start = instance.start();
            Vector3d end = instance.end();

            float width = 1 - (instance.age / instance.lifetime);

            UpdateMesh(start, end, width * 3);

            Matrix4 model = RenderTools.CameraRelativeTranslation(start);

            shader.Uniform("modelMatrix", model);
            shader.Uniform("colorIn", instance.color);

            // Render 2 alternating beams.

            shader.Uniform("time", MainAPI.Capi.World.ElapsedMilliseconds / 1000f * 2);
            RenderTools.RenderMesh(meshHandle);

            shader.Uniform("time", MainAPI.Capi.World.ElapsedMilliseconds / 1000f * 4);
            RenderTools.RenderMesh(meshHandle);

            instance.age += dt;

            if (instance.age >= instance.lifetime)
            {
                QueueInstanceRemoval(id);
                return;
            }
        });

        lastShader?.Use();
    }

    public void UpdateMesh(Vector3d start, Vector3d end, float width)
    {
        Vector3 length = (Vector3)(end - start);

        Vector3 direction = length.Normalized();
        Vector3 cameraToMidpoint = (Vector3)(MainAPI.CameraPosition - start).Normalized();

        Vector3 spread = Vector3.Cross(direction, cameraToMidpoint).Normalized();

        meshInfo.vertices[0].position = Vector3.Zero - (spread * width);
        meshInfo.vertices[1].position = Vector3.Zero + (spread * width);

        meshInfo.vertices[2].position = length - (spread * width);
        meshInfo.vertices[3].position = length + (spread * width);

        RenderTools.UpdateMesh(meshInfo, meshHandle);
    }

    public override void OnClosing()
    {
        meshHandle.Dispose();
        arcTexture1.Dispose();
        particleSystem.Dispose();
    }
}