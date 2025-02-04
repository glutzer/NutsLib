using ProtoBuf;
using System;
using Vintagestory.API.MathTools;

namespace MareLib;

/// <summary>
/// Struct for fast block positions.
/// </summary>
[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public struct GridPos : IEquatable<GridPos>
{
    public int X;
    public int Y;
    public int Z;

    public GridPos(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public GridPos(BlockPos blockPos)
    {
        X = blockPos.X;
        Y = blockPos.Y;
        Z = blockPos.Z;
    }

    public readonly BlockPos AsBlockPos => new(X, Y, Z);

    public readonly bool Equals(GridPos other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is GridPos i && Equals(i);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public static bool operator ==(GridPos left, GridPos right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GridPos left, GridPos right)
    {
        return !(left == right);
    }

    public static GridPos operator +(GridPos left, GridPos right)
    {
        return new GridPos(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    }

    public static GridPos operator -(GridPos left, GridPos right)
    {
        return new GridPos(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    }

    public static GridPos operator *(GridPos left, int right)
    {
        return new GridPos(left.X * right, left.Y * right, left.Z * right);
    }

    public static GridPos operator /(GridPos left, int right)
    {
        return new GridPos(left.X / right, left.Y / right, left.Z / right);
    }

    public readonly GridPos OffsetByFace(EnumBlockFacing face)
    {
        return face switch
        {
            EnumBlockFacing.North => new GridPos(X, Y, Z - 1),
            EnumBlockFacing.South => new GridPos(X, Y, Z + 1),
            EnumBlockFacing.West => new GridPos(X - 1, Y, Z),
            EnumBlockFacing.East => new GridPos(X + 1, Y, Z),
            EnumBlockFacing.Up => new GridPos(X, Y + 1, Z),
            EnumBlockFacing.Down => new GridPos(X, Y - 1, Z),
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };
    }
}