using MareLib;
using OpenTK.Mathematics;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace Equimancy;

public class LevitateInstance
{
    public Entity entity = null!;
    public Vector4 color;
    public bool alive = true;
    public float age;
}

[FX]
public class FXLevitate : FXType<LevitateInstance>
{
    public override EnumRenderStage[] RenderStages => new EnumRenderStage[] { EnumRenderStage.Before, EnumRenderStage.OIT };

    private readonly MareShader levitateShader;
    private readonly MeshHandle cubeMesh;

    public FXLevitate()
    {
        levitateShader = MareShaderRegistry.AddShader("equimancy:spells/levitate", "equimancy:spells/levitate", "levitate");

        cubeMesh = CubeMeshUtility.CreateCenteredCubeMesh(vert =>
        {
            return new StandardVertex(vert.position, vert.uv, vert.normal, Vector4.One);
        });
    }

    public override void OnRenderFrame(float dt, EnumRenderStage stage)
    {
        // Add point lights.
        if (stage == EnumRenderStage.Before)
        {
            ForEachInstance((id, inst) =>
            {
                LightingUtilities.AddPointLight(inst.entity.Pos.ToVector(), inst.color.Xyz * inst.age * 2);
            });
            return;
        }

        ShaderProgramBase lastShader = ShaderProgramBase.CurrentShaderProgram;

        levitateShader.Use();
        levitateShader.Uniform("time", TimeUtility.ElapsedClientSeconds());

        ForEachInstance((id, inst) =>
        {
            if (inst.alive)
            {
                inst.age += dt;
                inst.age = Math.Clamp(inst.age, 0, 0.5f);
            }
            else
            {
                inst.age -= dt;
                if (inst.age < 0) QueueInstanceRemoval(id);
            }

            Vector3d entityPos = inst.entity.Pos.ToVector();

            Cuboidf selBox = inst.entity.SelectionBox;

            entityPos.Y += selBox.YSize / 2;

            Matrix4 modelMat = Matrix4.CreateScale((float)selBox.XSize + 0.5f, (float)selBox.YSize + 0.5f, (float)selBox.ZSize + 0.5f) * RenderTools.CameraRelativeTranslation(entityPos);

            levitateShader.Uniform("modelMatrix", modelMat);
            levitateShader.Uniform("color", inst.color * inst.age * 2);

            RenderTools.RenderMesh(cubeMesh);
        });

        lastShader?.Use();
    }

    private long distortionInstance;

    protected override void EnableRendering()
    {
        base.EnableRendering();
        distortionInstance = MainAPI.GetGameSystem<DistortionSystem>(EnumAppSide.Client).RegisterRenderer(RenderDistortion);
    }

    protected override void DisableRendering()
    {
        base.DisableRendering();
        MainAPI.GetGameSystem<DistortionSystem>(EnumAppSide.Client).UnregisterRenderer(distortionInstance);
    }

    protected void RenderDistortion(float dt, MareShader mareShader)
    {
        ForEachInstance((id, inst) =>
        {
            Vector3d entityPos = inst.entity.Pos.ToVector();

            Cuboidf selBox = inst.entity.SelectionBox;

            entityPos.Y += selBox.YSize / 2;

            Matrix4 modelMat = Matrix4.CreateScale((float)selBox.XSize + 0.6f, (float)selBox.YSize + 0.6f, (float)selBox.ZSize + 0.6f) * RenderTools.CameraRelativeTranslation(entityPos);

            mareShader.Uniform("modelMatrix", modelMat);
            mareShader.Uniform("strength", inst.age);

            RenderTools.RenderMesh(cubeMesh);
        });

        mareShader.Uniform("strength", 1f);
    }

    public override void OnClosing()
    {
        cubeMesh.Dispose();
    }
}