using Vintagestory.API.MathTools;

namespace Equimancy;

public interface IPipeConnectable
{
    public bool CanConnectTo(BlockPos pos);
}