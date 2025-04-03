using MareLib;
using OpenTK.Mathematics;
using System;
using System.Text.Json.Nodes;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Equimancy;

/// <summary>
/// This is the singleton of a fluid, similar to the singleton of a block or item.
/// </summary>
[Fluid]
public class Fluid
{
    /// <summary>
    /// Which fluid stack class will be used for this.
    /// Must extend FluidStack.
    /// StackTypes must use the same constructors.
    /// </summary>
    protected virtual Type StackType => typeof(FluidStack);

    /// <summary>
    /// Creates a fluid stack of this fluid with 0 units.
    /// </summary>
    public virtual FluidStack CreateFluidStack()
    {
        FluidStack fluidStack = (FluidStack)Activator.CreateInstance(StackType, this)!;
        return fluidStack;
    }

    public readonly string code;
    public readonly int id;

    protected readonly float glowLevel;
    protected readonly Vector4 color;

    public readonly ICoreAPI api;

#pragma warning disable IDE0060 // Remove unused parameter.
    public Fluid(FluidJson fluidJson, JsonObject jsonObject, int id, ICoreAPI api)
#pragma warning restore IDE0060 // Remove unused parameter.
    {
        code = fluidJson.Code;
        this.id = id;
        glowLevel = fluidJson.GlowLevel;

        float[] color = fluidJson.Color;
        this.color = new Vector4(color[0], color[1], color[2], color[3]);

        this.api = api;
    }

    public virtual string GetName(FluidStack fluidStack)
    {
        return Lang.Get(code);
    }

    public virtual float GetGlowLevel(FluidStack fluidStack)
    {
        return glowLevel;
    }

    public virtual Vector4 GetColor(FluidStack fluidStack)
    {
        return color;
    }
}