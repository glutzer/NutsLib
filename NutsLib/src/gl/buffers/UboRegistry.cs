using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace NutsLib;

/// <summary>
/// Manages ubos for shader.
/// </summary>
public static class UboRegistry
{
    // Id of a ubo to the current handle bound for that id.
    private static readonly int[] idToHandle = new int[256];
    private static readonly int[] idToCurrentBind = new int[256];

    // Ubo name to it's id.
    private static readonly Dictionary<string, int> uboNameToId = [];

    private static int currentId = 0;

    /// <summary>
    /// Called by shader registry, registers a ubo name.
    /// Returns id.
    /// </summary>
    public static int RegisterUboName(string name)
    {
        if (!uboNameToId.TryGetValue(name, out int id))
        {
            id = currentId++;
            uboNameToId.Add(name, id);
        }

        return id;
    }

    /// <summary>
    /// Tries to get a ubo, or registers it if it doesn't exist.
    /// </summary>
    public static int GetUboId(string name)
    {
        if (!uboNameToId.TryGetValue(name, out int id))
        {
            id = RegisterUboName(name);
        }

        return id;
    }

    /// <summary>
    /// Sets a ubo and binds it.
    /// </summary>
    public static void SetUbo(int id, int handle)
    {
        idToHandle[id] = handle;
        int bindIndex = idToCurrentBind[id];
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindIndex, handle);
    }

    /// <summary>
    /// Sets a ubo and binds it.
    /// </summary>
    public static void SetUbo(int id, IUbo ubo)
    {
        idToHandle[id] = ubo.Handle;
        int bindIndex = idToCurrentBind[id];
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindIndex, ubo.Handle);
    }

    /// <summary>
    /// Sets a ubo and binds it.
    /// Registers it if it doesn't exist.
    /// Only set before shader use or while relevant one is active.
    /// </summary>
    public static void SetUbo(string name, IUbo ubo)
    {
        if (!uboNameToId.TryGetValue(name, out int id))
        {
            id = RegisterUboName(name);
        }
        idToHandle[id] = ubo.Handle;
        int bindIndex = idToCurrentBind[id];
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindIndex, ubo.Handle);
    }

    /// <summary>
    /// Sets a ubo and binds it.
    /// Registers it if it doesn't exist.
    /// Only set before shader use or while relevant one is active.
    /// </summary>
    public static void SetUbo(string name, int handle)
    {
        if (!uboNameToId.TryGetValue(name, out int id))
        {
            id = RegisterUboName(name);
        }
        idToHandle[id] = handle;
        int bindIndex = idToCurrentBind[id];
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindIndex, handle);
    }

    /// <summary>
    /// Syncs needed ubos for the current shader when using it.
    /// </summary>
    public static void SyncUbos(BindingIndex[] bindIndices)
    {
        for (uint i = 0; i < bindIndices.Length; i++)
        {
            int bindingPoint = bindIndices[i].bindingPoint;

            int idToBind = bindIndices[i].bindId;
            idToCurrentBind[idToBind] = bindingPoint;
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, idToHandle[idToBind]);
        }
    }

    public static void Dispose()
    {
        uboNameToId.Clear();
        Array.Clear(idToHandle);
        Array.Clear(idToCurrentBind);
    }
}