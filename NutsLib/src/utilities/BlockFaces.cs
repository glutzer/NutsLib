using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace NutsLib;

public static class BlockFaces
{
    /// <summary>
    /// Iterate every block around a position.
    /// </summary>
    public static void IterateBlocksAtFaces(BlockPos pos, IBlockAccessor blockAccessor, Action<EnumBlockFacing, Block, BlockPos> action)
    {
        BlockPos newPos = pos.Copy();

        newPos.Z--;
        action(EnumBlockFacing.North, blockAccessor.GetBlock(newPos), newPos);

        newPos.Z += 2;
        action(EnumBlockFacing.South, blockAccessor.GetBlock(newPos), newPos);

        newPos.Z--;

        newPos.X--;
        action(EnumBlockFacing.West, blockAccessor.GetBlock(newPos), newPos);

        newPos.X += 2;
        action(EnumBlockFacing.East, blockAccessor.GetBlock(newPos), newPos);

        newPos.X--;

        newPos.Y--;
        action(EnumBlockFacing.Down, blockAccessor.GetBlock(newPos), newPos);
        newPos.Y += 2;

        action(EnumBlockFacing.Up, blockAccessor.GetBlock(newPos), newPos);
    }

    public static void ForEachFace(Action<EnumBlockFacing> action)
    {
        action(EnumBlockFacing.North);
        action(EnumBlockFacing.East);
        action(EnumBlockFacing.South);
        action(EnumBlockFacing.West);
        action(EnumBlockFacing.Up);
        action(EnumBlockFacing.Down);
    }

    public static void ForEachPerpendicularFace(EnumBlockFacing face, Action<EnumBlockFacing> action)
    {
        switch (face)
        {
            case EnumBlockFacing.North:
            case EnumBlockFacing.South:
                action(EnumBlockFacing.East);
                action(EnumBlockFacing.West);
                action(EnumBlockFacing.Up);
                action(EnumBlockFacing.Down);
                break;
            case EnumBlockFacing.East:
            case EnumBlockFacing.West:
                action(EnumBlockFacing.North);
                action(EnumBlockFacing.South);
                action(EnumBlockFacing.Up);
                action(EnumBlockFacing.Down);
                break;
            case EnumBlockFacing.Up:
            case EnumBlockFacing.Down:
                action(EnumBlockFacing.North);
                action(EnumBlockFacing.South);
                action(EnumBlockFacing.East);
                action(EnumBlockFacing.West);
                break;
        }
    }

    public static GridPos GetFaceOffset(EnumBlockFacing face)
    {
        return face switch
        {
            EnumBlockFacing.North => new GridPos(0, 0, -1),
            EnumBlockFacing.East => new GridPos(1, 0, 0),
            EnumBlockFacing.South => new GridPos(0, 0, 1),
            EnumBlockFacing.West => new GridPos(-1, 0, 0),
            EnumBlockFacing.Up => new GridPos(0, 1, 0),
            EnumBlockFacing.Down => new GridPos(0, -1, 0),
            _ => new GridPos(0, 0, 0)
        };
    }
}