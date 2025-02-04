using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

namespace MareLib;

public struct BindingIndex
{
    public int bindingPoint;
    public int bindId; // <--- What the original value was.

    public BindingIndex(int bindingPoint, int bindId)
    {
        this.bindingPoint = bindingPoint;
        this.bindId = bindId;
    }
}

/// <summary>
/// Constructed from a ShaderProgram.
/// </summary>
public class MareShader
{
    // Set when re-registering shader.
    public BindingIndex[]? UniformBlockIds { get; private set; }

    public int ProgramId { get; private set; }
    public Dictionary<string, int> uniformLocations = null!;
    public Dictionary<string, int> textureLocations = null!;

    public MareShader()
    {

    }

    /// <summary>
    /// When reloading shaders, reset the program id.
    /// </summary>
    public void SetProgram(ShaderProgram program)
    {
        ProgramId = program.ProgramId;
        uniformLocations = program.uniformLocations;
        textureLocations = program.textureLocations;

        // Uniforms blocks.
        GL.GetProgram(program.ProgramId, GetProgramParameterName.ActiveUniformBlocks, out int numUniformBlocks);

        if (numUniformBlocks > 0)
        {
            UniformBlockIds = new BindingIndex[numUniformBlocks];

            for (int i = 0; i < numUniformBlocks; i++)
            {
                GL.GetActiveUniformBlock(program.ProgramId, i, ActiveUniformBlockParameter.UniformBlockNameLength, out int nameLength);
                GL.GetActiveUniformBlockName(program.ProgramId, i, nameLength, out _, out string uniformBlockName);

                GL.GetActiveUniformBlock(program.ProgramId, i, ActiveUniformBlockParameter.UniformBlockBinding, out int bindingPoint);

                // Register it, returns the id of the ubo at that index.
                UniformBlockIds[i] = new BindingIndex(bindingPoint, UboRegistry.RegisterUboName(uniformBlockName));
            }
        }
        else
        {
            UniformBlockIds = null;
        }
    }

    /// <summary>
    /// Use a shader, will stop the current shader if one is active.
    /// Binds the gl state and syncs ubos.
    /// </summary>
    public void Use()
    {
        ShaderProgramBase.CurrentShaderProgram?.Stop();
        GL.UseProgram(ProgramId);

        if (UniformBlockIds != null)
        {
            UboRegistry.SyncUbos(UniformBlockIds);
        }
    }

    public void Uniform(ReadOnlySpan<char> name, int value)
    {
        GL.Uniform1(uniformLocations[name.ToString()], value);
    }

    public void Uniform(ReadOnlySpan<char> name, float value)
    {
        GL.Uniform1(uniformLocations[name.ToString()], value);
    }

    public void Uniform(ReadOnlySpan<char> name, int[] value)
    {
        GL.Uniform1(uniformLocations[name.ToString()], value.Length, value);
    }

    public void Uniform(ReadOnlySpan<char> name, float[] value)
    {
        GL.Uniform1(uniformLocations[name.ToString()], value.Length, value);
    }

    public void Uniform(ReadOnlySpan<char> name, Vector2 value)
    {
        GL.Uniform2(uniformLocations[name.ToString()], value.X, value.Y);
    }

    public void Uniform(ReadOnlySpan<char> name, Vector3 value)
    {
        GL.Uniform3(uniformLocations[name.ToString()], value.X, value.Y, value.Z);
    }

    public void Uniform(ReadOnlySpan<char> name, Vector4 value)
    {
        GL.Uniform4(uniformLocations[name.ToString()], value.X, value.Y, value.Z, value.W);
    }

    public void Uniform(ReadOnlySpan<char> name, Matrix4 value)
    {
        GL.UniformMatrix4(uniformLocations[name.ToString()], false, ref value);
    }

    public void Uniform(ReadOnlySpan<char> name, Matrix3x4 value)
    {
        GL.UniformMatrix3x4(uniformLocations[name.ToString()], false, ref value);
    }

    public unsafe void UniformMatrix(ReadOnlySpan<char> name, float[] matrix)
    {
        fixed (float* ptr = matrix)
        {
            GL.UniformMatrix4(uniformLocations[name.ToString()], 1, false, ptr);
        }
    }

