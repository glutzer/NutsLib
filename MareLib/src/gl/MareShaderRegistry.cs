using System.Collections.Generic;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace MareLib;

public static class MareShaderRegistry
{
    public static Dictionary<string, ShaderProgram> Shaders { get; } = new();

    public static IShaderProgram RegisterShader(string vertPath, string fragPath, string shaderName, string domain = "marelib")
    {
        ICoreClientAPI capi = MainHook.Capi;

        IShaderProgram shader = capi.Shader.NewShaderProgram();

        MethodInfo method = typeof(ShaderRegistry).GetMethod("HandleIncludes", BindingFlags.NonPublic | BindingFlags.Static)!;
        object[] vertParams = new object[] { shader, capi.Assets.Get($"{domain}:shaders/{vertPath}.vert").ToText(), null! };
        object[] fragParams = new object[] { shader, capi.Assets.Get($"{domain}:shaders/{fragPath}.frag").ToText(), null! };

        shader.VertexShader = capi.Shader.NewShader(EnumShaderType.VertexShader);
        shader.FragmentShader = capi.Shader.NewShader(EnumShaderType.FragmentShader);

        shader.VertexShader.Code = (string)method.Invoke(null, vertParams)!;
        shader.FragmentShader.Code = (string)method.Invoke(null, fragParams)!;

        capi.Shader.RegisterMemoryShaderProgram(shaderName, shader);

        shader.Compile();
        if (Shaders.TryGetValue(shaderName, out _))
        {
            Shaders.Remove(shaderName);
        }

        Shaders.Add(shaderName, (ShaderProgram)shader);

        return shader;
    }

    public static ShaderProgram RegisterShader(string vertPath, string fragPath, string geomPath, string shaderName, string domain = "marelib")
    {
        ICoreClientAPI capi = MainHook.Capi;

        IShaderProgram shader = capi.Shader.NewShaderProgram();

        MethodInfo method = typeof(ShaderRegistry).GetMethod("HandleIncludes", BindingFlags.NonPublic | BindingFlags.Static)!;
        object[] vertParams = new object[] { shader, capi.Assets.Get($"{domain}:shaders/{vertPath}.vert").ToText(), null! };
        object[] fragParams = new object[] { shader, capi.Assets.Get($"{domain}:shaders/{fragPath}.frag").ToText(), null! };
        object[] geomParams = new object[] { shader, capi.Assets.Get($"{domain}:shaders/{geomPath}.geom").ToText(), null! };

        shader.VertexShader = capi.Shader.NewShader(EnumShaderType.VertexShader);
        shader.FragmentShader = capi.Shader.NewShader(EnumShaderType.FragmentShader);
        shader.GeometryShader = capi.Shader.NewShader(EnumShaderType.GeometryShader);

        shader.VertexShader.Code = (string)method.Invoke(null, vertParams)!;
        shader.FragmentShader.Code = (string)method.Invoke(null, fragParams)!;
        shader.GeometryShader.Code = (string)method.Invoke(null, geomParams)!;

        capi.Shader.RegisterMemoryShaderProgram(shaderName, shader);

        shader.Compile();

        if (Shaders.TryGetValue(shaderName, out _))
        {
            Shaders.Remove(shaderName);
        }

        Shaders.Add(shaderName, (ShaderProgram)shader);

        return (ShaderProgram)shader;
    }

    public static void Dispose()
    {
        foreach (ShaderProgram shader in Shaders.Values)
        {
            shader.Dispose();
        }

        Shaders.Clear();
    }
}