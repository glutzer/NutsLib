using System.Drawing;

namespace RectpackSharp;

/// <summary>
/// A rectangle that can be used for a rectangle packing operation.
/// </summary>
public struct PackingRectangle : IEquatable<PackingRectangle>, IComparable<PackingRectangle>
{
    /// <summary>
    /// A value that can be used to identify this <see cref="PackingRectangle"/>. This value is
    /// never touched by the rectangle packing algorithm.
    /// </summary>
    public int Id;

    /// <summary>A value used internally by the packing algorithm for sorting rectangles.</summary>
    public uint SortKey;

    /// <summary>The X coordinate of the left edge of this <see cref="PackingRectangle"/>.</summary>
    public uint X;

    /// <summary>The Y coordinate of the top edge of this <see cref="PackingRectangle"/>.</summary>
    public uint Y;

    /// <summary>The width of this <see cref="PackingRectangle"/>.</summary>
    public uint Width;

    /// <summary>The height of this <see cref="PackingRectangle"/>.</summary>
    public uint Height;

    /// <summary>
    /// Gets or sets the X coordinate of the right edge of this <see cref="PackingRectangle"/>.
    /// </summary>
    /// <remarks>Setting this will only modify the <see cref="Width"/>.</remarks>
    public uint Right
    {
        readonly get => X + Width;
        set => Width = value - X;
    }

    /// <summary>
    /// Gets or sets the Y coordinate of the bottom edge of this <see cref="PackingRectangle"/>.
    /// </summary>
    /// <remarks>Setting this will only modify the <see cref="Height"/>.</remarks>
    public uint Bottom
    {
        readonly get => Y + Height;
        set => Height = value - Y;
    }

    /// <summary>Calculates this <see cref="PackingRectangle"/>'s area.</summary>
    public readonly uint Area => Width * Height;

    /// <summary>Calculates this <see cref="PackingRectangle"/>'s perimeter.</summary>
    public readonly uint Perimeter => Width + Width + Height + Height;

    /// <summary>Gets this <see cref="PackingRectangle"/>'s bigger side.</summary>
    public readonly uint BiggerSide => Math.Max(Width, Height);

    /// <summary>Calculates this <see cref="PackingRectangle"/>'s pathological multiplier.</summary>
    /// <remarks>This is calculated as: <code>max(width, height) / min(width, height) * width * height</code></remarks>
    public readonly uint PathologicalMultiplier => (Width > Height ? (Width / Height) : (Height / Width)) * Width * Height;

    /// <summary>
    /// Creates a <see cref="PackingRectangle"/> with the specified values.
    /// </summary>
    public PackingRectangle(uint x, uint y, uint width, uint height, int id = 0)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Id = id;
        SortKey = 0;
    }

    /// <summary>
    /// Creates a <see cref="PackingRectangle"/> from a <see cref="Rectangle"/>.
    /// </summary>
    public PackingRectangle(Rectangle rectangle, int id = 0)
        : this((uint)rectangle.X, (uint)rectangle.Y, (uint)rectangle.Width, (uint)rectangle.Height, id)
    {

    }

    public static implicit operator Rectangle(PackingRectangle rectangle)
    {
        return new Rectangle((int)rectangle.X, (int)rectangle.Y, (int)rectangle.Width, (int)rectangle.Height);
    }

    public static implicit operator PackingRectangle(Rectangle rectangle)
    {
        return new PackingRectangle((uint)rectangle.X, (uint)rectangle.Y, (uint)rectangle.Width, (uint)rectangle.Height);
    }

    public static bool operator ==(PackingRectangle left, PackingRectangle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PackingRectangle left, PackingRectangle right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Returns whether the given <see cref="PackingRectangle"/> is contained
    /// entirely within this <see cref="PackingRectangle"/>.
    /// </summary>
    public readonly bool Contains(in PackingRectangle other)
    {
        return X <= other.X && Y <= other.Y && Right >= other.Right && Bottom >= other.Bottom;
    }

    /// <summary>
    /// Returns whether the given <see cref="PackingRectangle"/> intersects with
    /// this <see cref="PackingRectangle"/>.
    /// </summary>
    public readonly bool Intersects(in PackingRectangle other)
    {
        return other.X < X + Width && X < (other.X + other.Width)
            && other.Y < Y + Height && Y < other.Y + other.Height;
    }

    /// <summary>
    /// Calculates the intersection of this <see cref="PackingRectangle"/> with another.
    /// </summary>
    public readonly PackingRectangle Intersection(in PackingRectangle other)
    {
        uint x1 = Math.Max(X, other.X);
        uint x2 = Math.Min(Right, other.Right);
        uint y1 = Math.Max(Y, other.Y);
        uint y2 = Math.Min(Bottom, other.Bottom);

        return x2 >= x1 && y2 >= y1 ? new PackingRectangle(x1, y1, x2 - x1, y2 - y1) : default;
    }

    public override readonly string ToString()
    {
        return string.Concat("{ X=", X.ToString(), ", Y=", Y.ToString(), ", Width=", Width.ToString() + ", Height=", Height.ToString(), ", Id=", Id.ToString(), " }");
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height, Id);
    }

    public readonly bool Equals(PackingRectangle other)
    {
        return X == other.X && Y == other.Y && Width == other.Width
            && Height == other.Height && Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        return obj is PackingRectangle viewport && Equals(viewport);
    }

    /// <summary>
    /// Compares this <see cref="SortKey"/> with another <see cref="PackingRectangle"/>'s.
    /// </summary>
    public readonly int CompareTo(PackingRectangle other)
    {
        return -SortKey.CompareTo(other.SortKey);
    }
}