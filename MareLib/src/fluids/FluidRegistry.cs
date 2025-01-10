using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Vintagestory.API.Common;

namespace MareLib;

/// <summary>
/// Fluid registry, existing on both the client and the server.
/// </summary>
[GameSystem(0, EnumAppSide.Universal)]
public class FluidRegistry : GameSystem
{
    public Fluid[] Fluids { get; private set; } = new Fluid[512];

    private readonly Dictionary<string, Fluid> fluidsByCode = new();

    private readonly Dictionary<string, Type> fluidTypes = new();

    public FluidRegistry(bool isServer, ICoreAPI api) : base(isServer, api)
    {

    }

    public override void OnAssetsLoaded()
    {
        RegisterFluids();
    }

    public Fluid GetFluid(string code)
    {
        return fluidsByCode[code];
    }

    public bool TryGetFluid(string code, [NotNullWhen(true)] out Fluid? fluid)
    {
        return fluidsByCode.TryGetValue(code, out fluid);
    }

    /// <summary>
    /// Registers all fluid classes, then loads all fluid assets.
    /// </summary>
    public void RegisterFluids()
    {
        // Register all types by their name.
        (Type, FluidAttribute)[] types = AttributeUtilities.GetAllAnnotatedClasses<FluidAttribute>();
        foreach ((Type type, _) in types)
        {
            fluidTypes.Add(type.Name, type);
        }

        // Load all fluids from json.
        List<IAsset> fluidAssets = api.Assets.GetMany("fluidtypes");

        // Incremental id.
        int id = 0;

        foreach (IAsset item in fluidAssets)
        {
            string text = item.ToText();

            // Convert to json with System.Text.Json.
            JsonNode? json = JsonNode.Parse(text);

            if (json is JsonObject jsonObject)
            {
                jsonObject = JsonUtilities.HandleExtends(jsonObject, api);

                JsonUtilities.ForEachVariant(jsonObject, variant =>
                {
                    // Deserialize JsonObject to FluidJson.
                    FluidJson? fluidJson = JsonSerializer.Deserialize<FluidJson>(variant);
                    if (fluidJson == null) return; // Deserialization failed.

                    // Instantiate the fluid.
                    Type type = fluidJson.Class != null ? fluidTypes[fluidJson.Class] : typeof(Fluid);
                    Fluid fluid = (Fluid)Activator.CreateInstance(type, fluidJson, variant, id)!;

                    Fluids[id++] = fluid;
                    fluidsByCode[fluidJson.Code] = fluid;
                });
            }
        }
    }
}