    public void BindTexture(int textureId, ReadOnlySpan<char> samplerName, int unit)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + unit);
        GL.BindTexture(TextureTarget.Texture2D, textureId);

        // Tell uniform sampler to use that texture slot.
        GL.Uniform1(uniformLocations[samplerName.ToString()], unit);
    }

    public void BindTexture(int textureId, ReadOnlySpan<char> samplerName)
    {
        int id = textureLocations[samplerName.ToString()];

        GL.ActiveTexture(TextureUnit.Texture0 + id);
        GL.BindTexture(TextureTarget.Texture2D, textureId);

        // Tell uniform sampler to use that texture slot.
        GL.Uniform1(uniformLocations[samplerName.ToString()], id);
    }

    public void BindTexture(Texture texture, ReadOnlySpan<char> samplerName, int unit)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + unit);
        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

        // Tell uniform sampler to use that texture slot.
        GL.Uniform1(uniformLocations[samplerName.ToString()], unit);
    }

    public void BindTexture(Texture texture, ReadOnlySpan<char> samplerName)
    {
        int id = textureLocations[samplerName.ToString()];

        GL.ActiveTexture(TextureUnit.Texture0 + id);
        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

        // Tell uniform sampler to use that texture slot.
        GL.Uniform1(uniformLocations[samplerName.ToString()], id);
    }

    /// <summary>
    /// Uniforms vanilla sets through shaders for lighting.
    /// Also fog/ambient, as these are needed for lighting.
    /// </summary>
    public void LightUniforms(bool fogAmbient = true)
    {
        DefaultShaderUniforms shaderUniforms = ScreenManager.Platform.ShaderUniforms;

        Uniform("zNear", shaderUniforms.ZNear);
        Uniform("zFar", shaderUniforms.ZFar);
        ObsoleteUniform("lightPosition", shaderUniforms.LightPosition3D);
        Uniform("shadowIntensity", shaderUniforms.DropShadowIntensity);
        Uniform("glitchStrength", shaderUniforms.GlitchStrength);

        if (ShaderProgramBase.shadowmapQuality > 0)
        {
            FrameBufferRef shadowFarTex = ScreenManager.Platform.FrameBuffers[11];
            FrameBufferRef shadowNearTex = ScreenManager.Platform.FrameBuffers[12];

            BindTexture(shadowFarTex.DepthTextureId, "shadowMapFar");
            BindTexture(shadowNearTex.DepthTextureId, "shadowMapNear");

            Uniform("shadowMapWidthInv", 1f / shadowFarTex.Width);
            Uniform("shadowMapHeightInv", 1f / shadowFarTex.Height);
            Uniform("viewDistance", (float)ClientSettings.ViewDistance);
            Uniform("viewDistanceLod0", Math.Min(640, ClientSettings.ViewDistance) * ClientSettings.LodBias);
        }

        int fogSphereQuantity = shaderUniforms.FogSphereQuantity;
        Uniform("fogSphereQuantity", fogSphereQuantity);
        ObsoleteUniform("fogSpheres", fogSphereQuantity * 8, shaderUniforms.FogSpheres);
        int pointLightsCount = shaderUniforms.PointLightsCount;
        Uniform("pointLightQuantity", pointLightsCount);
        ObsoleteUniform3("pointLights", pointLightsCount, shaderUniforms.PointLights3);
        ObsoleteUniform3("pointLightColors", pointLightsCount, shaderUniforms.PointLightColors3);
        Uniform("flatFogDensity", shaderUniforms.FlagFogDensity);
        Uniform("flatFogStart", shaderUniforms.FlatFogStartYPos - shaderUniforms.PlayerPos.Y);
        Uniform("glitchStrengthFL", shaderUniforms.GlitchStrength);
        Uniform("viewDistance", (float)ClientSettings.ViewDistance);
        Uniform("viewDistanceLod0", Math.Min(640, ClientSettings.ViewDistance) * ClientSettings.LodBias);
        Uniform("nightVisionStrength", shaderUniforms.NightVisionStrength);

        if (!fogAmbient) return;

        ObsoleteUniform("rgbaFogIn", MainAPI.Capi.Render.FogColor);
        ObsoleteUniform("rgbaAmbientIn", MainAPI.Capi.Render.AmbientColor);
        Uniform("fogMinIn", MainAPI.Capi.Render.FogMin);
        Uniform("fogDensityIn", MainAPI.Capi.Render.FogDensity);
    }

    /// <summary>
    /// Uniforms for rendering to the shadow map.
    /// Or sampling it.
    /// </summary>
    public void ShadowUniforms()
    {
        DefaultShaderUniforms shaderUniforms = ScreenManager.Platform.ShaderUniforms;

        Uniform("shadowRangeNear", shaderUniforms.ShadowRangeNear);
        Uniform("shadowRangeFar", shaderUniforms.ShadowRangeFar);
        UniformMatrix("toShadowMapSpaceMatrixNear", shaderUniforms.ToShadowMapSpaceMatrixNear);
        UniformMatrix("toShadowMapSpaceMatrixFar", shaderUniforms.ToShadowMapSpaceMatrixFar);
    }

    private void ObsoleteUniform3(string uniformName, int count, float[] values)
    {
        GL.Uniform3(uniformLocations[uniformName], count, values);
    }

    private void ObsoleteUniform(string uniformName, int count, float[] value)
    {
        GL.Uniform1(uniformLocations[uniformName], count, value);
    }

    private void ObsoleteUniform(string uniformName, Vec3f value)
    {
        GL.Uniform3(uniformLocations[uniformName], value.X, value.Y, value.Z);
    }

    private void ObsoleteUniform(string uniformName, Vec4f value)
    {
        GL.Uniform4(uniformLocations[uniformName], value.X, value.Y, value.Z, value.W);
    }
}
