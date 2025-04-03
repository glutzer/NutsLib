namespace MareLib;

/// <summary>
/// Faster lookup than typeof(T).Name.
/// Use for any type dictionary.
/// </summary>
public static class InnerClass<T>
{
    /// <summary>
    /// Name for use in dictionaries.
    /// </summary>
    public static readonly string Name = typeof(T).Name;

    /// <summary>
    /// Id for use in dictionaries.
    /// </summary>
    public static readonly int Id = InnerClassIncrementer.CurrentId++;
}

public static class InnerClassIncrementer
{
    public static int CurrentId { get; set; } = 0;
}