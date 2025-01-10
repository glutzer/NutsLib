using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Vintagestory.Client.NoObf;

namespace MareLib;

/// <summary>
/// Constructed from a ShaderProgram.
/// </summary>
public class MareShader
{
    // Set when re-registering shader.
    public int[]? UniformBlockIds { get; private set; }

    public int ProgramId { get; private set; }
    public Dictionary<string, int> uniformLocations = null!;

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

        // Uniforms blocks.
        GL.GetProgram(program.ProgramId, GetProgramParameterName.ActiveUniformBlocks, out int numUniformBlocks);

        if (numUniformBlocks > 0)
        {
            UniformBlockIds = new int[numUniformBlocks];

            for (int i = 0; i < numUniformBlocks; i++)
            {
                GL.GetActiveUniformBlock(program.ProgramId, i, ActiveUniformBlockParameter.UniformBlockNameLength, out int nameLength);
                GL.GetActiveUniformBlockName(program.ProgramId, i, nameLength, out _, out string uniformBlockName);

                // Register it, returns the id of the ubo at that index.
                UniformBlockIds[i] = UboRegistry.RegisterUboName(uniformBlockName);
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

    /// <summary>
    /// Bind a texture to the shader.
    /// Not used with bindless.
    /// </summary>
    public void BindTexture(int textureId, ReadOnlySpan<char> samplerName, int unit = 0)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + unit);
        GL.BindTexture(TextureTarget.Texture2D, textureId);

        // Tell uniform sampler to use that texture slot.
        GL.Uniform1(uniformLocations[samplerName.ToString()], unit);
    }

    public void BindTexture(Texture texture, ReadOnlySpan<char> samplerName, int unit = 0)
    {
        GL.ActiveTexture(TextureUnit.Texture0 + unit);
        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);

        // Tell uniform sampler to use that texture slot.
        GL.Uniform1(uniformLocations[samplerName.ToString()], unit);
    }
}
