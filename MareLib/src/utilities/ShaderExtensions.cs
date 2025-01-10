using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using Vintagestory.Client.NoObf;

namespace MareLib;

public static class ShaderExtensions
{
    public static void Uniform(this ShaderProgram program, ReadOnlySpan<char> location, Vector2 vector)
    {
        GL.Uniform2(program.uniformLocations[location.ToString()], vector);
    }

    public static void Uniform(this ShaderProgram program, ReadOnlySpan<char> location, Vector3 vector)
    {
        GL.Uniform3(program.uniformLocations[location.ToString()], vector);
    }

    public static void Uniform(this ShaderProgram program, ReadOnlySpan<char> location, Vector4 vector)
    {
        GL.Uniform4(program.uniformLocations[location.ToString()], vector);
    }

    public static void Uniform(this ShaderProgram program, ReadOnlySpan<char> location, Matrix3x4 matrix)
    {
        GL.UniformMatrix3x4(program.uniformLocations[location.ToString()], false, ref matrix);
    }

    public static void Uniform(this ShaderProgram program, ReadOnlySpan<char> location, Matrix4 matrix)
    {
        GL.UniformMatrix4(program.uniformLocations[location.ToString()], false, ref matrix);
    }

    public static void BindTexture(this ShaderProgram program, Texture texture, ReadOnlySpan<char> samplerName, int textureUnit = 0)
    {
        GL.ActiveTexture((TextureUnit)((int)TextureUnit.Texture0 + textureUnit));
        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
        program.Uniform(samplerName.ToString(), 0);
    }
}