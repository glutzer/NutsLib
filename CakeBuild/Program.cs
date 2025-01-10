using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Clean;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Vintagestory.API.Common;

namespace CakeBuild;

public static class Program
{
    public static string[] ModsToBuild { get; } = { "MareLib" };
    public static bool BuildToModsFolder { get; } = true;

    public static string ModsFolder { get; } = null!;
    public static string CurrentMod { get; private set; } = null!;

    static Program()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);
        ModsFolder = System.IO.Path.Combine(appData, "VintagestoryData/mods");
    }

    public static void Main(string[] args)
    {
        for (int i = 0; i < ModsToBuild.Length; i++)
        {
            CurrentMod = ModsToBuild[i];
            new CakeHost()
                .UseContext<BuildContext>()
                .Run(args);
        }
    }
}

public class BuildContext : FrostingContext
{
    public string ProjectName = Program.CurrentMod;
    public bool BuildToModsFolder = Program.BuildToModsFolder;
    public string BuildConfiguration { get; }
    public string Version { get; }
    public string Name { get; }
    public bool SkipJsonValidation { get; }

    public BuildContext(ICakeContext context) : base(context)
    {
        BuildConfiguration = context.Argument("configuration", "Release");
        SkipJsonValidation = context.Argument("skipJsonValidation", false);
        ModInfo modInfo = context.DeserializeJsonFromFile<ModInfo>($"../{ProjectName}/modinfo.json");
        Version = modInfo.Version;
        Name = modInfo.ModID;
    }
}

[TaskName("ValidateJson")]
public sealed class ValidateJsonTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (context.SkipJsonValidation)
        {
            return;
        }

        FilePathCollection jsonFiles = context.GetFiles($"../{context.ProjectName}/assets/**/*.json");
        foreach (FilePath file in jsonFiles)
        {
            try
            {
                string json = File.ReadAllText(file.FullPath);
                JToken.Parse(json);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Validation failed for JSON file: {file.FullPath}{Environment.NewLine}{ex.Message}", ex);
            }
        }
    }
}

[TaskName("Build")]
[IsDependentOn(typeof(ValidateJsonTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetClean($"../{context.ProjectName}/{context.ProjectName}.csproj",
            new DotNetCleanSettings
            {
                Configuration = context.BuildConfiguration
            });

        context.DotNetPublish($"../{context.ProjectName}/{context.ProjectName}.csproj",
            new DotNetPublishSettings
            {
                Configuration = context.BuildConfiguration
            });
    }
}

[TaskName("Package")]
[IsDependentOn(typeof(BuildTask))]
public sealed class PackageTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.EnsureDirectoryExists("../Releases");
        context.CleanDirectory("../Releases");
        context.EnsureDirectoryExists($"../Releases/{context.Name}");
        context.CopyFiles($"../{context.ProjectName}/bin/{context.BuildConfiguration}/Mods/mod/publish/*", $"../Releases/{context.Name}");
        if (context.DirectoryExists($"../{context.ProjectName}/assets"))
        {
            context.CopyDirectory($"../{context.ProjectName}/assets", $"../Releases/{context.Name}/assets");
        }
        context.CopyFile($"../{context.ProjectName}/modinfo.json", $"../Releases/{context.Name}/modinfo.json");
        if (context.FileExists($"../{context.ProjectName}/modicon.png"))
        {
            context.CopyFile($"../{context.ProjectName}/modicon.png", $"../Releases/{context.Name}/modicon.png");
        }

        if (context.BuildToModsFolder)
        {
            context.Zip($"../Releases/{context.Name}", $"{Program.ModsFolder}/{context.Name}_{context.Version}.zip");
        }
        else
        {
            context.Zip($"../Releases/{context.Name}", $"../Releases/{context.Name}_{context.Version}.zip");
        }
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(PackageTask))]
public class DefaultTask : FrostingTask
{
}