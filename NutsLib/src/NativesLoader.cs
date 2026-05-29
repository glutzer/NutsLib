using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Common;

namespace NutsLib;

/// <summary>
/// Provides method to load all supported binaries for current platform.
/// </summary>
public static class NativesLoader
{
    private static readonly Lock nativeLock = new();

    /// <summary>
    /// Maps a normalised library name (e.g. "freetype") to the handle returned
    /// by <see cref="NativeLibrary.TryLoad(string)"/> when the file was loaded
    /// from the mod's native folder. Resolver callbacks can consult this map
    /// instead of re-searching the file system.
    /// </summary>
    public static readonly Dictionary<string, IntPtr> LoadedHandles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Loads all supported binaries for current platform.
    /// </summary>
    public static bool Load(ModSystem mod)
    {
        lock (nativeLock)
        {
            DllLoader loader = DllLoader.Loader();

            string prefix = RuntimeEnv.OS switch
            {
                OS.Windows => "nutswin/",
                OS.Mac => "nutsmac/",
                OS.Linux => "nutslinux/",
                _ => "nutslinux"
            };

            string dllPath = $"{((ModContainer)mod.Mod).FolderPath}/native/{prefix}";
            foreach (string file in Directory.GetFiles(dllPath, "*"))
            {
                string withoutExtension = Path.GetFileNameWithoutExtension(file);

                loader.Load(withoutExtension, mod.Mod);
            }
        }

        return true;
    }

    public static void LoadV2(ModSystem mod)
    {
        lock (nativeLock)
        {
            DllLoader loader = DllLoader.Loader();

            string prefix = RuntimeEnv.OS switch
            {
                OS.Windows => "nutswin/",
                OS.Mac => "nutsmac/",
                OS.Linux => "nutslinux/",
                _ => "nutslinux"
            };

            string dllPath = $"{((ModContainer)mod.Mod).FolderPath}/native/{prefix}";
            foreach (string file in Directory.GetFiles(dllPath, "*"))
            {
                try
                {
                    bool loaded = loader.LoadV2(file, mod.Mod);
                }
                catch
                {

                }
            }
        }
    }
}

/// <summary>
/// Base class for native dll loaders for different platforms.
/// </summary>
internal abstract class DllLoader
{
    /// <summary>
    /// Returns loader for the current platform.
    /// </summary>
    public static DllLoader Loader() => new NativeLibraryLoader();

    protected DllLoader()
    {

    }

    public bool Load(string dllName, Mod mod)
    {
        string suffix = RuntimeEnv.OS switch
        {
            OS.Windows => ".dll",
            OS.Mac => ".dylib",
            OS.Linux => ".so",
            _ => ".so"
        };

        string prefix = RuntimeEnv.OS switch
        {
            OS.Windows => "nutswin/",
            OS.Mac => "nutsmac/",
            OS.Linux => "nutslinux/",
            _ => "nutslinux"
        };

        string dllPath = $"{((ModContainer)mod).FolderPath}/native/{prefix}{dllName}{suffix}";

        return Load(dllPath);
    }

    public bool LoadV2(string fullPath, Mod mod)
    {
        return Load(fullPath);
    }

    protected abstract bool Load(string dllPath);
}

internal class NativeLibraryLoader : DllLoader
{
    protected override bool Load(string dllPath)
    {
        if (!NativeLibrary.TryLoad(dllPath, out IntPtr handle))
        {
            Console.WriteLine($"Failed to load binary {dllPath} for some reason.");
            return false;
        }

        // Derive a normalised key: strip "lib" prefix and any extension so that
        // e.g. "libfreetype.so" and "freetype.dll" both map to "freetype".
        string fileName = Path.GetFileNameWithoutExtension(dllPath);
        if (fileName.StartsWith("lib", StringComparison.OrdinalIgnoreCase))
            fileName = fileName[3..];

        NativesLoader.LoadedHandles.TryAdd(fileName, handle);
        return true;
    }
}