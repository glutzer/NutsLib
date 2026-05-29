using OpenTK.Graphics.OpenGL4;
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

    /// <summary>
    /// Scans GLSL source for UBO names and registers them with UboRegistry.
    /// The dictionary maps name -> binding point (= registry id), shared across all stages.
    /// </summary>
    private static void CollectUboNames(Dictionary<string, int> uboBindings, string code)
    {
        string pattern = @"layout\(std140\)\s+uniform\s+(\w+)";
        foreach (Match match in Regex.Matches(code, pattern))
        {
            string blockName = match.Groups[1].Value;
            if (!uboBindings.ContainsKey(blockName))
            {
                uboBindings[blockName] = uboBindings.Count; // 0, 1, 2... per shader
            }
        }
    }

    private static void RegisterShader(string vertPath, string fragPath, string? geomPath, string shaderName)
    {
        ICoreClientAPI capi = MainAPI.Capi;

        IShaderProgram shader = capi.Shader.NewShaderProgram();

        MethodInfo method = typeof(ShaderRegistry).GetMethod("HandleIncludes", BindingFlags.NonPublic | BindingFlags.Static)!;
        object[] vertParams = [shader, capi.Assets.Get(vertPath).ToText(), null!];
        object[] fragParams = [shader, capi.Assets.Get(fragPath).ToText(), null!];

        shader.VertexShader = capi.Shader.NewShader(EnumShaderType.VertexShader);
        shader.FragmentShader = capi.Shader.NewShader(EnumShaderType.FragmentShader);

        shader.VertexShader.Code = (string)method.Invoke(null, vertParams)!;
        shader.FragmentShader.Code = (string)method.Invoke(null, fragParams)!;

        // Collect UBO names across all stages before compiling.
        Dictionary<string, int> uboBindings = [];
        CollectUboNames(uboBindings, shader.VertexShader.Code);
        CollectUboNames(uboBindings, shader.FragmentShader.Code);

        if (geomPath != null)
        {
            object[] geomParams = [shader, capi.Assets.Get(geomPath).ToText(), null!];
            shader.GeometryShader = capi.Shader.NewShader(EnumShaderType.GeometryShader);
            shader.GeometryShader.Code = (string)method.Invoke(null, geomParams)!;
            CollectUboNames(uboBindings, shader.GeometryShader.Code);
        }

        capi.Shader.RegisterMemoryShaderProgram(shaderName, shader);

        shader.Compile();

        // After compilation, assign each unique UBO its binding point via the GL API.
        // This avoids needing layout(binding=N) in the GLSL source.
        int programId = ((ShaderProgram)shader).ProgramId;
        foreach ((string blockName, int bindingPoint) in uboBindings)
        {
            int blockIndex = GL.GetUniformBlockIndex(programId, blockName);
            if (blockIndex != -1)
            {
                GL.UniformBlockBinding(programId, blockIndex, bindingPoint);
            }
        }

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