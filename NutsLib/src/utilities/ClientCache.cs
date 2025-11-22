using System.Collections.Generic;
using System.Threading;

namespace NutsLib;

/// <summary>
/// Cache for things like textures and meshes.
/// Cleared on world shutting down.
/// </summary>
public static class ClientCache
{
    private static readonly ReaderWriterLockSlim slimLock = new();
    private static readonly Dictionary<string, object> cache = [];

    /// <summary>
    /// Get or create a cached value, such as a texture.
    /// Any IDisposable objects will be disposed at disposal.
    /// </summary>
    public static T GetOrCache<T>(string key, Func<T> value)
    {
        slimLock.EnterUpgradeableReadLock();

        if (!cache.TryGetValue(key, out object? obj))
        {
            obj = value();

            slimLock.EnterWriteLock();
            cache.Add(key, obj!);
            slimLock.ExitWriteLock();
        }

        slimLock.ExitUpgradeableReadLock();

        return (T)obj!;
    }

    /// <summary>
    /// Dispose on main thread when closing.
    /// </summary>
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