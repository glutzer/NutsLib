using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace NutsLib;

/// <summary>
/// Manages Ssbos for shader.
/// </summary>
public static class SsboRegistry
{
    // Id of a ubo to the current handle bound for that id.
    private static readonly int[] idToHandle = new int[256];
    private static readonly int[] idToCurrentBind = new int[256];

    // Ssbo name to it's id.
    private static readonly Dictionary<string, int> SsboNameToId = [];

    private static int currentId;

    /// <summary>
    /// Called by shader registry, registers an Ssbo name.
    /// Returns id.
    /// </summary>
    public static int RegisterSsboName(string name)
    {
        if (!SsboNameToId.TryGetValue(name, out int id))
        {
            id = currentId++;
            SsboNameToId.Add(name, id);
        }

        return id;
    }

    /// <summary>
    /// Tries to get an Ssbo, or registers it if it doesn't exist.
    /// </summary>
    public static int GetSsboId(string name)
    {
        if (!SsboNameToId.TryGetValue(name, out int id))
        {
            id = RegisterSsboName(name);
        }

        return id;
    }

    /// <summary>
    /// Sets an Ssbo and binds it.
    /// </summary>
    public static void SetSsbo(int id, int handle)
    {
        idToHandle[id] = handle;
        int bindIndex = idToCurrentBind[id];
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindIndex, handle);
    }

    /// <summary>
    /// Sets an Ssbo and binds it.
    /// </summary>
    public static void SetSsbo(int id, ISsbo Ssbo)
    {
        idToHandle[id] = Ssbo.Handle;
        int bindIndex = idToCurrentBind[id];
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindIndex, Ssbo.Handle);
    }

    /// <summary>
    /// Sets an Ssbo and binds it.
    /// Registers it if it doesn't exist.
    /// Only set before shader use or while relevant one is active.
    /// </summary>
    public static void SetSsbo(string name, ISsbo Ssbo)
    {
        if (!SsboNameToId.TryGetValue(name, out int id))
        {
            id = RegisterSsboName(name);
        }

        idToHandle[id] = Ssbo.Handle;
        int bindIndex = idToCurrentBind[id];
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindIndex, Ssbo.Handle);
    }

    /// <summary>
    /// Sets an Ssbo and binds it.
    /// Registers it if it doesn't exist.
    /// Only set before shader use or while relevant one is active.
    /// </summary>
    public static void SetSsbo(string name, int handle)
    {
        if (!SsboNameToId.TryGetValue(name, out int id))
        {
            id = RegisterSsboName(name);
        }

        idToHandle[id] = handle;
        int bindIndex = idToCurrentBind[id];
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindIndex, handle);
    }

    /// <summary>
    /// Syncs needed Ssbos for the current shader when using it.
    /// </summary>
    public static void SyncSsbos(BindingIndex[] bindIndices)
    {
        for (uint i = 0; i < bindIndices.Length; i++)
        {
            int bindingPoint = bindIndices[i].bindingPoint;

            int idToBind = bindIndices[i].bindId;
            idToCurrentBind[idToBind] = bindingPoint;
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, idToHandle[idToBind]);
        }
    }

    /// <summary>
    /// Disposed when the client is closing, or when shaders are reloaded.
    /// </summary>
    public static void Dispose()
    {
        SsboNameToId.Clear();
        Array.Clear(idToHandle);
        Array.Clear(idToCurrentBind);
    }
}