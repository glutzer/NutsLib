using System;
using System.Collections.Generic;

namespace MareLib;

/// <summary>
/// Cache for things like textures and meshes.
/// Cleared on world shutting down.
/// </summary>
public static class ClientCache
{
    private static readonly Dictionary<string, object> cache = new();

    /// <summary>
    /// Get or create a cached value, such as a texture.
    /// Any IDisposable objects will be disposed at disposal.
    /// </summary>
    public static T GetOrCache<T>(string key, Func<T> value)
    {
        if (!cache.TryGetValue(key, out object? obj))
        {
            obj = value();
            cache.Add(key, obj!);
        }

        return (T)obj!;
    }

    public static void Dispose()
    {
        foreach (object obj in cache.Values)
        {
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        cache.Clear();
    }
}