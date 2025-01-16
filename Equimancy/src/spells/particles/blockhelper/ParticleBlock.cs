using MareLib;
using Vintagestory.API.Common;

namespace Equimancy;

[Block]
public class ParticleBlock : Block
{
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
    {
        if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is ParticleBlockEntity be)
        {
            be.gui = new(be.currentConfig, be);
            be.gui.TryOpen();
        }

        return false;
    }
}