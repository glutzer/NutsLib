using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Util;

namespace NutsLib;

/// <summary>
/// Used to store attributes for syncing.
/// </summary>
public class AttributeTree
{
    private Dictionary<string, object> keyValues = [];
    private readonly HashSet<string> dirtyPaths = [];

    public AttributeTree()
    {

    }

    /// <summary>
    /// Get a value as a type.
    /// </summary>
    public T? Get<T>(string key)
    {
        return !keyValues.TryGetValue(key, out object? value) ? default : value is T tValue ? tValue : default;
    }

    /// <summary>
    /// Get a value as a type.
    /// </summary>
    public bool TryGet<T>(string key, [NotNullWhen(true)] out T? value)
    {
        if (!keyValues.TryGetValue(key, out object? obj))
        {
            value = default;
            return false;
        }

        if (obj is T tValue)
        {
            value = tValue;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Set a value.
    /// </summary>
    public void Set<T>(string key, T value)
    {
        keyValues[key] = value!;
        dirtyPaths.Add(key);
    }

    /// <summary>
    /// Remove a value.
    /// </summary>
    public void Remove(string key)
    {
        keyValues.Remove(key);
        dirtyPaths.Add(key);
    }

    public byte[] ToBytes()
    {
        string json = JsonConvert.SerializeObject(keyValues);
        return SerializerUtil.Serialize(json);
    }

    public static AttributeTree FromBytes(byte[] bytes)
    {
        AttributeTree newTree = new();
        newTree.LoadBytes(bytes);
        return newTree;
    }

    public void LoadBytes(byte[] bytes)
    {
        string json = SerializerUtil.Deserialize<string>(bytes);
        keyValues = JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? [];
    }
}