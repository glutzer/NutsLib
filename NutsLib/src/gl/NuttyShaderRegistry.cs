using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Vintagestory.API.Client;
using Vintagestory.Client.NoObf;

namespace NutsLib;

public class ShaderEntry
{
    public string vertPath;
    public string fragPath;
    public string? geomPath;
    public string shaderName;

    public ShaderEntry(string vertPath, string fragPath, string? geomPath, string shaderName)
    {
        string[] vertInfo = vertPath.Split(':');
        if (vertInfo.Length != 2) vertInfo = ["game", vertInfo[0]];

        string[] fragInfo = fragPath.Split(':');
        if (fragInfo.Length != 2) fragInfo = ["game", fragInfo[0]];

        if (geomPath != null)
        {
            string[] geomInfo = geomPath.Split(':');
            if (geomInfo.Length != 2) geomInfo = ["game", geomInfo[0]];
            this.geomPath = $"{geomInfo[0]}:shaders/{geomInfo[1]}.geom";
        }

        this.vertPath = $"{vertInfo[0]}:shaders/{vertInfo[1]}.vert";
        this.fragPath = $"{fragInfo[0]}:shaders/{fragInfo[1]}.frag";

        this.shaderName = shaderName;
    }
}

public static class NuttyShaderRegistry
{
    public static Dictionary<string, NuttyShader> Shaders { get; } = [];
    private static readonly List<ShaderEntry> shaderEntries = [];
    private static bool initialized = false;

    /// <summary>
    /// Get a shader.
    /// </summary>
    public static NuttyShader Get(string name)
    {
        return Shaders[name];
    }

    /// <summary>
    /// Get a typed shader.
    /// </summary>
    public static T Get<T>(string name) where T : NuttyShader
    {
        return (T)Shaders[name];
    }

    /// <summary>
    /// Add a shader, it will be available to use once initialized.
    /// Automatically reloaded.
    /// Example path: "nutslib:gui" - same as nutslib:shaders/gui.vert.
    /// </summary>
    public static NuttyShader AddShader(string vertPath, string fragPath, string shaderName, string? geomPath = null)
    {
        if (!initialized) Initialize();

        shaderEntries.Add(new ShaderEntry(vertPath, fragPath, geomPath, shaderName));
        Shaders.Add(shaderName, new NuttyShader());
        return Shaders[shaderName];
    }

    /// <summary>
    /// Add a shader, it will be available to use once initialized.
    /// Automatically reloaded.
    /// Example path: "nutslib:gui" - same as nutslib:shaders/gui.vert.
    /// </summary>
    public static T AddShader<T>(string vertPath, string fragPath, string shaderName, string? geomPath = null) where T : NuttyShader, new()
    {
        if (!initialized) Initialize();

        shaderEntries.Add(new ShaderEntry(vertPath, fragPath, geomPath, shaderName));
        Shaders.Add(shaderName, new T());
        return (T)Shaders[shaderName];
    }

    public static void Initialize()
    {
        MainAPI.Capi.Event.ReloadShader += () =>
        {
            foreach (ShaderEntry entry in shaderEntries)
            {
                RegisterShader(entry.vertPath, entry.fragPath, entry.geomPath, entry.shaderName);
            }

            return true;
        };
    }

    public static string SetUBOBindings(Dictionary<string, int> uniqueBlocks, string code)
    {
        // There is actually a way better way to do this.

        string pattern = @"layout\(std140\)\s+uniform\s+(\w+)";

        return Regex.Replace(code, pattern, match =>
        {
            string blockDefinition = match.Groups[0].Value;
            if (!uniqueBlocks.TryGetValue(blockDefinition, out int id))
            {
                id = uniqueBlocks.Count;
                uniqueBlocks[blockDefinition] = id;
            }
            string modifiedBlock = blockDefinition.Replace("layout(std140)", $"layout(std140, binding = {id})");

            return modifiedBlock;
        });
    }

    private static void RegisterShader(string vertPath, string fragPath, string? geomPath, string shaderName)
    {
        ICoreClientAPI capi = MainAPI.Capi;

        IShaderProgram shader = capi.Shader.NewShaderProgram();

        MethodInfo method = typeof(ShaderRegistry).GetMethod("HandleIncludes", BindingFlags.NonPublic | BindingFlags.Static)!;
        object[] vertParams = [shader, capi.Assets.Get(vertPath).ToText(), null!];
        object[] fragParams = [shader, capi.Assets.Get(fragPath).ToText(), null!];

        Dictionary<string, int> uniqueBlocks = [];

        shader.VertexShader = capi.Shader.NewShader(EnumShaderType.VertexShader);
        shader.FragmentShader = capi.Shader.NewShader(EnumShaderType.FragmentShader);

        shader.VertexShader.Code = (string)method.Invoke(null, vertParams)!;
        shader.FragmentShader.Code = (string)method.Invoke(null, fragParams)!;

        shader.VertexShader.Code = SetUBOBindings(uniqueBlocks, shader.VertexShader.Code);
        shader.FragmentShader.Code = SetUBOBindings(uniqueBlocks, shader.FragmentShader.Code);

        if (geomPath != null)
        {
            object[] geomParams = [shader, capi.Assets.Get(geomPath).ToText(), null!];
            shader.GeometryShader = capi.Shader.NewShader(EnumShaderType.GeometryShader);
            shader.GeometryShader.Code = (string)method.Invoke(null, geomParams)!;
            shader.GeometryShader.Code = SetUBOBindings(uniqueBlocks, shader.GeometryShader.Code);
        }

        capi.Shader.RegisterMemoryShaderProgram(shaderName, shader);

        shader.Compile();

        // Set relevant shader info.
        NuttyShader nuShader = Shaders[shaderName];
        nuShader.SetProgram((ShaderProgram)shader);
    }

    public static void Dispose()
    {
        // Registered shaders are already disposed by the game.

        initialized = false;
        Shaders.Clear();
        shaderEntries.Clear();
    }
}