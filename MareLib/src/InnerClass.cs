namespace MareLib;

/// <summary>
/// Faster lookup than typeof(T).Name.
/// Use for any type dictionary.
/// </summary>
public static class InnerClass<T>
{
    public static readonly string Name = typeof(T).Name;
}