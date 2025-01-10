namespace MareLib;

/// <summary>
/// Base fluid stack class.
/// </summary>
public class FluidStack
{
    private int units;
    public readonly Fluid fluid;

    /// <summary>
    /// Stack volume, limited by container.
    /// </summary>
    public int Units
    {
        get => units;
        set
        {
            units = value;
        }
    }

    public FluidStack(Fluid fluid, int units)
    {
        this.fluid = fluid;
        this.units = units;
    }
}