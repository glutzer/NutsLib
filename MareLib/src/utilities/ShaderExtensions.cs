using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vintagestory.Client.NoObf;

namespace MareLib;

public static class ShaderExtensions
{
    public static void Uniform(this ShaderProgram program, string location, Vector2 vector)
    {
        //GL.Uniform2(program.uniformLocations[location], vector);
        GL.Uniform2(program.uniformLocations[location], vector.X, vector.Y);
    }

    public static void Uniform(this ShaderProgram program, string location, Vector3 vector)
    {
        //GL.Uniform3(program.uniformLocations[location], vector);
        GL.Uniform3(program.uniformLocations[location], vector.X, vector.Y, vector.Z);
    }

    public static void Uniform(this ShaderProgram program, string location, Vector4 vector)
    {
        //GL.Uniform4(program.uniformLocations[location], vector);
        GL.Uniform4(program.uniformLocations[location], vector.X, vector.Y, vector.Z, vector.W);
    }

    //public static void Uniform(this ShaderProgram program, string location, Matrix3x4 matrix)
    //{
    //    //GL.UniformMatrix3x4(program.uniformLocations[location], false, ref matrix);
    //}

    public static unsafe void Uniform(this ShaderProgram program, string location, Matrix4 matrix)
    {
        //GL.UniformMatrix4(program.uniformLocations[location], false, ref matrix);
        // Think this works...
        GL.UniformMatrix4(program.uniformLocations[location], 1, false, &matrix.Row0.X);
    }

    public static void BindTexture(this ShaderProgram program, Texture texture, string samplerName, int textureUnit = 0)
    {
        GL.ActiveTexture((TextureUnit)((int)TextureUnit.Texture0 + textureUnit));
        GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
        program.Uniform(samplerName, 0);
    }
}