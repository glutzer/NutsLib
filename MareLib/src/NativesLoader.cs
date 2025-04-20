using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.Common;

namespace MareLib;

/// <summary>
/// Provides method to load all supported binaries for current platform.
/// https://github.com/maltiez2/VintageStory_ImGui/blob/master/VSImGui/source/Utils/NativesLoader.cs
/// </summary>
internal static class NativesLoader
{
    /// <summary>
    /// Loads all supported binaries for current platform.
    /// </summary>
    public static bool Load(ModSystem mod)
    {
        DllLoader loader = DllLoader.Loader();

        foreach (string library in nativeLibraries)
        {
            if (!loader.Load(library, mod.Mod)) return false;
        }

        return true;
    }

    private static readonly HashSet<string> nativeLibraries = new()
    {
        "freetype"
    };
}

/// <summary>
/// Base class for native dll loaders for different platforms.
/// </summary>
internal abstract class DllLoader
{
    /// <summary>
    /// Returns loader for the current platform.
    /// </summary>
    public static DllLoader Loader()
    {
        return RuntimeEnv.OS switch
        {
            OS.Windows => new WindowsDllLoader(),
            OS.Mac => new MacDllLoader(),
            OS.Linux => new LinuxDllLoader(),
            _ => new WindowsDllLoader()
        };
    }

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
            OS.Windows => "win/",
            OS.Mac => "mac/",
            OS.Linux => "linux/",
            _ => "linux"
        };

        string dllPath = $"{((ModContainer)mod).FolderPath}/native/{prefix}{dllName}{suffix}";

        return Load(dllPath);
    }

    protected abstract bool Load(string dllPath);
}

internal partial class WindowsDllLoader : DllLoader
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr LoadLibrary(string fileName);

    [DllImport("kernel32")]
    private static extern uint GetLastError();

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    private static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, [Out] StringBuilder lpBuffer, uint nSize, IntPtr[] Arguments);

    protected override bool Load(string dllPath)
    {
        IntPtr handle = LoadLibrary(dllPath);

        if (handle == IntPtr.Zero)
        {
            return false;
        }

        return true;
    }
}

internal partial class LinuxDllLoader : DllLoader
{
    [DllImport("libdl.so.2", CharSet = CharSet.Unicode)]
    static extern IntPtr dlopen(string fileName, int flags);

    [DllImport("libdl.so.2")]
    private static extern IntPtr dlerror();

    protected override bool Load(string dllPath)
    {
        IntPtr? handle = dlopen(dllPath, 1);

        if (handle == IntPtr.Zero)
        {
            return false;
        }

        return true;
    }
}

internal class MacDllLoader : DllLoader
{
    [DllImport("libdl.dylib", EntryPoint = "dlopen", CharSet = CharSet.Unicode)]
    private static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libdl.dylib")]
    private static extern IntPtr dlerror();

    protected override bool Load(string dllPath)
    {
        IntPtr? handle = dlopen(dllPath, 1);

        if (handle == IntPtr.Zero)
        {
            return false;
        }

        return true;
    }
}