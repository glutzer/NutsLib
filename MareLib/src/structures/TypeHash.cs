using System;

namespace MareLib;

public readonly struct TypeHash : IEquatable<TypeHash>
{
    private readonly int hash;
    private readonly Type type;

    public TypeHash(object[] parameters, Type type)
    {
        this.type = type;

        hash = 17;
        foreach (object param in parameters)
        {
            hash = hash * 31 + param.GetHashCode();
        }
    }

    public override readonly int GetHashCode()
    {
        return hash;
    }

    public readonly bool Equals(TypeHash other)
    {
        if (other.type != type) return false;

        return hash == other.hash;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is TypeHash typeHash && Equals(typeHash);
    }

    public static bool operator ==(TypeHash left, TypeHash right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TypeHash left, TypeHash right)
    {
        return !(left == right);
    }
}