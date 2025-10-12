using Vintagestory.API.Common;

namespace NutsLib;

/// <summary>
/// Registers a class as a game system.
/// Must extend GameSystem.
/// </summary>
public class GameSystemAttribute : ClassAttribute
{
    public double loadOrder;
    public EnumAppSide forSide;

    public GameSystemAttribute(double loadOrder = 0, EnumAppSide forSide = EnumAppSide.Universal)
    {
        this.loadOrder = loadOrder;
        this.forSide = forSide;
    }
}