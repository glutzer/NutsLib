using System;
using System.Text.Json.Nodes;
using Vintagestory.API.Config;

namespace MareLib;

/// <summary>
/// This is the singleton of a fluid, similar to the singleton of a block or item.
/// </summary>
public class Fluid
{
    /// <summary>
    /// Which fluid stack class will be used for this.
    /// Must extend FluidStack.
    /// </summary>
    protected virtual Type StackType => typeof(FluidStack);

    public readonly string code;
    public readonly int id;

#pragma warning disable IDE0060 // Remove unused parameter.
    public Fluid(FluidJson fluidJson, JsonObject jsonObject, int id)
#pragma warning restore IDE0060 // Remove unused parameter.
    {
        code = fluidJson.Code;
        this.id = id;
    }

    public virtual string GetName(FluidStack fluidStack)
    {
        return Lang.Get(code);
    }

    /// <summary>
    /// Creates a fluid stack of this fluid with X units.
    /// </summary>
    public virtual FluidStack CreateFluidStack(int units)
    {
        FluidStack fluidStack = (FluidStack)Activator.CreateInstance(StackType, this, units)!;
        return fluidStack;
    }
}