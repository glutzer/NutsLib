using System;

namespace NutsLib;

public enum EnumBlockFacing
{
    North, // -Z.
    East, // +X.
    South, // +Z.
    West, // -X.
    Up, // +Y.
    Down // -Y.
}

[Flags]
public enum EnumFaceFlags
{
    None = 0,
    North = 1 << EnumBlockFacing.North,
    East = 1 << EnumBlockFacing.East,
    South = 1 << EnumBlockFacing.South,
    West = 1 << EnumBlockFacing.West,
    Up = 1 << EnumBlockFacing.Up,
    Down = 1 << EnumBlockFacing.Down
}